using Harmony;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(ConditionTableManager), "GetDisplayNameWithCondition")]
    internal class ConditionTableManager_GetDisplayNameWithCondition
    {
        internal static void Postfix(GearItem gearItem, ref string __result)
        {
            LiquidItem liquidItem = gearItem.m_LiquidItem;
            if (!liquidItem || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
            {
                return;
            }

            if (Water.IsEmpty(liquidItem))
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

    [HarmonyPatch(typeof(GearItem), "GetItemWeightIgnoreClothingWornBonusKG")]
    internal class GearItem_GetItemWeightIgnoreClothingWornBonusKG
    {
        internal static bool Prefix(GearItem __instance, ref float __result)
        {
            if (GameManager.GetInventoryComponent().GetPotableWaterSupply() == __instance ||
                GameManager.GetInventoryComponent().GetNonPotableWaterSupply() == __instance)
            {
                __result = 0;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(GearItem), "GetItemWeightKG")]
    internal class GearItem_GetItemWeightKG
    {
        internal static bool Prefix(GearItem __instance, ref float __result)
        {
            if (GameManager.GetInventoryComponent().GetPotableWaterSupply() == __instance ||
                GameManager.GetInventoryComponent().GetNonPotableWaterSupply() == __instance)
            {
                __result = 0;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(GearItem), "ManualStart")]
    internal class GearItem_ManualStart
    {
        internal static void Postfix(GearItem __instance)
        {
            LiquidItem liquidItem = __instance.m_LiquidItem;
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                WaterUtils.UpdateWaterBottle(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddGear")]
    internal class Inventory_AddGear
    {
        private const GearLiquidTypeEnum ModWater = (GearLiquidTypeEnum)1000;

        internal static void Postfix(GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == ModWater)
            {
                liquidItem.m_LiquidType = GearLiquidTypeEnum.Water;
                Water.AdjustWaterSupplyToWater();
            }
        }

        internal static void Prefix(Inventory __instance, GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                liquidItem.m_LiquidType = ModWater;
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddToPotableWaterSupply")]
    internal class Inventory_AddToPotableWaterSupply
    {
        internal static void Postfix(float volumeLiters)
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddToWaterSupply")]
    internal class Inventory_AddToWaterSupply
    {
        internal static void Postfix(float numLiters, LiquidQuality quality)
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Inventory), "RemoveGear")]
    internal class Inventory_RemoveGear
    {
        internal static void Postfix(GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                Water.AdjustWaterSupplyToWater();
            }
        }
    }

    [HarmonyPatch(typeof(Panel_Container), "IgnoreWaterSupplyItem")]
    internal class Panel_Container_IgnoreWaterSupplyItem
    {
        internal static bool Prefix(WaterSupply ws, ref bool __result)
        {
            __result = ws != null && ws.GetComponent<LiquidItem>() == null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Panel_Inventory), "IgnoreWaterSupplyItem")]
    internal class Panel_Inventory_IgnoreWaterSupplyItem
    {
        internal static bool Prefix(WaterSupply ws, ref bool __result)
        {
            __result = ws != null && ws.GetComponent<LiquidItem>() == null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "Enable")]
    internal class Panel_PickWater_Enable
    {
        internal static void Postfix(Panel_PickWater __instance)
        {
            PickWater.UpdateDrinking(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "Refresh")]
    internal class Panel_PickWater_Refresh
    {
        internal static void Prefix(Panel_PickWater __instance)
        {
            PickWater.ClampAmount(__instance);
        }

        internal static void Postfix(Panel_PickWater __instance)
        {
            PickWater.UpdateButtons(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "OnTakeWaterComplete")]
    internal class Panel_PickWater_OnTakeWaterComplete
    {
        internal static void Postfix()
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "SetWaterSourceForTaking")]
    internal class Panel_PickWater_SetWaterSourceForTaking
    {
        internal static void Postfix(Panel_PickWater __instance)
        {
            PickWater.UpdateCapacityInfo(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "Start")]
    internal class Panel_PickWater_Start
    {
        internal static void Postfix(Panel_PickWater __instance)
        {
            PickWater.Prepare(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "Update")]
    internal class Panel_PickWater_Update
    {
        internal static void Postfix(Panel_PickWater __instance)
        {
            if (InputManager.GetEquipPressed())
            {
                Traverse traverse = Traverse.Create(__instance);

                GameObject gameObject = new GameObject();
                GearItem gearItem = gameObject.AddComponent<GearItem>();
                gearItem.m_LocalizedDisplayName = new LocalizedString { m_LocalizationID = "" };

                WaterSourceSupply waterSourceSupply = gameObject.AddComponent<WaterSourceSupply>();
                waterSourceSupply.SetWaterSource(traverse.Field("m_WaterSource").GetValue<WaterSource>());

                gearItem.Awake();

                traverse.Method("ExitInterface").GetValue();

                GameManager.GetPlayerManagerComponent().UseInventoryItem(gearItem);
            }
        }
    }

    [HarmonyPatch(typeof(Utils), "GetInventoryIconTexture")]
    internal class Utils_GetInventoryIconTexture
    {
        internal static bool Prefix(GearItem gi, ref Texture2D __result)
        {
            LiquidItem liquidItem = gi.m_LiquidItem;
            if (!liquidItem || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
            {
                return true;
            }

            string textureName = gi.name.Replace("GEAR_", "ico_GearItem__") + WaterUtils.GetWaterSuffix(liquidItem);
            __result = Utils.GetInventoryIconTextureFromName(textureName);

            return __result == null;
        }
    }

    internal class WaterSourceSupply : WaterSupply
    {
        internal WaterSource waterSource;

        internal void SetWaterSource(WaterSource waterSource)
        {
            this.waterSource = waterSource;
            this.m_VolumeInLiters = waterSource.GetVolumeLiters();
            this.m_WaterQuality = waterSource.GetQuality();
            this.m_DrinkingAudio = "Play_Slurping1";
            this.m_TimeToDrinkSeconds = 4;
        }

        internal void UpdateWaterSource()
        {
            waterSource.RemoveLiters(waterSource.GetVolumeLiters() - this.m_VolumeInLiters);
        }
    }
}