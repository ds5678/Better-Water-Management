using Harmony;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(CookingPotItem), "DoSpecialActionFromInspectMode")]
    public class CookingPotItem_DoSpecialActionFromInspectMode
    {
        internal static bool Prefix(CookingPotItem __instance)
        {
            if (__instance.GetCookingState() == CookingPotItem.CookingState.Cooking)
            {
                return true;
            }

            float waterAmount = WaterUtils.GetWaterAmount(__instance);
            if (waterAmount <= 0)
            {
                return true;
            }

            bool potable = __instance.GetCookingState() == CookingPotItem.CookingState.Ready;

            GearItem gearItem = __instance.GetComponent<GearItem>();

            WaterSupply waterSupply = gearItem.m_WaterSupply;
            if (waterSupply == null)
            {
                waterSupply = gearItem.gameObject.AddComponent<WaterSupply>();
                gearItem.m_WaterSupply = waterSupply;
            }

            waterSupply.m_VolumeInLiters = waterAmount;
            waterSupply.m_WaterQuality = potable ? LiquidQuality.Potable : LiquidQuality.NonPotable;
            waterSupply.m_TimeToDrinkSeconds = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_TimeToDrinkSeconds;
            waterSupply.m_DrinkingAudio = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_DrinkingAudio;

            GameManager.GetPlayerManagerComponent().UseInventoryItem(gearItem);

            return false;
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "ExitPlaceMesh")]
    public class CookingPotItem_ExitPlaceMesh
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            CoolDown coolDown = __instance.gameObject.GetComponent<CoolDown>();
            if (coolDown == null)
            {
                coolDown = __instance.gameObject.AddComponent<CoolDown>();
            }

            coolDown.SetEnabled(!__instance.AttachedFireIsBurning());
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                CodeInstruction codeInstruction = codeInstructions[i];

                if (codeInstruction.opcode != OpCodes.Call)
                {
                    continue;
                }

                MethodInfo methodInfo = codeInstruction.operand as MethodInfo;
                if (methodInfo == null)
                {
                    continue;
                }

                if (methodInfo.Name == "PickUpCookedItem" && methodInfo.DeclaringType == typeof(CookingPotItem))
                {
                    codeInstructions[i - 1].opcode = OpCodes.Nop;
                    codeInstructions[i].opcode = OpCodes.Nop;
                }
            }

            return codeInstructions;
        }
    }

    //[HarmonyPatch(typeof(CookingPotItem), "PickUpCookedItem")]
    public class CookingPotItem_PickUpCookedItem
    {
        public static bool Prefix(CookingPotItem __instance)
        {
            if (!__instance.AttachedFireIsBurning())
            {
                return true;
            }

            float waterAmount = WaterUtils.GetWaterAmount(__instance);
            if (waterAmount <= 0)
            {
                return true;
            }

            float capacity;
            bool potable = __instance.GetCookingState() == CookingPotItem.CookingState.Ready;
            if (potable)
            {
                capacity = Water.WATER.RemainingCapacityPotable;
            }
            else
            {
                capacity = Water.WATER.RemainingCapacityNonPotable;
            }

            if (capacity > waterAmount)
            {
                return true;
            }

            GameManager.GetInventoryComponent().AddToPotableWaterSupply(capacity);
            GearMessage.AddMessage(
                potable ?
                    GameManager.GetInventoryComponent().GetPotableWaterSupply().name :
                    GameManager.GetInventoryComponent().GetNonPotableWaterSupply().name,
                Localization.Get("GAMEPLAY_Added"),
                (potable ?
                    Localization.Get("GAMEPLAY_WaterPotable") :
                    Localization.Get("GAMEPLAY_WaterNonPotable")) + " (" + Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, capacity) + ")",
                false);

            WaterUtils.SetWaterAmount(__instance, waterAmount - capacity);
            return false;
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "UpdateCookingTimeAndState")]
    public class CookingPotItem_UpdateCookingTimeAndState
    {
        public static bool IsCookingFoodItem(CookingPotItem cookingPotItem)
        {
            Debug.Log("IsCookingFoodItem(" + cookingPotItem + ")");
            GearItem gearItem = cookingPotItem.GetGearItemForInspectMode();
            return gearItem != null && gearItem.m_FoodItem != null;
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                CodeInstruction codeInstruction = codeInstructions[i];

                if (codeInstruction.opcode != OpCodes.Ldfld)
                {
                    continue;
                }

                FieldInfo info = codeInstruction.operand as FieldInfo;
                if (info == null)
                {
                    continue;
                }

                if (info.Name == "m_GearItemBeingCooked" && info.DeclaringType == typeof(CookingPotItem))
                {
                    //codeInstructions[i - 1].opcode = OpCodes.Nop;
                    codeInstructions[i].opcode = OpCodes.Call;
                    codeInstructions[i].operand = AccessTools.Method(typeof(CookingPotItem_UpdateCookingTimeAndState), "IsCookingFoodItem");
                    codeInstructions[i + 1].opcode = OpCodes.Nop;
                    codeInstructions[i + 2].opcode = OpCodes.Nop;
                    break;
                }
            }

            return codeInstructions;
        }
    }
}