extern alias Hinterland;
using HarmonyLib;
using Hinterland;

namespace BetterWaterManagement
{
	//
	//These patches make the cook water menu increment by a custom amount instead of the default 0.5 L
	//
	internal class PanelCookingPatches
	{
		[HarmonyPatch(typeof(Panel_Cooking), nameof(Panel_Cooking.OnBoilDown))]
		internal class Panel_Cooking_OnBoilDown
		{
			private static void Prefix(Panel_Cooking __instance, out float __state)
			{
				__state = __instance.m_BoilWaterLiters;
			}
			private static void Postfix(Panel_Cooking __instance, float __state)
			{
				//Implementation.Log("OnBoilDown");
				if (__state != __instance.m_BoilWaterLiters)
				{
					__instance.m_BoilWaterLiters = __state - Settings.GetWaterIncrement();
					__instance.ClampWaterBoilAmount();
				}
			}
		}
		[HarmonyPatch(typeof(Panel_Cooking), nameof(Panel_Cooking.OnBoilUp))]
		internal class Panel_Cooking_OnBoilUp
		{
			private static void Prefix(Panel_Cooking __instance, out float __state)
			{
				__state = __instance.m_BoilWaterLiters;
			}
			private static void Postfix(Panel_Cooking __instance, float __state)
			{
				//Implementation.Log("OnBoilUp");
				if (__state != __instance.m_BoilWaterLiters)
				{
					__instance.m_BoilWaterLiters = __state + Settings.GetWaterIncrement();
					__instance.ClampWaterBoilAmount();
				}
			}
		}
		[HarmonyPatch(typeof(Panel_Cooking), nameof(Panel_Cooking.OnMeltSnowDown))]
		internal class Panel_Cooking_OnMeltSnowDown
		{
			private static void Prefix(Panel_Cooking __instance, out float __state)
			{
				__state = __instance.m_MeltSnowLiters;
			}
			private static void Postfix(Panel_Cooking __instance, float __state)
			{
				//Implementation.Log("OnMeltSnowDown");
				if (__state != __instance.m_MeltSnowLiters)
				{
					__instance.m_MeltSnowLiters = __state - Settings.GetWaterIncrement();
					__instance.ClampMeltSnowAmount();
				}
			}
		}
		[HarmonyPatch(typeof(Panel_Cooking), nameof(Panel_Cooking.OnMeltSnowUp))]
		internal class Panel_Cooking_OnMeltSnowUp
		{
			private static void Prefix(Panel_Cooking __instance, out float __state)
			{
				__state = __instance.m_MeltSnowLiters;
			}
			private static void Postfix(Panel_Cooking __instance, float __state)
			{
				//Implementation.Log("OnMeltSnowUp");
				if (__state != __instance.m_MeltSnowLiters)
				{
					__instance.m_MeltSnowLiters = __state + Settings.GetWaterIncrement();
					__instance.ClampMeltSnowAmount();
				}
			}
		}
	}
}
