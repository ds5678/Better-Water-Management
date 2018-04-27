using Harmony;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(GameManager), "Start")]
    internal class GameManager_Start
    {
        internal static void Postfix()
        {
            Water.AdjustWaterSupplyToWater();
        }
    }

    [HarmonyPatch(typeof(ItemDescriptionPage), "GetEquipButtonLocalizationId")]
    internal class ItemDescriptionPageGetEquipButtonLocalizationIdPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldc_R4)
                {
                    continue;
                }

                var operand = codes[i].operand;
                if (!(operand is float))
                {
                    continue;
                }

                float value = (float)operand;
                if (value == 0.01f)
                {
                    codes[i].operand = Water.MIN_AMOUNT;
                }
            }

            return codes;
        }
    }

    [HarmonyPatch(typeof(Panel_ActionsRadial), "GetDrinkItemsInInventory")]
    internal class Panel_ActionsRadial_GetDrinkItemsInInventory
    {
        internal static bool Prefix(Panel_ActionsRadial __instance, ref List<GearItem> __result)
        {
            __result = new List<GearItem>();

            for (int index = 0; index < GameManager.GetInventoryComponent().m_Items.Count; ++index)
            {
                GearItem component = GameManager.GetInventoryComponent().m_Items[index].GetComponent<GearItem>();
                if (component.m_FoodItem != null && component.m_FoodItem.m_IsDrink)
                {
                    if (component.m_IsInSatchel)
                    {
                        __result.Insert(0, component);
                    }
                    else
                    {
                        __result.Add(component);
                    }
                }

                if (WaterUtils.ContainsWater(component))
                {
                    if (component.m_IsInSatchel)
                    {
                        __result.Insert(0, component);
                    }
                    else
                    {
                        __result.Add(component);
                    }
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Panel_Inventory), "CanBeAddedToSatchel")]
    internal class Panel_Inventory_CanBeAddedToSatchel
    {
        internal static bool Prefix(GearItem gi, ref bool __result)
        {
            if (gi.m_DisableFavoriting)
            {
                return false;
            }

            if (WaterUtils.ContainsWater(gi))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "DrinkFromWaterSupply")]
    internal class PlayerManager_DrinkFromWaterSupply
    {
        internal static void Postfix(WaterSupply ws, bool __result)
        {
            if (GameManager.GetThirstComponent().IsAddingThirstOverTime())
            {
                return;
            }

            LiquidItem liquidItem = ws.GetComponent<LiquidItem>();
            if (liquidItem == null)
            {
                return;
            }

            liquidItem.m_LiquidLiters = ws.m_VolumeInLiters;
            Object.Destroy(ws);
            liquidItem.GetComponent<GearItem>().m_WaterSupply = null;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "OnDrinkWaterComplete")]
    internal class PlayerManager_OnDrinkWaterComplete
    {
        internal static void Postfix(PlayerManager __instance)
        {
            WaterSupply waterSupply = AccessTools.Field(__instance.GetType(), "m_WaterSourceToDrinkFrom").GetValue(__instance) as WaterSupply;
            if (waterSupply == null)
            {
                return;
            }

            LiquidItem liquidItem = waterSupply.GetComponent<LiquidItem>();
            if (liquidItem == null)
            {
                return;
            }

            liquidItem.m_LiquidLiters = waterSupply.m_VolumeInLiters;
            UnityEngine.Object.Destroy(waterSupply);
            liquidItem.GetComponent<GearItem>().m_WaterSupply = null;

            Water.AdjustWaterSupplyToWater();
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "UpdateInspectGear")]
    internal class PlayerManager_UpdateInspectGear
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                CodeInstruction codeInstruction = codeInstructions[i];

                if (codeInstruction.opcode != OpCodes.Callvirt)
                {
                    continue;
                }

                MethodInfo methodInfo = codeInstruction.operand as MethodInfo;
                if (methodInfo == null)
                {
                    continue;
                }

                if ((methodInfo.Name == "GetPotableWaterSupply" || methodInfo.Name == "GetNonPotableWaterSupply") && methodInfo.DeclaringType == typeof(Inventory) && methodInfo.GetParameters().Length == 0)
                {
                    codeInstructions[i - 2].opcode = OpCodes.Ldarg_0;
                    codeInstructions[i - 1].opcode = OpCodes.Ldarg_0;
                    codeInstructions[i].opcode = OpCodes.Ldfld;
                    codeInstructions[i].operand = typeof(PlayerManager).GetField("m_Gear", BindingFlags.Instance | BindingFlags.NonPublic);
                }
            }

            return codeInstructions;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "UseInventoryItem")]
    internal class PlayerManager_UseInventoryItem
    {
        internal static void Prefix(GearItem gi, ref bool __result)
        {
            if (!WaterUtils.IsWaterItem(gi))
            {
                return;
            }

            LiquidItem liquidItem = gi.m_LiquidItem;

            WaterSupply waterSupply = liquidItem.GetComponent<WaterSupply>();
            if (waterSupply == null)
            {
                waterSupply = liquidItem.gameObject.AddComponent<WaterSupply>();
                gi.m_WaterSupply = waterSupply;
            }

            waterSupply.m_VolumeInLiters = liquidItem.m_LiquidLiters;
            waterSupply.m_WaterQuality = liquidItem.m_LiquidQuality;
            waterSupply.m_TimeToDrinkSeconds = liquidItem.m_TimeToDrinkSeconds;
            waterSupply.m_DrinkingAudio = liquidItem.m_DrinkingAudio;
        }
    }
}