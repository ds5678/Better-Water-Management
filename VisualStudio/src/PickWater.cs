using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Harmony;

namespace BetterWaterManagement
{
    internal class PickWater
    {
        private static UILabel labelNoCapacityWarning;

        internal static void Prepare(Panel_PickWater panel)
        {
            labelNoCapacityWarning = NGUITools.AddChild<UILabel>(panel.gameObject);
            labelNoCapacityWarning.depth = 2000;
            labelNoCapacityWarning.color = new Color(0.640f, 0.202f, 0.231f);
            labelNoCapacityWarning.bitmapFont = panel.m_Label_Description.bitmapFont;
            labelNoCapacityWarning.fontSize = 14;
            labelNoCapacityWarning.alignment = NGUIText.Alignment.Center;
            labelNoCapacityWarning.pivot = UIWidget.Pivot.Center;
            labelNoCapacityWarning.width = 400;
            labelNoCapacityWarning.height = 20;
            labelNoCapacityWarning.capsLock = true;
            labelNoCapacityWarning.transform.position = new Vector3(0, -0.858f, 0);
            labelNoCapacityWarning.text = Localization.Get("GAMEPLAY_NoCapacityAvailable");
            labelNoCapacityWarning.gameObject.SetActive(false);
        }

        public static void ClampAmount(Panel_PickWater panel)
        {
            WaterSupply waterSupply = AccessTools.Field(panel.GetType(), "m_WaterSupplyInventory").GetValue(panel) as WaterSupply;
            if (!waterSupply)
            {
                Debug.LogError("Could not find WaterSupply to transfer to");
                return;
            }

            float limit = Water.GetRemainingCapacity(waterSupply.m_WaterQuality) + Water.GetRemainingCapacityEmpty();
            panel.m_numLiters = Mathf.Clamp(panel.m_numLiters, 0, limit);

            AccessTools.Method(panel.GetType(), "Refresh").Invoke(panel, null);

            panel.m_ButtonIncrease.SetActive(panel.m_numLiters < limit);

            labelNoCapacityWarning.gameObject.SetActive(limit == 0);
        }
    }
}
