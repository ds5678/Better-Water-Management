using Harmony;
using System.Reflection;
using UnityEngine;

using static BetterWaterManagement.Water;

namespace BetterWaterManagement
{
    internal class Cooking
    {
        private static bool inInspect;

        private static UILabel labelCookingWarning;
        private static UILabel labelEmptyCapacity;

        private static Vector3 offset = new Vector3(0, -60f, 0);

        internal static GearItem Cookware
        {
            get; private set;
        }

        internal static BreakDown CurrentBreakDown
        {
            get; private set;
        }

        internal static GearItem CurrentBreakDownReplacement
        {
            get; private set;
        }

        internal static int MaxLiters
        {
            get; private set;
        }

        internal static void ClampBoilWaterAmount(Panel_FeedFire panel)
        {
            float totalCapacity = Water.GetRemainingCapacity(LiquidQuality.Potable) + Water.GetRemainingCapacityEmpty();
            labelCookingWarning.gameObject.SetActive(Cookware == null);

            FieldInfo boilWaterLitersField = AccessTools.Field(panel.GetType(), "m_BoilWaterLiters");
            float value = Mathf.Clamp((float)boilWaterLitersField.GetValue(panel), 0, MaxLiters);
            boilWaterLitersField.SetValue(panel, value);

            panel.m_ButtonIncreaseWater.SetActive(panel.m_ButtonIncreaseWater.activeSelf && value < MaxLiters);
            panel.m_WaterAmountLabel.text = Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, value);
        }

