using Harmony;
using ModComponentMapper;
using UnityEngine;

namespace BetterWaterManagement
{
    internal static class DrinkFromPot
    {
        internal static CookingPotItem cookingPot;
        internal static GearItem gearItem;
        internal static WaterSupply waterSupply;
        internal static float waterVolumeToDrink;

        internal static void OnDrink()
        {
            float waterAmount = cookingPot.m_LitersWaterBeingBoiled;
            waterVolumeToDrink = GameManager.GetPlayerManagerComponent().CalculateWaterVolumeToDrink(waterAmount);
            gearItem = cookingPot.GetComponent<GearItem>();
            waterSupply = gearItem.m_WaterSupply;
            if (waterSupply == null)
            {
                waterSupply = gearItem.gameObject.AddComponent<WaterSupply>();
                gearItem.m_WaterSupply = waterSupply;
            }
            waterSupply.m_VolumeInLiters = waterAmount;
            bool potable = cookingPot.GetCookingState() == CookingPotItem.CookingState.Ready;
            waterSupply.m_WaterQuality = potable ? LiquidQuality.Potable : LiquidQuality.NonPotable;
            waterSupply.m_TimeToDrinkSeconds = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_TimeToDrinkSeconds;
            waterSupply.m_DrinkingAudio = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_DrinkingAudio;

            GameManager.GetThirstComponent().AddThirstOverTime(waterVolumeToDrink, waterSupply.m_TimeToDrinkSeconds);
            InterfaceManager.m_Panel_GenericProgressBar.Launch(Localization.Get("GAMEPLAY_DrinkingProgress"), waterSupply.m_TimeToDrinkSeconds, 0f, 0f,null,waterSupply.m_DrinkingAudio, true, false, new System.Action<bool, bool, float>(OnDrinkComplete));
            GameManager.GetPlayerVoiceComponent().BlockNonCriticalVoiceForDuration(waterSupply.m_TimeToDrinkSeconds + 2f);
        }
        internal static void OnDrinkComplete(bool success, bool playerCancel, float progress)
        {
            GameManager.GetThirstComponent().ClearAddThirstOverTime();
            cookingPot.m_LitersWaterBeingBoiled -= progress * waterVolumeToDrink;
            Object.Destroy(waterSupply);
            // Enable drinking without taking the remaining water
            gearItem.m_WaterSupply = null;
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "DoSpecialActionFromInspectMode")] //like eating, drinking, or passing time
    internal class CookingPotItem_DoSpecialActionFromInspectMode
    {
        internal static bool Prefix(CookingPotItem __instance)
        {
            float waterAmount = __instance.m_LitersWaterBeingBoiled;
            if (Water.IsNone(waterAmount)) //only applies with water 
            {
                return true;
            }
            float waterVolumeToDrink = GameManager.GetPlayerManagerComponent().CalculateWaterVolumeToDrink(waterAmount);
            if (Water.IsNone(waterVolumeToDrink)) // Not thirsty.
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_Youarenotthirsty"));
                GameAudioManager.PlayGUIError();
                return false;
            }
            bool is_ready = __instance.GetCookingState() == CookingPotItem.CookingState.Ready;
            bool is_not_ready_and_no_fire = __instance.GetCookingState() == CookingPotItem.CookingState.Cooking && !__instance.AttachedFireIsBurning();
            if (is_ready || is_not_ready_and_no_fire) //patch applies if ready or if still cooking but no fire.
            {
                DrinkFromPot.cookingPot = __instance;

                DrinkFromPot.OnDrink();

                if (Water.IsNone(__instance.m_LitersWaterBeingBoiled))
                {
                    return true;
                }
                else
                {
                    GameManager.GetPlayerManagerComponent().ExitInspectGearMode(false);
                    return false;
                }
            }
            return true;
        }
    }
}
