using UnityEngine;
using Harmony;


namespace BetterWaterManagement
{
    internal class WaterButtons
    {
        internal static string fillText;
        internal static string transferText;
        internal static string dumpText;
        private static GameObject fillButton;
        private static GameObject transferButton;
        private static GameObject dumpButton;
        internal static LiquidItem currentLiquidItemShowing;

        internal static void Initialize(ItemDescriptionPage itemDescriptionPage)
        {
            if (itemDescriptionPage == null) return;

            fillText = Localization.Get("GAMEPLAY_BWM_FillBottle");
            transferText = Localization.Get("GAMEPLAY_BWM_TransferBottle");
            dumpText = Localization.Get("GAMEPLAY_BWM_DumpBottle");

            GameObject drinkButton = itemDescriptionPage.m_MouseButtonEquip;
            fillButton = Object.Instantiate<GameObject>(drinkButton,drinkButton.transform.parent,true);
            fillButton.transform.Translate(0, -0.09f, 0);
            Utils.GetComponentInChildren<UILabel>(fillButton).text = fillText;

            transferButton = Object.Instantiate<GameObject>(drinkButton, drinkButton.transform.parent, true);
            transferButton.transform.Translate(0, -0.18f, 0);
            Utils.GetComponentInChildren<UILabel>(transferButton).text = transferText;

            dumpButton = Object.Instantiate<GameObject>(drinkButton, drinkButton.transform.parent, true);
            dumpButton.transform.Translate(0, -0.27f, 0);
            Utils.GetComponentInChildren<UILabel>(dumpButton).text = dumpText;

            AddAction(fillButton, new System.Action(OnFill));
            AddAction(transferButton, new System.Action(OnTransfer));
            AddAction(dumpButton, new System.Action(OnDump));

            SetActive(true);
        }

        private static void AddAction(GameObject button,System.Action action)
        {
            Il2CppSystem.Collections.Generic.List<EventDelegate> placeHolderList = new Il2CppSystem.Collections.Generic.List<EventDelegate>();
            placeHolderList.Add(new EventDelegate(action));
            Utils.GetComponentInChildren<UIButton>(button).onClick = placeHolderList;
        }

        internal static void SetActive(bool active)
        {
            NGUITools.SetActive(fillButton, active);
            NGUITools.SetActive(transferButton, active);
            NGUITools.SetActive(dumpButton, active);
        }

