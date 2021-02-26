using UnityEngine;

namespace BetterWaterManagement
{
    internal class PickWater
    {
        private static UILabel labelNoCapacityWarning;
        private static UILabel labelCapacityInfo;

        internal static void ClampAmount(Panel_PickWater panel)
        {
            WaterSource waterSource = GetWaterSource(panel);
            if (!waterSource)
            {
                Implementation.LogError("Could not find WaterSupply to transfer to");
                return;
            }

            float limit = Water.GetRemainingCapacity(waterSource.GetQuality()) + Water.GetRemainingCapacityEmpty();
            panel.m_numLiters = Mathf.Min(panel.m_numLiters, limit);
        }

        internal static void Prepare(Panel_PickWater panel)
        {
            labelNoCapacityWarning = NGUITools.AddChild<UILabel>(panel.gameObject);
            labelNoCapacityWarning.depth = 2000;
            labelNoCapacityWarning.bitmapFont = panel.m_Label_Description.bitmapFont;
            labelNoCapacityWarning.fontSize = 12;
            labelNoCapacityWarning.alignment = NGUIText.Alignment.Center;
            labelNoCapacityWarning.pivot = UIWidget.Pivot.Center;
            labelNoCapacityWarning.width = 400;
            labelNoCapacityWarning.height = 12;
            labelNoCapacityWarning.capsLock = true;
            labelNoCapacityWarning.transform.position = new Vector3(0, -0.908f, 0);
            labelNoCapacityWarning.text = Localization.Get("GAMEPLAY_NoCapacityAvailable");
            labelNoCapacityWarning.gameObject.SetActive(false);

            labelCapacityInfo = NGUITools.AddChild<UILabel>(panel.gameObject);
            labelCapacityInfo.depth = 2000;
            labelNoCapacityWarning.color = new Color(0.640f, 0.202f, 0.231f);
            labelCapacityInfo.bitmapFont = panel.m_Label_Description.bitmapFont;
            labelCapacityInfo.fontSize = 14;
            labelCapacityInfo.alignment = NGUIText.Alignment.Center;
            labelCapacityInfo.pivot = UIWidget.Pivot.Center;
            labelCapacityInfo.width = 400;
            labelCapacityInfo.height = 14;
            labelCapacityInfo.capsLock = true;
            labelCapacityInfo.transform.position = new Vector3(0, -0.858f, 0);
        }

        internal static void UpdateButtons(Panel_PickWater panel)
        {
            WaterSource waterSource = GetWaterSource(panel);
            if (!waterSource)
            {
                Implementation.LogError("Could not find WaterSource");
                return;
            }

            float limit = Water.GetRemainingCapacity(waterSource.GetQuality()) + Water.GetRemainingCapacityEmpty();
            panel.m_ButtonIncrease.SetActive(panel.m_numLiters < limit);
        }

        internal static void UpdateCapacityInfo(Panel_PickWater panel)
        {
            WaterSource waterSource = GetWaterSource(panel);
            if (!waterSource)
            {
                Implementation.LogError("UpdateCapacityInfo: Could not find WaterSource");
                return;
            }

            labelCapacityInfo.text = GetWaterInfo(LiquidQuality.Potable) + "    " +
                GetWaterInfo(LiquidQuality.NonPotable) + "    " +
                Localization.Get("GAMEPLAY_Empty") + ": " + WaterUtils.FormatWaterAmount(0) + "/" + WaterUtils.FormatWaterAmount(Water.GetRemainingCapacityEmpty());

            labelNoCapacityWarning.gameObject.SetActive(Water.GetRemainingCapacityEmpty() == 0 && Water.GetRemainingCapacity(waterSource.GetQuality()) == 0);
        }

        private static string GetWaterInfo(LiquidQuality quality)
        {
            return Localization.Get("GAMEPLAY_Water" + quality.ToString()) + ": " + WaterUtils.FormatWaterAmount(Water.GetActual(quality)) + "/" + WaterUtils.FormatWaterAmount(Water.GetCapacity(quality));
        }

        internal static void UpdateDrinking(Panel_PickWater panel)
        {
            InterfaceManager.m_Panel_HUD.m_InspectMode_Equip.gameObject.SetActive(panel.IsEnabled());
            InterfaceManager.m_Panel_HUD.m_InspectMode_Equip.text = Localization.Get("GAMEPLAY_DrinkIt");
            InterfaceManager.m_Panel_HUD.m_InspectMode_Equip.enabled = true;//test
            //InterfaceManager.m_Panel_HUD.m_InspectMode_Equip.
        }

        private static WaterSource GetWaterSource(Panel_PickWater panel)
        {
            //return Traverse.Create(panel).Field("m_WaterSource").GetValue<WaterSource>();
            return panel.m_WaterSource;
        }

        private static void Refresh(Panel_PickWater panel)
        {
            //Traverse.Create(panel).Method("Refresh").GetValue();
            panel.Refresh();
        }
    }
}