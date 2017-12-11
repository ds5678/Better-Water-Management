using Harmony;
using System.Collections.Generic;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(BreakDown), "ProcessInteraction")]
    internal class BreakDown_ProcessInteraction
    {
        public static bool Prefix(BreakDown __instance, ref bool __result)
        {
            if (!__instance.gameObject.activeSelf)
            {
                return true;
            }

            if (Cooking.IsCookware(__instance))
            {
                GameAudioManager.PlayGUIMenuOpen();
                InterfaceManager.m_Panel_BreakDown.m_BreakDown = __instance;
                InterfaceManager.m_Panel_BreakDown.Enable(true);

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Panel_BreakDown), "Enable")]
    internal class Panel_BreakDown_Enable
    {
        private static GameObject buttonPickUp;

        public static void Postfix(Panel_BreakDown __instance, bool enable)
        {
            if (!Cooking.IsCookware(__instance.m_BreakDown))
            {
                return;
            }

            if (enable)
            {
                buttonPickUp = Object.Instantiate(__instance.m_BreakDownButton, __instance.m_BreakDownButton.transform.parent);

                Vector3 position = __instance.m_BreakDownButton.transform.position;
                position.y -= 0.2f;
                buttonPickUp.transform.position = position;

                buttonPickUp.GetComponentInChildren<UILocalize>().key = "GAMEPLAY_PickUp";

                UIButton uiButton = buttonPickUp.GetComponentInChildren<UIButton>();
                uiButton.onClick.Clear();
                uiButton.onClick.Add(new EventDelegate(OnPickup));
            }
            else
            {
                Object.Destroy(buttonPickUp);
            }
        }

        internal static void OnPickup()
        {
            Panel_BreakDown panelBreakDown = InterfaceManager.m_Panel_BreakDown;
            panelBreakDown.Enable(false);

            Cooking.SetCurrentBreakDown(panelBreakDown.m_BreakDown);
            GameManager.GetPlayerManagerComponent().ProcessInspectablePickupItem(Cooking.CurrentBreakDownReplacement);
        }
    }

    [HarmonyPatch(typeof(Panel_BreakDown), "Update")]
    internal class Panel_BreakDown_Update
    {
        public static void Postfix(Panel_BreakDown __instance)
        {
            if (!__instance.IsEnabled() || __instance.IsBreakingDown() || Utils.IsMouseActive())
            {
                return;
            }

            if (InputManager.GetInventoryExaminePressed())
            {
                Panel_BreakDown_Enable.OnPickup();
            }
        }
    }

    [HarmonyPatch(typeof(Panel_BreakDown), "GetSelectedTool")]
    internal class Panel_BreakDown_GetSelectedTool
    {
        public static bool Prefix(Panel_BreakDown __instance, ref GearItem __result)
        {
            List<GearItem> m_Tool = (List<GearItem>)AccessTools.Field(__instance.GetType(), "m_Tools").GetValue(__instance);
            if (m_Tool.Count == 0)
            {
                __result = null;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Panel_BreakDown), "UpdateButtonLegend")]
    internal class Panel_BreakDown_UpdateButtonLegend
    {
        public static bool Prefix(Panel_BreakDown __instance)
        {
            if (!Cooking.IsCookware(__instance.m_BreakDown))
            {
                return true;
            }

            __instance.m_ButtonLegendContainer.BeginUpdate();
            __instance.m_ButtonLegendContainer.UpdateButton("Inventory_Examine", "GAMEPLAY_PickUp", !__instance.IsBreakingDown(), 2, true);
            __instance.m_ButtonLegendContainer.UpdateButton("Continue", "GAMEPLAY_BreakDown", !__instance.IsBreakingDown(), 1, true);
            __instance.m_ButtonLegendContainer.UpdateButton("Escape", "GAMEPLAY_ButtonBack", !__instance.IsBreakingDown(), 0, true);
            __instance.m_ButtonLegendContainer.EndUpdate();

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "ExitInspectGearMode")]
    internal class PlayerManager_ExitInspectGearMode
    {
        public static void Postfix(PlayerManager __instance)
        {
            Cooking.ResetCurrentBreakDown();
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "ProcessPickupItemInteraction")]
    internal class PlayerManager_ProcessPickupItemInteraction
    {
        public static void Postfix(PlayerManager __instance)
        {
            Cooking.ConsumeCurrentBreakDown();
        }
    }
}