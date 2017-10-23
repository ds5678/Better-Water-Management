using System.Collections.Generic;
using UnityEngine;

namespace BetterWaterManagement
{
    public class Water
    {
        public static readonly Water WATER = new Water();

        private Water()
        {

        }

        private List<LiquidItem> liquidItems = new List<LiquidItem>();

        public float ActualNonPotable
        {
            get; private set;
        }

        public float ActualPotable
        {
            get; private set;
        }

        public float CapacityEmpty
        {
            get; private set;
        }

        public float CapacityNonPotable
        {
            get; private set;
        }

        public float CapacityPotable
        {
            get; private set;
        }

        public void Update()
        {
            liquidItems.Clear();

            Inventory inventory = GameManager.GetInventoryComponent();
            foreach (GameObject eachItem in inventory.m_Items)
            {
                LiquidItem liquidItem = eachItem.GetComponent<LiquidItem>();
                if (liquidItem == null || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
                {
                    continue;
                }

                liquidItems.Add(liquidItem);
            }

            liquidItems.Sort(delegate (LiquidItem x, LiquidItem y)
            {
                return y.m_LiquidCapacityLiters.CompareTo(x.m_LiquidCapacityLiters);
            });

            UpdateAmounts();
        }

        public static void AdjustWaterSupplyToWater()
        {
            WATER.Update();

            var nonPotableWaterSupply = GameManager.GetInventoryComponent().GetNonPotableWaterSupply().m_WaterSupply;
            nonPotableWaterSupply.m_VolumeInLiters = WATER.ActualNonPotable;

            var potableWaterSupply = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply;
            potableWaterSupply.m_VolumeInLiters = WATER.ActualPotable;
        }

        public static void AdjustWaterToWaterSupply()
        {
            var potableWaterSupply = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply;
            var nonPotableWaterSupply = GameManager.GetInventoryComponent().GetNonPotableWaterSupply().m_WaterSupply;

            WATER.Redistribute(potableWaterSupply.m_VolumeInLiters, nonPotableWaterSupply.m_VolumeInLiters);

            float potableWaterLost = potableWaterSupply.m_VolumeInLiters - WATER.ActualPotable;
            potableWaterSupply.m_VolumeInLiters = WATER.ActualPotable;

            if (potableWaterLost > 0)
            {
                GearMessage.AddMessage(
                    potableWaterSupply.name,
                    Localization.Get("GAMEPLAY_Lost"),
                    " " + Localization.Get("GAMEPLAY_WaterPotable") + " (" + Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, potableWaterLost) + ")",
                    Color.red,
                    false);
            }

            float nonPotableWaterLost = nonPotableWaterSupply.m_VolumeInLiters - WATER.ActualNonPotable;
            nonPotableWaterSupply.m_VolumeInLiters = WATER.ActualNonPotable;

            if (nonPotableWaterLost > 0)
            {
                GearMessage.AddMessage(
                    nonPotableWaterSupply.name,
                    Localization.Get("GAMEPLAY_Lost"),
                    " " + Localization.Get("GAMEPLAY_WaterUnsafe") + " (" + Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, nonPotableWaterLost) + ")",
                    Color.red,
                    false);
            }
        }

        public void Redistribute(float potable, float nonPotable)
        {
            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                eachLiquidItem.m_LiquidLiters = 0;
            }

            Redistribute(potable, LiquidQuality.Potable);
            Redistribute(nonPotable, LiquidQuality.NonPotable);

            UpdateAmounts();
        }

        private void Redistribute(float amount, LiquidQuality quality)
        {
            float remaining = amount;

            int index = 0;
            while (remaining > 0.5f && index < liquidItems.Count)
            {
                LiquidItem liquidItem = liquidItems[index];
                if (liquidItem.m_LiquidLiters == 0)
                {
                    float transfer = Mathf.Min(liquidItem.m_LiquidCapacityLiters, remaining);
                    liquidItem.m_LiquidLiters = transfer;
                    liquidItem.m_LiquidQuality = quality;
                    remaining -= transfer;
                }

                index++;
            }

            index = liquidItems.Count - 1;
            while (remaining > 0 && index >= 0)
            {
                LiquidItem liquidItem = liquidItems[index];
                if (liquidItem.m_LiquidLiters == 0)
                {
                    float transfer = Mathf.Min(liquidItem.m_LiquidCapacityLiters, remaining);
                    liquidItem.m_LiquidLiters = transfer;
                    liquidItem.m_LiquidQuality = quality;
                    remaining -= transfer;
                }

                index--;
            }
        }

        private void UpdateAmounts()
        {
            CapacityEmpty = 0;

            CapacityNonPotable = 0;
            ActualNonPotable = 0;

            CapacityPotable = 0;
            ActualPotable = 0;

            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                if (eachLiquidItem.m_LiquidLiters == 0)
                {
                    CapacityEmpty += eachLiquidItem.m_LiquidCapacityLiters;
                    continue;
                }

                if (eachLiquidItem.m_LiquidQuality == LiquidQuality.NonPotable)
                {
                    CapacityNonPotable += eachLiquidItem.m_LiquidCapacityLiters;
                    ActualNonPotable += eachLiquidItem.m_LiquidLiters;
                }
                else
                {
                    CapacityPotable += eachLiquidItem.m_LiquidCapacityLiters;
                    ActualPotable += eachLiquidItem.m_LiquidLiters;
                }
            }
        }
    }
}