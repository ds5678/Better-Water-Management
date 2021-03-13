using System.Collections.Generic;
using UnityEngine;

namespace BetterWaterManagement
{
    public class Water
    {
        public const float MIN_AMOUNT = 0.005f;
        public static readonly Water WATER = new Water();
        internal static bool IgnoreChanges;

        private static readonly System.Comparison<LiquidItem> ADDING_ORDER = (LiquidItem x, LiquidItem y) =>
        {
            int literComparison = y.m_LiquidLiters.CompareTo(x.m_LiquidLiters); //negative if x has more fluid than y
            return literComparison != 0 ? literComparison : y.m_LiquidCapacityLiters.CompareTo(x.m_LiquidCapacityLiters); //negative if x has a bigger capacity than y
            //if the liquid volumes are different, return a comparison of that; else, return a comparison of their capacities
            //sorts full containers to the beginning of the list; if two containers have the same amount of fluid the bigger container is filled first
        };

        private static readonly System.Comparison<LiquidItem> REMOVING_ORDER = (LiquidItem x, LiquidItem y) => x.m_LiquidLiters.CompareTo(y.m_LiquidLiters);
        //negative if x has less fluid than y
        //sorts containers by ascending liquid amount, ie empty at the start of the list towards more at the end

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

        //Readjusts the "game" potable water supply to match the "actual" amount of water held in containers by the player
        public static void AdjustWaterSupplyToWater()
        {
            if (IgnoreChanges)
            {
                return;
            }

            //Updates all the variables including ActualNonPotable and ActualPotable
            WATER.Update();

            Inventory inventory = GameManager.GetInventoryComponent();
            if (inventory == null)
            {
                return;
            }

            var nonPotableWaterSupply = inventory.GetNonPotableWaterSupply();
            if (nonPotableWaterSupply != null)
            {
                //sets the "game" nonpotable water supply to match the "actual" amount of nonpotable water held in containers by the player
                nonPotableWaterSupply.m_WaterSupply.m_VolumeInLiters = WATER.ActualNonPotable;
            }

            var potableWaterSupply = inventory.GetPotableWaterSupply();
            if (potableWaterSupply != null)
            {
                //sets the "game" potable water supply to match the "actual" amount of potable water held in containers by the player
                potableWaterSupply.m_WaterSupply.m_VolumeInLiters = WATER.ActualPotable;
            }
        }

        public static void AdjustWaterToWaterSupply()
        {
            if (IgnoreChanges)
            {
                return;
            }

            var potableWaterSupply = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply;
            var potableDelta = potableWaterSupply.m_VolumeInLiters - WATER.ActualPotable; //the change in potable water held by the player

            var nonPotableWaterSupply = GameManager.GetInventoryComponent().GetNonPotableWaterSupply().m_WaterSupply;
            var nonPotableDelta = nonPotableWaterSupply.m_VolumeInLiters - WATER.ActualNonPotable; //the change in nonpotable water held by the player

            //Does nothing if potableDelta and nonPotableDelta are positive
            WATER.Remove(-nonPotableDelta, LiquidQuality.NonPotable);
            WATER.Remove(-potableDelta, LiquidQuality.Potable);
            //Does nothing if potableDelta and nonPotableDelta are negative
            WATER.Add(potableDelta, LiquidQuality.Potable);
            WATER.Add(nonPotableDelta, LiquidQuality.NonPotable);

            WATER.UpdateAmounts();//Recalculates the total amounts of water held and the total capacities for each type
            WATER.UpdateBottles();//Updates the sound and texture of each water bottle in the inventory

            float potableWaterLost = potableWaterSupply.m_VolumeInLiters - WATER.ActualPotable; //This water could not be added due to lack of space. It is lost.
            potableWaterSupply.m_VolumeInLiters = WATER.ActualPotable;
            if (!IsNone(potableWaterLost))
            {
                SendDelayedLostMessage(potableWaterSupply, "GAMEPLAY_WaterPotable", potableWaterLost);
            }

            float nonPotableWaterLost = nonPotableWaterSupply.m_VolumeInLiters - WATER.ActualNonPotable; //This water could not be added due to lack of space. It is lost.
            nonPotableWaterSupply.m_VolumeInLiters = WATER.ActualNonPotable;
            if (!IsNone(nonPotableWaterLost))
            {
                SendDelayedLostMessage(nonPotableWaterSupply, "GAMEPLAY_WaterUnsafe", nonPotableWaterLost);
            }
        }

        //Returns the current value
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

        //Returns the current value
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

        //Returns the current value
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

        //Returns the current value of the variable representing the total capacity of the empty containers
        public static float GetRemainingCapacityEmpty()
        {
            return WATER.CapacityEmpty;
        }

