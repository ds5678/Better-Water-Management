using Harmony;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(ConditionTableManager), "GetDisplayNameWithCondition")]
    public class ConditionTableManager_GetDisplayNameWithCondition
    {
        public static void Postfix(GearItem gearItem, ref string __result)
        {
            LiquidItem liquidItem = gearItem.m_LiquidItem;
            if (!liquidItem || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
            {
                return;
            }

            if (liquidItem.m_LiquidLiters == 0)
            {
                __result += " - " + Localization.Get("GAMEPLAY_Empty");
            }
            else if (liquidItem.m_LiquidQuality == LiquidQuality.Potable)
            {
                __result += " - " + Localization.Get("GAMEPLAY_WaterPotable");
            }
            else
            {
                __result += " - " + Localization.Get("GAMEPLAY_WaterUnsafe");
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), "RemoveGear")]
    public class Inventory_RemoveGear
    {
        public static void Postfix(GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                Water.AdjustWaterSupplyToWater();
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddGear")]
    public class Inventory_AddGear
    {
        private const GearLiquidTypeEnum ModWater = (GearLiquidTypeEnum)1000;

        public static void Postfix(GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == ModWater)
            {
                liquidItem.m_LiquidType = GearLiquidTypeEnum.Water;
                Water.AdjustWaterSupplyToWater();
            }
        }

        public static void Prefix(Inventory __instance, GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                liquidItem.m_LiquidType = ModWater;
            }
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
            Object.Destroy(waterSupply);
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
            }

            waterSupply.m_VolumeInLiters = liquidItem.m_LiquidLiters;
            waterSupply.m_WaterQuality = liquidItem.m_LiquidQuality;
            waterSupply.m_TimeToDrinkSeconds = liquidItem.m_TimeToDrinkSeconds;
            waterSupply.m_DrinkingAudio = liquidItem.m_DrinkingAudio;

            gi.m_WaterSupply = waterSupply;
        }
    }
}