        private static void OnFill()
        {
            //MelonLoader.MelonLogger.Log("Fill");
            var liquidItem = WaterButtons.currentLiquidItemShowing;
            if (liquidItem == null) return;
            if (liquidItem.m_LiquidLiters >= liquidItem.m_LiquidCapacityLiters)//if the container is already full
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_BWM_AlreadyFull"));
                GameAudioManager.PlayGUIError();
                return;
            }
            if (Water.IsNone(Water.GetActual(liquidItem.m_LiquidQuality))) // If the current water supply is empty.
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_BWM_Empty"));
                GameAudioManager.PlayGUIError();
                return;
            }
            float maxWaterInBottle = Mathf.Min(Water.GetActual(liquidItem.m_LiquidQuality), liquidItem.m_LiquidCapacityLiters);
            float maximumWaterRefill = Mathf.Max(maxWaterInBottle - liquidItem.m_LiquidLiters, 0);
            if (Water.IsNone(maximumWaterRefill)) // If nothing gets transferred.
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_None"));
                GameAudioManager.PlayGUIError();
                return;
            }
            GameAudioManager.PlayGuiConfirm();
            float refuelDuration = Mathf.Max(maximumWaterRefill * 4, 1);
            InterfaceManager.m_Panel_GenericProgressBar.Launch(Localization.Get("GAMEPLAY_BWM_FillingProgress"), refuelDuration, 0f, 0f,
                            "Play_SndActionRefuelLantern", null, false, true, new System.Action<bool, bool, float>(OnFillFinished));
        }
        private static void OnFillFinished(bool success, bool playerCancel, float progress)
        {
            //MelonLoader.MelonLogger.Log("Fill Finished");
            LiquidItem liquidItem = WaterButtons.currentLiquidItemShowing;
            // Remove water and adjust the water supply.
            float maxWaterInBottle = Mathf.Min(Water.GetActual(liquidItem.m_LiquidQuality), liquidItem.m_LiquidCapacityLiters);
            float maximumWaterRefuel = maxWaterInBottle - liquidItem.m_LiquidLiters;
            float finalWaterRefuel = maximumWaterRefuel * progress;
            float finalWaterInBottle = finalWaterRefuel + liquidItem.m_LiquidLiters;
            liquidItem.m_LiquidLiters = 0;
            Water.WATER.Remove(finalWaterRefuel, liquidItem.m_LiquidQuality);
            liquidItem.m_LiquidLiters = finalWaterInBottle;
        }
        private static void OnTransfer()
        {
            //MelonLoader.MelonLogger.Log("Transfer");
            var liquidItem = WaterButtons.currentLiquidItemShowing;
            if (liquidItem == null) return;
            if (Water.IsEmpty(liquidItem)) // If the selected liquid container is empty.
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_BWM_Empty"));
                GameAudioManager.PlayGUIError();
                return;
            }
            float spaceAvailable = Water.GetRemainingCapacityEmpty() + Water.GetRemainingCapacity(liquidItem.m_LiquidQuality) - liquidItem.m_LiquidCapacityLiters + liquidItem.m_LiquidLiters;
            if (Water.IsNone(spaceAvailable))
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_BWM_NoCapacityAvailable"));
                GameAudioManager.PlayGUIError();
                return;
            }
            float maximumWaterTransfer = Mathf.Min(spaceAvailable, liquidItem.m_LiquidLiters);
            GameAudioManager.PlayGuiConfirm();
            float refillDuration = Mathf.Max(maximumWaterTransfer * 4, 1);
            InterfaceManager.m_Panel_GenericProgressBar.Launch(Localization.Get("GAMEPLAY_BWM_TransferingProgress"), refillDuration, 0f, 0f,
                            "Play_SndActionRefuelLantern", null, false, true, new System.Action<bool, bool, float>(OnTransferFinished));
        }
        private static void OnTransferFinished(bool success, bool playerCancel, float progress)
        {
            //MelonLoader.MelonLogger.Log("Transfer Finished");
            var liquidItem = WaterButtons.currentLiquidItemShowing;
            float liquidBefore = liquidItem.m_LiquidLiters;
            float spaceAvailable = Water.GetRemainingCapacityEmpty() + Water.GetRemainingCapacity(liquidItem.m_LiquidQuality) - liquidItem.m_LiquidCapacityLiters + liquidItem.m_LiquidLiters;
            float maximumWaterTransfer = Mathf.Min(spaceAvailable, liquidItem.m_LiquidLiters);
            float actualWaterTransfer = progress * maximumWaterTransfer;
            liquidItem.m_LiquidLiters = liquidItem.m_LiquidCapacityLiters;
            Water.WATER.Add(actualWaterTransfer, liquidItem.m_LiquidQuality);
            liquidItem.m_LiquidLiters = liquidBefore - actualWaterTransfer;
            Water.AdjustWaterSupplyToWater();
        }
        private static void OnDump()
        {
            //MelonLoader.MelonLogger.Log("Dump");
            LiquidItem liquidItem = WaterButtons.currentLiquidItemShowing;
            if (liquidItem == null) return;
            if (liquidItem.m_LiquidLiters <= 0.001f)
            {
                HUDMessage.AddMessage(Localization.Get("GAMEPLAY_BWM_Empty"));
                GameAudioManager.PlayGUIError();
                return;
            }

            GameAudioManager.PlayGuiConfirm();
            float lostLitersDuration = Mathf.Max(liquidItem.m_LiquidLiters * 4, 1);

            InterfaceManager.m_Panel_GenericProgressBar.Launch(Localization.Get("GAMEPLAY_BWM_DumpingProgress"), lostLitersDuration, 0f, 0f,
                            "Play_SndActionRefuelLantern", null, false, true, new System.Action<bool, bool, float>(OnDumpFinished));
        }
        private static void OnDumpFinished(bool success, bool playerCancel, float progress)
        {
            //MelonLoader.MelonLogger.Log("Dump Finished");
            LiquidItem liquidItem = WaterButtons.currentLiquidItemShowing;
            float lostLiters = liquidItem.m_LiquidLiters * progress;
            if (liquidItem.m_LiquidQuality == LiquidQuality.Potable) // Potable water
            {
                WaterSupply potableWaterSupply = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply;
                Water.ShowLostMessage(potableWaterSupply, "GAMEPLAY_WaterPotable", lostLiters);
            }
            else // NonPotable water
            {
                WaterSupply nonPotableWaterSupply = GameManager.GetInventoryComponent().GetNonPotableWaterSupply().m_WaterSupply;
                Water.ShowLostMessage(nonPotableWaterSupply, "GAMEPLAY_WaterUnsafe", lostLiters);
            }

            // Remove water and adjust the water supply.
            liquidItem.m_LiquidLiters = Mathf.Max(liquidItem.m_LiquidLiters - lostLiters, 0);
            Water.AdjustWaterSupplyToWater();
        }
    }

    [HarmonyPatch(typeof(Panel_Inventory),"Start")]
    internal class Panel_Inventory_Start
    {
        private static void Postfix(Panel_Inventory __instance)
        {
            WaterButtons.Initialize(__instance.m_ItemDescriptionPage);
        }
    }

    [HarmonyPatch(typeof(ItemDescriptionPage),"BuildItemDescription")]
    internal class ItemDescriptionPage_Start
    {
        private static void Postfix(ItemDescriptionPage __instance, GearItem gi)
        {
            if (__instance != InterfaceManager.m_Panel_Inventory?.m_ItemDescriptionPage) return;

            WaterButtons.currentLiquidItemShowing = gi?.GetComponent<LiquidItem>();
            if (WaterButtons.currentLiquidItemShowing == null || WaterButtons.currentLiquidItemShowing.m_LiquidType != GearLiquidTypeEnum.Water) WaterButtons.SetActive(false);
            else WaterButtons.SetActive(true);
        }
    }
}