        //Updates the list of water containers in the inventory
        //Recalculates the total amounts of water held and the total capacities for each type
        //Updates the sound and texture of each water bottle in the inventory
        public void Update()
        {
            liquidItems.Clear();

            Inventory inventory = GameManager.GetInventoryComponent();
            foreach (GameObject eachItem in inventory.m_Items)
            {
                LiquidItem liquidItem = eachItem.GetComponent<LiquidItem>();
                //if not a liquid item or not a water container
                if (liquidItem == null || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
                {
                    continue; // move to the next item
                }
                //else add it to the list of water containers
                liquidItems.Add(liquidItem);
            }

            UpdateAmounts();
            UpdateBottles();
        }

        internal static bool IsEmpty(LiquidItem liquidItem)
        {
            return IsNone(liquidItem.m_LiquidLiters);
        }

        //Waits 1 second before showing
        private static System.Collections.IEnumerator DelayedLostMessage(WaterSupply waterSupply, string name, float amount)
        {
            yield return new WaitForSeconds(1f);

            ShowLostMessage(waterSupply, name, amount);
        }

        //Method to send a notification informing the player that water has been lost
        private static void ShowLostMessage(WaterSupply waterSupply, string name, float amount)
        {
            GearMessage.AddMessage(
                waterSupply.name,
                Localization.Get("GAMEPLAY_Lost"),
                " " + Localization.Get(name) + " (" + Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, amount) + ")",
                Color.red,
                false);
        }

        //Sends a message when water is lost
        //Currently sends it instantly because there is an issue with the Coroutine
        private static void SendDelayedLostMessage(WaterSupply waterSupply, string name, float amount)
        {
            //GameManager.Instance().StartCoroutine(DelayedLostMessage(waterSupply, name, amount));
            ShowLostMessage(waterSupply, name, amount);
        }

        //returns true for negative numbers, zero, and small positive numbers
        internal static bool IsNone(float liters)
        {
            return liters < MIN_AMOUNT;
        }

        //Adds water to the bottles in the inventory
        private void Add(float amount, LiquidQuality quality)
        {
            if (IsNone(amount))//returns true for negative numbers, zero, and small positive numbers
            {
                return;
            }

            float remaining = amount;
            liquidItems.Sort(ADDING_ORDER);//fuller bottles first; if the same fill amount, bigger bottle first

            //Nonempty bottles
            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                if (IsEmpty(eachLiquidItem) || eachLiquidItem.m_LiquidQuality != quality)
                {
                    continue;
                }

                //can't add more water than the space available in the bottle
                float transfer = Mathf.Min(remaining, eachLiquidItem.m_LiquidCapacityLiters - eachLiquidItem.m_LiquidLiters);

                eachLiquidItem.m_LiquidLiters += transfer;
                remaining -= transfer;

                if (IsNone(remaining))
                {
                    return;
                }
            }

            //Empty Bottles
            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                if (!IsEmpty(eachLiquidItem))
                {
                    continue;
                }

                //can't add more water than the space available in the bottle
                float transfer = Mathf.Min(remaining, eachLiquidItem.m_LiquidCapacityLiters - eachLiquidItem.m_LiquidLiters);

                eachLiquidItem.m_LiquidLiters += transfer;
                eachLiquidItem.m_LiquidQuality = quality;
                remaining -= transfer;

                if (IsNone(remaining))
                {
                    return;
                }
            }

            //If remaining is still greater than zero at this point, that water becomes the lost amount
        }

        //Take water out of the bottles for things like cooking
        private void Remove(float amount, LiquidQuality quality)
        {
            if (IsNone(amount))//returns true for negative numbers, zero, and small positive numbers
            {
                return;
            }

            float remaining = amount;
            liquidItems.Sort(REMOVING_ORDER);//sorts containers by ascending liquid amount, i.e. empty at the start of the list, more fluid at the end

            foreach (LiquidItem eachLiquidItem in liquidItems)
            {
                if (IsEmpty(eachLiquidItem) || eachLiquidItem.m_LiquidQuality != quality)
                {
                    continue;
                }

                float transfer = Mathf.Min(remaining, eachLiquidItem.m_LiquidLiters); //can't take more water than what is in the container
                eachLiquidItem.m_LiquidLiters -= transfer;
                remaining -= transfer;

                if (IsNone(remaining))
                {
                    return;
                }
            }

            //It should not be possible for remaining to be nonzero at this point. 
            //That would mean that some kind of error occured and that the game tried to take more water than the player possessed.
        }

        //Recalculates the total amounts of water held and the total capacities for each type
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

        //Updates the sound and texture of each water bottle in the inventory
        private void UpdateBottles()
        {
            foreach (LiquidItem eachLiquidItem in this.liquidItems)
            {
                WaterUtils.UpdateWaterBottle(eachLiquidItem.GetComponent<GearItem>());
            }
        }
    }
}