        internal static void ClampMeltSnowAmount(Panel_FeedFire panel)
        {
            float totalCapacity = Water.GetRemainingCapacity(LiquidQuality.NonPotable) + Water.GetRemainingCapacityEmpty();

            float limit = MaxLiters;
            if (limit > 0 && totalCapacity == 0)
            {
                labelCookingWarning.text = Localization.Get("GAMEPLAY_NoCapacityAvailable");
                limit = 0;
            }
            labelCookingWarning.gameObject.SetActive(limit == 0);

            FieldInfo meltSnowLitersField = AccessTools.Field(panel.GetType(), "m_MeltSnowLiters");
            float value = Mathf.Clamp((float)meltSnowLitersField.GetValue(panel), 0, limit);
            meltSnowLitersField.SetValue(panel, value);

            panel.m_ButtonIncreaseWater.SetActive(panel.m_ButtonIncreaseWater.activeSelf && value < limit);
            panel.m_WaterAmountLabel.text = Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, value);
        }

        internal static void Configure(Panel_FeedFire panel)
        {
            panel.m_MaxMeltSnowLiters = MaxLiters;

            FieldInfo meltSnowLitersField = AccessTools.Field(panel.GetType(), "m_MeltSnowLiters");
            meltSnowLitersField.SetValue(panel, Mathf.Clamp((float)meltSnowLitersField.GetValue(panel), 0, MaxLiters));

            FieldInfo boilWaterLitersField = AccessTools.Field(panel.GetType(), "m_BoilWaterLiters");
            boilWaterLitersField.SetValue(panel, Mathf.Clamp((float)boilWaterLitersField.GetValue(panel), 0, MaxLiters));

            if (Cookware == null)
            {
                labelCookingWarning.text = Localization.Get("GAMEPLAY_RequiresCookware");
                labelCookingWarning.gameObject.SetActive(true);
            }
            else
            {
                labelCookingWarning.gameObject.SetActive(false);
            }
        }

        internal static void HideCookingGearInInspect(Panel_FeedFire panel)
        {
            if (!inInspect || Cookware == null)
            {
                return;
            }

            Utils.ExitInspectForGearItem(Cookware);
            panel.m_Texture_InspectItem.gameObject.SetActive(false);
            panel.m_Texture_GearItem.gameObject.SetActive(true);

            inInspect = false;
        }

        internal static bool IsCookware(BreakDown breakDown)
        {
            if (!breakDown)
            {
                return false;
            }

            return breakDown.m_LocalizedDisplayName.m_LocalizationID == "GAMEPLAY_MetalPot" || breakDown.m_LocalizedDisplayName.m_LocalizationID == "GAMEPLAY_MetalPan";
        }

        internal static void Prepare(Panel_FeedFire panel)
        {
            labelCookingWarning = NGUITools.AddChild<UILabel>(panel.m_TabWater);
            labelCookingWarning.depth = 2000;
            labelCookingWarning.color = new Color(0.640f, 0.202f, 0.231f);
            labelCookingWarning.bitmapFont = panel.m_Label_Water.bitmapFont;
            labelCookingWarning.fontSize = 14;
            labelCookingWarning.alignment = NGUIText.Alignment.Center;
            labelCookingWarning.pivot = UIWidget.Pivot.Center;
            labelCookingWarning.width = 400;
            labelCookingWarning.height = 20;
            labelCookingWarning.capsLock = true;
            labelCookingWarning.transform.position = new Vector3(0, -0.858f, 0);

            UISprite[] sprites = Resources.FindObjectsOfTypeAll<UISprite>();
            foreach (UISprite eachSprite in sprites)
            {
                if (eachSprite.width == 320 && eachSprite.height == 60 && NGUITools.IsChild(panel.transform, eachSprite.transform) && eachSprite.transform.localPosition.x == -160 && eachSprite.transform.localPosition.y == -30)
                {
                    eachSprite.pivot = UIWidget.Pivot.Top;
                    eachSprite.height = (int)(eachSprite.height + Mathf.Abs(offset.y));
                }
            }

            UITexture texturePotable = WaterUtils.GetUITexure("gearIcon_Potable");
            if (texturePotable != null)
            {
                UITexture textureEmpty = Object.Instantiate(texturePotable, texturePotable.transform.parent);
                textureEmpty.transform.localPosition = texturePotable.transform.localPosition + offset;
                textureEmpty.mainTexture = (Texture)Resources.Load("InventoryGridIcons/ico_GearItem__WaterSupplyNone");
            }

            UILabel labelPotable = WaterUtils.GetUILabel("LabelPotable");
            if (labelPotable != null)
            {
                UILabel labelEmpty = Object.Instantiate(labelPotable, labelPotable.transform.parent);
                labelEmpty.transform.localPosition = labelPotable.transform.localPosition + offset;
                labelEmpty.GetComponent<UILocalize>().key = "GAMEPLAY_Empty";
            }

            labelEmptyCapacity = Object.Instantiate(panel.m_Label_PotableSupply, panel.m_Label_PotableSupply.transform.parent);
            labelEmptyCapacity.transform.localPosition = panel.m_Label_PotableSupply.transform.localPosition + offset;
        }

        internal static void RefreshTabWater(Panel_FeedFire panel)
        {
            MeasurementUnits units = InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units;

            panel.m_Label_PotableSupply.text = Utils.GetLiquidQuantityStringNoOunces(units, WATER.ActualPotable) + "/" + Utils.GetLiquidQuantityStringWithUnitsNoOunces(units, WATER.CapacityPotable);
            panel.m_Label_NonpotableSupply.text = Utils.GetLiquidQuantityStringNoOunces(units, WATER.ActualNonPotable) + "/" + Utils.GetLiquidQuantityStringWithUnitsNoOunces(units, WATER.CapacityNonPotable);
            labelEmptyCapacity.text = Utils.GetLiquidQuantityStringWithUnitsNoOunces(units, WATER.CapacityEmpty);

            panel.m_Texture_InspectItem.gameObject.SetActive(Cookware != null);
            panel.m_Texture_GearItem.alpha = 0.25f;
        }

        internal static void RemoveCurrentBreakDown()
        {
            if (!CurrentBreakDown)
            {
                return;
            }

            CurrentBreakDown.m_YieldObject = new GameObject[0];
            CurrentBreakDown.DoBreakDown();
            SetCurrentBreakDown(null);
        }

        internal static void Restore(Panel_FeedFire panel)
        {
            HideCookingGearInInspect(panel);
        }

        internal static void SelectCookingGear()
        {
            GearItem metalPot = GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_MetalPot");
            if (metalPot != null)
            {
                Cookware = metalPot;
                MaxLiters = 3;
                return;
            }

            GearItem metalPan = GameManager.GetInventoryComponent().GetBestGearItemWithName("GEAR_MetalPan");
            if (metalPan != null)
            {
                Cookware = metalPan;
                MaxLiters = 1;
                return;
            }

            Cookware = null;
            MaxLiters = 0;
        }

        internal static void SetCurrentBreakDown(BreakDown breakDown)
        {
            CurrentBreakDown = breakDown;
            CurrentBreakDownReplacement = null;

            if (CurrentBreakDown != null)
            {
                GameObject prefab = Resources.Load(GetCurrentReplacementName()) as GameObject;
                if (prefab != null)
                {
                    CurrentBreakDownReplacement = Object.Instantiate(prefab).GetComponent<GearItem>();
                    CurrentBreakDownReplacement.name = prefab.name;
                }
            }
        }

        internal static void ShowCookingGearInInspect(Panel_FeedFire panel)
        {
            if (inInspect || Cookware == null)
            {
                return;
            }

            Utils.ShowInspectForGearItem(Cookware);
            panel.m_Texture_InspectItem.gameObject.SetActive(true);
            panel.m_Texture_GearItem.gameObject.SetActive(false);

            inInspect = true;
        }

        internal static void UnselectCookingGear()
        {
            Cookware = null;
        }

        private static string GetCurrentReplacementName()
        {
            if (CurrentBreakDown.name == "OBJ_MetalPot")
            {
                return "GEAR_MetalPot";
            }

            if (CurrentBreakDown.name == "OBJ_MetalPan")
            {
                return "GEAR_MetalPan";
            }

            return CurrentBreakDown.m_LocalizedDisplayName.m_LocalizationID.Replace("GAMEPLAY_", "GEAR_");
        }
    }
}