using Harmony;

namespace BetterWaterManagement
{
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
}