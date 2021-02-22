﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace BetterWaterManagement
{
    //
    //These patches make the cook water menu increment by a custom amount instead of the default 0.5 L
    //
    internal class PanelCookingPatches
    {
        [HarmonyPatch(typeof(Panel_Cooking), "OnBoilDown")]
        internal class Panel_Cooking_OnBoilDown
        {
            private static void Prefix(Panel_Cooking __instance,out float __state)
            {
                __state = __instance.m_BoilWaterLiters;
            }
            private static void Postfix(Panel_Cooking __instance, float __state)
            {
                //Implementation.Log("OnBoilDown");
                if(__state != __instance.m_BoilWaterLiters)
                {
                    __instance.m_BoilWaterLiters = __state - BetterWaterSettings.GetWaterIncrement();
                    __instance.ClampWaterBoilAmount();
                }
            }
        }
        [HarmonyPatch(typeof(Panel_Cooking), "OnBoilUp")]
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
                    __instance.m_BoilWaterLiters = __state + BetterWaterSettings.GetWaterIncrement();
                    __instance.ClampWaterBoilAmount();
                }
            }
        }
        [HarmonyPatch(typeof(Panel_Cooking), "OnMeltSnowDown")]
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
                    __instance.m_MeltSnowLiters = __state - BetterWaterSettings.GetWaterIncrement();
                    __instance.ClampMeltSnowAmount();
                }
            }
        }
        [HarmonyPatch(typeof(Panel_Cooking), "OnMeltSnowUp")]
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
                    __instance.m_MeltSnowLiters = __state + BetterWaterSettings.GetWaterIncrement();
                    __instance.ClampMeltSnowAmount();
                }
            }
        }
    }
}
