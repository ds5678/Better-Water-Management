using System.Collections.Generic;
using UnityEngine;

namespace BetterWaterManagement
{
    public class Water
    {
        public const float MIN_AMOUNT = 0.005f;
        public static readonly Water WATER = new Water();

        private static readonly System.Comparison<LiquidItem> ADDING_ORDER = (LiquidItem x, LiquidItem y) =>
        {
            int literComparison = y.m_LiquidLiters.CompareTo(x.m_LiquidLiters);
            return literComparison != 0 ? literComparison : y.m_LiquidCapacityLiters.CompareTo(x.m_LiquidCapacityLiters);
        };

        private static readonly System.Comparison<LiquidItem> REMOVING_ORDER = (LiquidItem x, LiquidItem y) => x.m_LiquidLiters.CompareTo(y.m_LiquidLiters);

        private List<LiquidItem> liquidItems = new List<LiquidItem>();

        private Water()
        {
        }

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

        public float RemainingCapacityNonPotable
        {
            get
            {
                return CapacityNonPotable - ActualNonPotable;
            }
        }

        public float RemainingCapacityPotable
        {
            get
            {
                return CapacityPotable - ActualPotable;
            }
        }

        public static void AdjustWaterSupplyToWater()
        {
            WATER.Update();

            Inventory inventory = GameManager.GetInventoryComponent();
            if (inventory == null)
            {
                return;
            }

            var nonPotableWaterSupply = inventory.GetNonPotableWaterSupply();
            if (nonPotableWaterSupply != null)
            {
                nonPotableWaterSupply.m_WaterSupply.m_VolumeInLiters = WATER.ActualNonPotable;
            }

            var potableWaterSupply = inventory.GetPotableWaterSupply();
            if (potableWaterSupply != null)
            {
                potableWaterSupply.m_WaterSupply.m_VolumeInLiters = WATER.ActualPotable;
            }
        }

        public static void AdjustWaterToWaterSupply()
        {
            var potableWaterSupply = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply;
            var potableDelta = potableWaterSupply.m_VolumeInLiters - WATER.ActualPotable;

            var nonPotableWaterSupply = GameManager.GetInventoryComponent().GetNonPotableWaterSupply().m_WaterSupply;
            var nonPotableDelta = nonPotableWaterSupply.m_VolumeInLiters - WATER.ActualNonPotable;

            WATER.Remove(-nonPotableDelta, LiquidQuality.NonPotable);
            WATER.Remove(-potableDelta, LiquidQuality.Potable);
            WATER.Add(potableDelta, LiquidQuality.Potable);
            WATER.Add(nonPotableDelta, LiquidQuality.NonPotable);

            WATER.UpdateAmounts();
            WATER.UpdateBottles();

            float potableWaterLost = potableWaterSupply.m_VolumeInLiters - WATER.ActualPotable;
            potableWaterSupply.m_VolumeInLiters = WATER.ActualPotable;
            if (!IsNone(potableWaterLost))
            {
                SendDelayedLostMessage(potableWaterSupply, "GAMEPLAY_WaterPotable", potableWaterLost);
            }

            float nonPotableWaterLost = nonPotableWaterSupply.m_VolumeInLiters - WATER.ActualNonPotable;
            nonPotableWaterSupply.m_VolumeInLiters = WATER.ActualNonPotable;
            if (!IsNone(nonPotableWaterLost))
            {
                SendDelayedLostMessage(nonPotableWaterSupply, "GAMEPLAY_WaterUnsafe", nonPotableWaterLost);
            }
        }

        public static float GetActual(LiquidQuality quality)
        {
            if (quality == LiquidQuality.NonPotable)
            {
                return WATER.ActualNonPotable;
            }

            if (quality == LiquidQuality.Potable)
            {
                return WATER.ActualPotable;
            }

            return 0;
        }

        public static float GetCapacity(LiquidQuality quality)
        {
            if (quality == LiquidQuality.NonPotable)
            {
                return WATER.CapacityNonPotable;
            }

            if (quality == LiquidQuality.Potable)
            {
                return WATER.CapacityPotable;
            }

            return 0;
        }

        public static float GetRemainingCapacity(LiquidQuality quality)
        {
            if (quality == LiquidQuality.NonPotable)
            {
                return WATER.RemainingCapacityNonPotable;
            }

            if (quality == LiquidQuality.Potable)
            {
                return WATER.RemainingCapacityPotable;
            }

            return 0;
        }

        public static float GetRemainingCapacityEmpty()
        {
            return WATER.CapacityEmpty;
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

            UpdateAmounts();
            UpdateBottles();
        }

        internal static bool IsEmpty(LiquidItem liquidItem)
        {
            return IsNone(liquidItem.m_LiquidLiters);
        }

        private static System.Collections.IEnumerator DelayedLostMessage(WaterSupply waterSupply, string name, float amount)
        {
            yield return new WaitForSeconds(1f);

            GearMessage.AddMessage(
                waterSupply.name,
                Localization.Get("GAMEPLAY_Lost"),
                " " + Localization.Get(name) + " (" + Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, amount) + ")",
                Color.red,
                false);
        }

        private static bool IsNone(float liters)
        {
            return liters < MIN_AMOUNT;
        }

        private static void SendDelayedLostMessage(WaterSupply waterSupply, string name, float amount)
        {
            GameManager.Instance().StartCoroutine(DelayedLostMessage(waterSupply, name, amount));
        }

        private void Add(float amount, LiquidQuality quality)
        {
            if (IsNone(amount))
            {
                return;
            }

            float remaining = amount;
            liquidItems.Sort(ADDING_ORDER);

            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                if (IsEmpty(eachLiquidItem) || eachLiquidItem.m_LiquidQuality != quality)
                {
                    continue;
                }

                float transfer = Mathf.Min(remaining, eachLiquidItem.m_LiquidCapacityLiters - eachLiquidItem.m_LiquidLiters);
                eachLiquidItem.m_LiquidLiters += transfer;
                remaining -= transfer;

                if (IsNone(remaining))
                {
                    return;
                }
            }

            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                if (!IsEmpty(eachLiquidItem))
                {
                    continue;
                }

                float transfer = Mathf.Min(remaining, eachLiquidItem.m_LiquidCapacityLiters - eachLiquidItem.m_LiquidLiters);

                eachLiquidItem.m_LiquidLiters += transfer;
                eachLiquidItem.m_LiquidQuality = quality;
                remaining -= transfer;

                if (IsNone(remaining))
                {
                    return;
                }
            }
        }

        private void Remove(float amount, LiquidQuality quality)
        {
            if (IsNone(amount))
            {
                return;
            }

            float remaining = amount;
            liquidItems.Sort(REMOVING_ORDER);

            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                if (IsEmpty(eachLiquidItem) || eachLiquidItem.m_LiquidQuality != quality)
                {
                    continue;
                }

                float transfer = Mathf.Min(remaining, eachLiquidItem.m_LiquidLiters);
                eachLiquidItem.m_LiquidLiters -= transfer;
                remaining -= transfer;

                if (IsNone(remaining))
                {
                    return;
                }
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
                if (IsEmpty(eachLiquidItem))
                {
                    CapacityEmpty += eachLiquidItem.m_LiquidCapacityLiters;
                }
                else if (eachLiquidItem.m_LiquidQuality == LiquidQuality.NonPotable)
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

        private void UpdateBottles()
        {
            foreach (LiquidItem eachLiquidItem in this.liquidItems)
            {
                WaterUtils.UpdateWaterBottle(eachLiquidItem.GetComponent<GearItem>());
            }
        }
    }
}