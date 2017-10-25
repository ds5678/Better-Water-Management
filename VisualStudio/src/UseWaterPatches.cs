using Harmony;
using System.Collections.Generic;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(Panel_ActionsRadial), "GetDrinkItemsInInventory")]
    public class Panel_ActionsRadial_GetDrinkItemsInInventory
    {
        public static bool Prefix(Panel_ActionsRadial __instance, ref List<GearItem> __result)
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
    public class Panel_Inventory_CanBeAddedToSatchel
    {
        public static bool Prefix(GearItem gi, ref bool __result)
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

    [HarmonyPatch(typeof(PlayerManager), "OnDrinkWaterComplete")]
    public class PlayerManager_OnDrinkWaterComplete
    {
        public static void Postfix(PlayerManager __instance)
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

    [HarmonyPatch(typeof(PlayerManager), "UseInventoryItem")]
    public class PlayerManager_UseInventoryItem
    {
        public static void Prefix(GearItem gi, ref bool __result)
        {
            if (gi == null)
            {
                return;
            }

            LiquidItem liquidItem = gi.m_LiquidItem;
            if (liquidItem == null || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
            {
                return;
            }

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

    [HarmonyPatch(typeof(GameManager), "Start")]
    public class GameManager_Start
    {
        public static void Postfix()
        {
            Water.AdjustWaterSupplyToWater();
        }
    }
}