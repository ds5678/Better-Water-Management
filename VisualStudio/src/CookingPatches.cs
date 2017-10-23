using Harmony;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(GearItem), "GetItemWeightKG")]
    public class GearItem_GetItemWeightKG
    {
        public static void Postfix(GearItem __instance, ref float __result)
        {
            if (__instance.m_WaterSupply != null)
            {
                __result -= __instance.m_WaterSupply.m_VolumeInLiters;
            }
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "BoilingWaterCompleted")]
    public class Panel_FeedFire_BoilingWaterCompleted
    {
        public static void Postfix()
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "CookingCompleted")]
    public class Panel_FeedFire_CookingCompleted
    {
        public static void Postfix()
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "Enable")]
    public class Panel_FeedFire_Enable
    {
        public static void Postfix(Panel_FeedFire __instance, bool enable)
        {
            if (enable)
            {
                Cooking.SelectCookingGear();
                Cooking.Configure(__instance);
            }
            else
            {
                Cooking.Restore(__instance);
                Cooking.UnselectCookingGear();
            }
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "MeltingSnowCompleted")]
    public class Panel_FeedFire_MeltingSnowCompleted
    {
        public static void Postfix()
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "Refresh")]
    public class Panel_FeedFire_Refresh
    {
        public static void Postfix(Panel_FeedFire __instance)
        {
            if (__instance.m_TabWater.activeSelf)
            {
                Cooking.RefreshTabWater(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "RefreshSnow")]
    public class Panel_FeedFire_RefreshSnow
    {
        public static void Postfix(Panel_FeedFire __instance)
        {
            Cooking.ClampMeltSnowAmount(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "RefreshWater")]
    public class Panel_FeedFire_RefreshWater
    {
        public static void Postfix(Panel_FeedFire __instance)
        {
            Cooking.ClampBoilWaterAmount(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "Start")]
    public class Panel_FeedFire_Start
    {
        public static void Postfix(Panel_FeedFire __instance)
        {
            Cooking.Prepare(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_FeedFire), "UpdateGearItem")]
    public class Panel_FeedFire_UpdateGearItem
    {
        public static void Postfix(Panel_FeedFire __instance)
        {
            if (__instance.m_TabWater.activeSelf)
            {
                Cooking.ShowCookingGearInInspect(__instance);
            }
            else
            {
                Cooking.HideCookingGearInInspect(__instance);
            }
        }
    }

    //[HarmonyPatch(typeof(Panel_Inventory), "IgnoreWaterSupplyItem")]
    //public class Panel_Inventory_IgnoreWaterSupplyItem
    //{
    //    public static bool Prefix(WaterSupply ws, ref bool __result)
    //    {
    //        __result = ws != null && ws.GetComponent<LiquidItem>() == null;
    //        return false;
    //    }
    //}

    //[HarmonyPatch(typeof(Panel_Container), "IgnoreWaterSupplyItem")]
    //public class Panel_Container_IgnoreWaterSupplyItem
    //{
    //    public static bool Prefix(WaterSupply ws, ref bool __result)
    //    {
    //        __result = ws != null && ws.GetComponent<LiquidItem>() == null;
    //        return false;
    //    }
    //}
}