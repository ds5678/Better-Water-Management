extern alias Hinterland;
using Hinterland;
using UnityEngine;

namespace BetterWaterManagement
{
	internal class WaterUtils
	{
		private const string SOUND_SUFFIX_EMPTY = "_empty";

		internal static bool ContainsWater(GearItem gearItem)
		{
			if (!IsWaterItem(gearItem))
			{
				return false;
			}

			return gearItem.m_LiquidItem.m_LiquidLiters > 0;
		}

		internal static bool ContainsPotableWater(GearItem gearItem)
		{
			return ContainsWater(gearItem) && gearItem.m_LiquidItem.m_LiquidQuality == LiquidQuality.Potable;
		}

		internal static string FormatWaterAmount(float liters)
		{
			return Utils.GetLiquidQuantityStringNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, liters);
		}
		internal static string FormatWaterAmountWithUnits(float liters)
		{
			return Utils.GetLiquidQuantityStringWithUnitsNoOunces(InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units, liters);
		}

		internal static UILabel GetUILabel(string name)
		{
			UILabel[] labels = Resources.FindObjectsOfTypeAll<UILabel>();
			foreach (UILabel eachLabel in labels)
			{
				if (eachLabel.name == name)
				{
					return eachLabel;
				}
			}

			return null;
		}

		internal static UITexture GetUITexure(string name)
		{
			UITexture[] textures = Resources.FindObjectsOfTypeAll<UITexture>();
			foreach (UITexture eachTexture in textures)
			{
				if (eachTexture.name == name)
				{
					return eachTexture;
				}
			}

			return null;
		}

		internal static float GetWaterAmount(CookingPotItem cookingPotItem)
		{
			//System.Reflection.FieldInfo fieldInfo = AccessTools.Field(typeof(CookingPotItem), "m_LitersWaterBeingBoiled");
			//return (float)fieldInfo.GetValue(cookingPotItem);
			return cookingPotItem.m_LitersWaterBeingBoiled;
		}

		internal static string GetWaterSuffix(LiquidItem liquidItem)
		{
			if (Water.IsEmpty(liquidItem))
			{
				return "_empty";
			}

			if (liquidItem.m_LiquidQuality == LiquidQuality.NonPotable)
			{
				return "_nonpotable";
			}

			return "_potable";
		}

		internal static bool IsCookingItem(CookingPotItem cookingPotItem)
		{
			if (!cookingPotItem.IsCookingSomething())
			{
				return false;
			}

			//return Traverse.Create(cookingPotItem).Field("m_GearItemBeingCooked").GetValue() != null;
			return cookingPotItem.m_GearItemBeingCooked != null;
		}

		internal static bool IsCooledDown(CookingPotItem cookingPotItem)
		{
			//return Traverse.Create(cookingPotItem).Field("m_GracePeriodElapsedHours").GetValue<float>() * 60.0 > (double)InterfaceManager.m_Panel_Cooking.m_MinutesGraceTimeInterruptedCooking;
			return (cookingPotItem.m_GracePeriodElapsedHours * 60f) > InterfaceManager.m_Panel_Cooking.m_MinutesGraceTimeInterruptedCooking;
		}

		internal static bool IsWaterItem(GearItem gearItem)
		{
			if (gearItem == null || gearItem.m_LiquidItem == null)
			{
				return false;
			}

			return gearItem.m_LiquidItem.m_LiquidType == GearLiquidTypeEnum.Water;
		}

		internal static void SetElapsedCookingTime(CookingPotItem cookingPotItem, float hours)
		{
			//Traverse.Create(cookingPotItem).Field("m_CookingElapsedHours").SetValue(hours);
			cookingPotItem.m_CookingElapsedHours = hours;
		}

		internal static void SetElapsedCookingTimeForWater(CookingPotItem cookingPotItem, float waterLiters)
		{
			float hours = waterLiters * InterfaceManager.m_Panel_Cooking.m_MinutesToBoilWaterPerLiter * cookingPotItem.GetTotalBoilMultiplier() / 60f;
			SetElapsedCookingTime(cookingPotItem, hours);
		}

		internal static void SetWaterAmount(CookingPotItem cookingPotItem, float value)
		{
			//System.Reflection.FieldInfo fieldInfo = AccessTools.Field(typeof(CookingPotItem), "m_LitersWaterBeingBoiled");
			//fieldInfo.SetValue(cookingPotItem, value);
			cookingPotItem.m_LitersWaterBeingBoiled = value;
		}

		internal static void UpdateWaterBottle(GearItem gearItem)
		{
			UpdateWaterBottleSound(gearItem);
			UpdateWaterBottleTexture(gearItem.m_LiquidItem);
		}

		private static string AppendSuffix(string sound, string suffix)
		{
			if (sound.EndsWith(suffix))
			{
				return sound;
			}

			return sound + suffix;
		}

		private static Texture GetTexture(LiquidItem liquidItem)
		{
			return Resources.Load("Textures/GEAR_WaterBottle" + GetWaterSuffix(liquidItem)).Cast<Texture>();
		}

		private static string StripSuffix(string sound, string suffix)
		{
			if (sound.EndsWith(suffix))
			{
				return sound.Substring(0, sound.Length - suffix.Length);
			}

			return sound;
		}

		private static void UpdateWaterBottleSound(GearItem instance)
		{
			/*if (Water.IsEmpty(instance.m_LiquidItem))
            {
                instance.m_PickUpAudio = AppendSuffix(instance.m_PickUpAudio, SOUND_SUFFIX_EMPTY);
                instance.m_PutBackAudio = AppendSuffix(instance.m_PutBackAudio, SOUND_SUFFIX_EMPTY);
            }
            else
            {
                instance.m_PickUpAudio = StripSuffix(instance.m_PickUpAudio, SOUND_SUFFIX_EMPTY);
                instance.m_PutBackAudio = StripSuffix(instance.m_PutBackAudio, SOUND_SUFFIX_EMPTY);
            }*/
		}

		private static void UpdateWaterBottleTexture(LiquidItem liquidItem)
		{
			Texture texture = GetTexture(liquidItem);

			Renderer[] renderers = liquidItem.GetComponentsInChildren<Renderer>();
			foreach (Renderer eachRenderer in renderers)
			{
				foreach (Material eachMaterial in eachRenderer.materials)
				{
					if ("GEAR_WaterBottle_Mat (Instance)" == eachMaterial.name)
					{
						eachMaterial.mainTexture = texture;
					}
				}
			}
		}
	}
}