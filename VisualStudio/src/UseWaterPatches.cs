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

    //
    //Changes the minimum water amount to display the "Drink" button
    //
    /*[HarmonyPatch(typeof(ItemDescriptionPage), "GetEquipButtonLocalizationId")] //Transpiler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
    }*/

    [HarmonyPatch(typeof(Panel_ActionsRadial), "GetDrinkItemsInInventory")]
    internal class Panel_ActionsRadial_GetDrinkItemsInInventory
    {
        internal static bool Prefix(Panel_ActionsRadial __instance, ref Il2CppSystem.Collections.Generic.List<GearItem> __result)
        {
            __result = new Il2CppSystem.Collections.Generic.List<GearItem>();

            for (int index = 0; index < GameManager.GetInventoryComponent().m_Items.Count; ++index)
            {
                GearItem component = GameManager.GetInventoryComponent().m_Items[index].m_GearItem;
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

                if (WaterUtils.ContainsPotableWater(component))
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

    [HarmonyPatch(typeof(PlayerManager), "DrinkFromWaterSupply")]//runs when you start drinking water; doesn't run when drinking tea
    internal class PlayerManager_DrinkFromWaterSupply
    {
        internal static void Postfix(WaterSupply ws, bool __result)
        {
            //Implementation.Log("PlayerManager -- DrinkFromWaterSupply");
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
        internal static void Postfix(PlayerManager __instance, float progress)
        {
            //WaterSupply waterSupply = AccessTools.Field(__instance.GetType(), "m_WaterSourceToDrinkFrom").GetValue(__instance) as WaterSupply;
            WaterSupply waterSupply = __instance.m_WaterSourceToDrinkFrom;
            if (waterSupply == null)
            {
                return;
            }

            GearItem gearItem = waterSupply.GetComponent<GearItem>();
            if (gearItem.m_LiquidItem != null)
            {
                gearItem.m_LiquidItem.m_LiquidLiters = waterSupply.m_VolumeInLiters;
                Object.Destroy(waterSupply);
                gearItem.m_WaterSupply = null;
            }

            if (gearItem.m_CookingPotItem != null)
            {
                if (!WaterUtils.IsCooledDown(gearItem.m_CookingPotItem))
                {
                    //GameManager.GetPlayerManagerComponent().ApplyFreezingBuff(20 * progress, 0.5f, 1 * progress);
                    GameManager.GetPlayerManagerComponent().ApplyFreezingBuff(20 * progress, 0.5f, 1 * progress,24f);
                    PlayerDamageEvent.SpawnAfflictionEvent("GAMEPLAY_WarmingUp", "GAMEPLAY_BuffHeader", "ico_injury_warmingUp", InterfaceManager.m_Panel_ActionsRadial.m_FirstAidBuffColor);
                }

                WaterUtils.SetWaterAmount(gearItem.m_CookingPotItem, waterSupply.m_VolumeInLiters);
                Object.Destroy(waterSupply);
            }

            if (waterSupply is WaterSourceSupply)
            {
                WaterSourceSupply waterSourceSupply = waterSupply as WaterSourceSupply;
                waterSourceSupply.UpdateWaterSource();
            }

            Water.AdjustWaterSupplyToWater();
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "OnPurifyWaterComplete")]
    internal class PlayerManager_OnPurifyWaterComplete
    {
        internal static void Postfix()
        {
            //Implementation.Log("PlayerManager -- OnPurifyWaterComplete");
            Water.AdjustWaterToWaterSupply();
        }
    }

    /*[HarmonyPatch(typeof(PlayerManager), "UpdateInspectGear")]// Transpiler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
    }*/

    //Replacement Patches
    
    internal class UpdateInspectGearTracker
    {
        internal static bool isExecuting = false;
    }

    [HarmonyPatch(typeof(PlayerManager),"UpdateInspectGear")]
    internal class PlayerManager_UpdateInspectGear
    {
        private static void Prefix()
        {
            UpdateInspectGearTracker.isExecuting = true;
        }
        private static void Postfix()
        {
            UpdateInspectGearTracker.isExecuting = false;
        }
    }

    /*[HarmonyPatch(typeof(PlayerManager), "UseInventoryItem")]
    internal class PlayerManager_UseInventoryItem
    {
        private static void Prefix(ref GearItem gi,float volumeAvailable)
        {
            if (UpdateInspectGearTracker.isExecuting && volumeAvailable > 0f)
            {
                gi = GameManager.GetPlayerManagerComponent().m_Gear;
            }
        }
    }*/

    /*[HarmonyPatch(typeof(Inventory), "GetPotableWaterSupply")]
    internal class Inventory_GetPotableWaterSupply
    {
        private static bool Prefix(ref GearItem __result)
        {
            if (UpdateInspectGearTracker.isExecuting)
            {
                __result = GameManager.GetPlayerManagerComponent().m_Gear;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), "GetNonPotableWaterSupply")]
    internal class Inventory_GetNonPotableWaterSupply
    {
        private static bool Prefix(ref GearItem __result)
        {
            if (UpdateInspectGearTracker.isExecuting)
            {
                __result = GameManager.GetPlayerManagerComponent().m_Gear;
                return false;
            }
            else
            {
                return true;
            }
        }
    }*/

    //End Replacements

    [HarmonyPatch(typeof(PlayerManager), "UseInventoryItem")]
    internal class PlayerManager_UseInventoryItem
    {
        internal static void Prefix(ref GearItem gi, float volumeAvailable, ref bool __result)
        {
            //Added for replacing transpiler patch:
            //ref to gi
            //float volumeAvailable
            //this if clause
            if (UpdateInspectGearTracker.isExecuting && volumeAvailable > 0f)
            {
                gi = GameManager.GetPlayerManagerComponent().m_Gear;
            }

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