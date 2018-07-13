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

    [HarmonyPatch(typeof(CookingPotItem), "SetCookingState")]
    public class CookingPotItem_SetCookingState
    {
        public static void Prefix(CookingPotItem __instance, ref CookingPotItem.CookingState cookingState)
        {
            if (cookingState == CookingPotItem.CookingState.Ruined && WaterUtils.GetWaterAmount(__instance) > 0)
            {
                cookingState = CookingPotItem.CookingState.Ready;
            }
        }
    }
}