using Harmony;
using ModComponentMapper;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(CookingPotItem), "DoSpecialActionFromInspectMode")]
    internal class CookingPotItem_DoSpecialActionFromInspectMode
    {
        internal static bool Prefix(CookingPotItem __instance)
        {
            if (__instance.GetCookingState() == CookingPotItem.CookingState.Cooking)
            {
                return true;
            }

            float waterAmount = WaterUtils.GetWaterAmount(__instance);
            if (waterAmount <= 0)
            {
                return true;
            }

            bool potable = __instance.GetCookingState() == CookingPotItem.CookingState.Ready;

            GearItem gearItem = __instance.GetComponent<GearItem>();

            WaterSupply waterSupply = gearItem.m_WaterSupply;
            if (waterSupply == null)
            {
                waterSupply = gearItem.gameObject.AddComponent<WaterSupply>();
                gearItem.m_WaterSupply = waterSupply;
            }

            waterSupply.m_VolumeInLiters = waterAmount;
            waterSupply.m_WaterQuality = potable ? LiquidQuality.Potable : LiquidQuality.NonPotable;
            waterSupply.m_TimeToDrinkSeconds = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_TimeToDrinkSeconds;
            waterSupply.m_DrinkingAudio = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_DrinkingAudio;

            GameManager.GetPlayerManagerComponent().UseInventoryItem(gearItem);

            return false;
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "ExitPlaceMesh")]
    internal class CookingPotItem_ExitPlaceMesh
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            if (!__instance.AttachedFireIsBurning() && WaterUtils.IsCookingItem(__instance))
            {
                __instance.PickUpCookedItem();
            }
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                CodeInstruction codeInstruction = codeInstructions[i];

                if (codeInstruction.opcode != OpCodes.Call)
                {
                    continue;
                }

                MethodInfo methodInfo = codeInstruction.operand as MethodInfo;
                if (methodInfo == null)
                {
                    continue;
                }

                if (methodInfo.Name == "PickUpCookedItem" && methodInfo.DeclaringType == typeof(CookingPotItem))
                {
                    codeInstructions[i - 1].opcode = OpCodes.Nop;
                    codeInstructions[i].opcode = OpCodes.Nop;
                }
            }

            return codeInstructions;
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "SetCookingState")]
    internal class CookingPotItem_SetCookingState
    {
        internal static void Prefix(CookingPotItem __instance, ref CookingPotItem.CookingState cookingState)
        {
            if (cookingState == CookingPotItem.CookingState.Cooking && !__instance.AttachedFireIsBurning() && WaterUtils.GetWaterAmount(__instance) > 0)
            {
                cookingState = __instance.GetCookingState();
            }
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "StartBoilingWater")]
    internal class CookingPotItem_StartBoilingWater
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            Water.AdjustWaterToWaterSupply();

            ModUtils.GetOrCreateComponent<OverrideCookingState>(__instance).ForceReady = false;
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "StartCooking")]
    internal class CookingPotItem_StartCooking
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "StartMeltingSnow")]
    internal class CookingPotItem_StartMeltingSnow
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            ModUtils.GetOrCreateComponent<OverrideCookingState>(__instance).ForceReady = false;
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "UpdateBoilingWater")]
    internal class CookingPotItem_UpdateBoilingWater
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            if (__instance.AttachedFireIsBurning())
            {
                return;
            }

            if ((__instance.m_ParticlesWaterBoiling.activeInHierarchy || __instance.m_ParticlesWaterReady.activeInHierarchy) && WaterUtils.IsCooledDown(__instance))
            {
                Utils.SetActive(__instance.m_ParticlesWaterReady, false);
                Utils.SetActive(__instance.m_ParticlesWaterBoiling, false);

                if (__instance.GetCookingState() == CookingPotItem.CookingState.Ready)
                {
                    ModUtils.GetOrCreateComponent<OverrideCookingState>(__instance).ForceReady = true;
                    WaterUtils.SetElapsedCookingTimeForWater(__instance, WaterUtils.GetWaterAmount(__instance));
                }
            }
        }
    }

    [HarmonyPatch(typeof(CookingPotItem), "UpdateMeltingSnow")]
    internal class CookingPotItem_UpdateMeltingSnow
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            if (__instance.AttachedFireIsBurning())
            {
                return;
            }

            if (__instance.m_ParticlesSnowMelting.activeInHierarchy && WaterUtils.IsCooledDown(__instance))
            {
                Utils.SetActive(__instance.m_ParticlesSnowMelting, false);
            }
        }
    }

    [HarmonyPatch(typeof(GearItem), "Deserialize")]
    internal class GearItem_Deserialize
    {
        internal static void Postfix(GearItem __instance)
        {
            float waterRequired = __instance?.m_Cookable?.m_PotableWaterRequiredLiters ?? 0;
            if (waterRequired > 0)
            {
                ModUtils.GetOrCreateComponent<CookingModifier>(__instance);
            }
        }

        internal static void Prefix(GearItem __instance)
        {
            if (__instance.m_CookingPotItem)
            {
                ModUtils.GetOrCreateComponent<OverrideCookingState>(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(GearPlacePoint), "UpdateAttachedFire")]
    internal class GearPlacePoint_UpdateAttachedFire
    {
        internal static void Postfix(GearItem placedGearNew)
        {
            if (placedGearNew == null || placedGearNew.m_CookingPotItem == null || !placedGearNew.m_CookingPotItem.AttachedFireIsBurning())
            {
                return;
            }

            CookingPotItem cookingPotItem = placedGearNew.m_CookingPotItem;
            OverrideCookingState overrideCookingState = ModUtils.GetComponent<OverrideCookingState>(cookingPotItem);

            if (overrideCookingState?.ForceReady ?? false)
            {
                WaterUtils.SetElapsedCookingTimeForWater(cookingPotItem, WaterUtils.GetWaterAmount(cookingPotItem));
            }
        }
    }

    internal class MeltAndCookButton
    {
        internal static string text;
        private static GameObject button;

        public static void Execute()
        {
            Panel_Cooking panel_Cooking = InterfaceManager.m_Panel_Cooking;
            GearItem cookedItem = Traverse.Create(panel_Cooking).Method("GetSelectedFood").GetValue<GearItem>();
            CookingPotItem cookingPotItem = Traverse.Create(panel_Cooking).Field("m_CookingPotInteractedWith").GetValue<CookingPotItem>();

            GearItem result = cookedItem.Drop(1, false, true);

            CookingModifier cookingModifier = ModUtils.GetOrCreateComponent<CookingModifier>(result);
            cookingModifier.additionalMinutes = result.m_Cookable.m_PotableWaterRequiredLiters * panel_Cooking.m_MinutesToMeltSnowPerLiter;
            cookingModifier.Apply();

            GameAudioManager.Play3DSound(result.m_Cookable.m_PutInPotAudio, cookingPotItem.gameObject);
            cookingPotItem.StartCooking(result);
            panel_Cooking.ExitCookingInterface();
        }

        internal static void Initialize(Panel_Cooking panel_Cooking)
        {
            text = Localization.Get("GAMEPLAY_ButtonMelt") + " & " + Localization.Get("GAMEPLAY_ButtonCook");

            button = Object.Instantiate<GameObject>(panel_Cooking.m_ActionButtonObject, panel_Cooking.m_ActionButtonObject.transform.parent, true);
            button.transform.Translate(0, 0.09f, 0);
            Utils.GetComponentInChildren<UILabel>(button).text = text;
            Utils.GetComponentInChildren<UIButton>(button).onClick = new List<EventDelegate>() { new EventDelegate(Execute) };

            NGUITools.SetActive(button, false);
        }

        internal static void SetActive(bool active)
        {
            NGUITools.SetActive(button, active);
        }
    }

    [HarmonyPatch(typeof(Panel_Cooking), "RefreshFoodList")]
    internal class Panel_Cooking_RefreshFoodList
    {
        internal static void Postfix(Panel_Cooking __instance)
        {
            List<GearItem> foodList = Traverse.Create(__instance).Field("m_FoodList").GetValue<List<GearItem>>();
            if (foodList == null)
            {
                return;
            }

            foreach (GearItem eachGearItem in foodList)
            {
                CookingModifier cookingModifier = ModUtils.GetComponent<CookingModifier>(eachGearItem);
                cookingModifier?.Revert();
            }
        }
    }

    [HarmonyPatch(typeof(Panel_Cooking), "Start")]
    internal class Panel_Cooking_Start
    {
        internal static void Postfix(Panel_Cooking __instance)
        {
            MeltAndCookButton.Initialize(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_Cooking), "UpdateButtonLegend")]
    internal class Panel_Cooking_UpdateButtonLegend
    {
        internal static void Prefix(Panel_Cooking __instance)
        {
            GearItem cookedItem = Traverse.Create(__instance).Method("GetSelectedFood").GetValue<GearItem>();
            bool requiresWater = (cookedItem?.m_Cookable?.m_PotableWaterRequiredLiters ?? 0) > 0;

            if (Utils.IsMouseActive())
            {
                MeltAndCookButton.SetActive(requiresWater);
            }
            else
            {
                __instance.m_ButtonLegendContainer.BeginUpdate();
                __instance.m_ButtonLegendContainer.UpdateButton("Inventory_Drop", MeltAndCookButton.text, requiresWater, 2, false);
            }
        }
    }

    [HarmonyPatch(typeof(Panel_Cooking), "UpdateGamepadControls")]
    internal class Panel_Cooking_UpdateGamepadControls
    {
        internal static bool Prefix(Panel_Cooking __instance)
        {
            if (!InputManager.GetInventoryDropPressed(GameManager.Instance()))
            {
                return true;
            }

            GearItem cookedItem = Traverse.Create(__instance).Method("GetSelectedFood").GetValue<GearItem>();
            bool requiresWater = (cookedItem?.m_Cookable?.m_PotableWaterRequiredLiters ?? 0) > 0;
            if (!requiresWater)
            {
                return true;
            }

            MeltAndCookButton.Execute();
            return false;
        }
    }

    [HarmonyPatch(typeof(Panel_Cooking), "UpdateGearItem")]
    internal class Panel_Cooking_UpdateGearItem
    {
        internal static void Postfix(Panel_Cooking __instance)
        {
            GearItem cookedItem = Traverse.Create(__instance).Method("GetSelectedFood").GetValue<GearItem>();
            if (cookedItem == null || cookedItem.m_Cookable == null)
            {
                return;
            }

            CookingPotItem cookingPotItem = Traverse.Create(__instance).Field("m_CookingPotInteractedWith").GetValue<CookingPotItem>();
            if (cookingPotItem == null)
            {
                return;
            }

            if (cookedItem.m_Cookable.m_PotableWaterRequiredLiters <= 0)
            {
                return;
            }

            float litersRequired = cookedItem.m_Cookable.m_PotableWaterRequiredLiters;
            float additionalMinutes = litersRequired * __instance.m_MinutesToMeltSnowPerLiter * cookingPotItem.GetTotalCookMultiplier();

            __instance.m_Label_CookedItemCookTime.text = GetCookingTime(cookedItem.m_Cookable.m_CookTimeMinutes * cookingPotItem.GetTotalCookMultiplier()) + " (+" + GetCookingTime(additionalMinutes) + " " + Localization.Get("GAMEPLAY_ButtonMelt") + ")";
        }

        private static string GetCookingTime(float minutes)
        {
            if (minutes < 60)
            {
                return Utils.GetExpandedDurationString(Mathf.RoundToInt(minutes));
            }

            return Utils.GetDurationString(Mathf.RoundToInt(minutes));
        }
    }
}