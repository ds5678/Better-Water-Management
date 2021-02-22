using Harmony;
using ModComponentMapper;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(CookingPotItem), "DoSpecialActionFromInspectMode")] //like eating, drinking, or passing time
    internal class CookingPotItem_DoSpecialActionFromInspectMode
    {
        internal static bool Prefix(CookingPotItem __instance)
        {
            //Implementation.Log("CookingPotItem -- DoSpecialActionFromInspectMode");
            if (__instance.GetCookingState() == CookingPotItem.CookingState.Cooking) //patch does not apply while cooking
            {
                return true;
            }

            float waterAmount = WaterUtils.GetWaterAmount(__instance);
            if (waterAmount <= 0) //There must be some water for this to apply
            {
                return true;
            }

            //sets to true if the item has been boiled
            bool potable = __instance.GetCookingState() == CookingPotItem.CookingState.Ready;

            GearItem gearItem = __instance.GetComponent<GearItem>();

            WaterSupply waterSupply = gearItem.m_WaterSupply; //Gets the cooking pot's water supply component
            if (waterSupply == null)//if it doesn't exist
            {
                waterSupply = gearItem.gameObject.AddComponent<WaterSupply>();//create one
                gearItem.m_WaterSupply = waterSupply;//and assign it
            }

            //assign values to the water supply
            waterSupply.m_VolumeInLiters = waterAmount;
            waterSupply.m_WaterQuality = potable ? LiquidQuality.Potable : LiquidQuality.NonPotable;
            waterSupply.m_TimeToDrinkSeconds = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_TimeToDrinkSeconds;
            waterSupply.m_DrinkingAudio = GameManager.GetInventoryComponent().GetPotableWaterSupply().m_WaterSupply.m_DrinkingAudio;

            GameManager.GetPlayerManagerComponent().UseInventoryItem(gearItem); //drink it?

            return false;
        }
    }

    /*[HarmonyPatch(typeof(CookingPotItem), "ExitPlaceMesh")] // transpiler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
    }*/

    //Replacement Patches

    internal class TrackExitPlaceMesh
    {
        public static bool isExecuting = false;
    }

    [HarmonyPatch(typeof(CookingPotItem), "ExitPlaceMesh")]
    internal class CookingPotItem_ExitPlaceMesh
    {
        internal static void Prefix()
        {
            TrackExitPlaceMesh.isExecuting = true;
        }
        
        internal static void Postfix(CookingPotItem __instance)
        {
            TrackExitPlaceMesh.isExecuting = false;
            if (!__instance.AttachedFireIsBurning() && WaterUtils.IsCookingItem(__instance))
            {
                __instance.PickUpCookedItem();
            }
        }
    }

    //Patch prevents PickUpCookedItem from running within the ExitPlaceMesh method
    //In other words, it allows water to be stored in cooking pots
    [HarmonyPatch(typeof(CookingPotItem),"PickUpCookedItem")]
    internal class CookingPotItem_PickUpCookedItem
    {
        private static bool Prefix()
        {
            //Implementation.Log("CookingPotItem - PickUpCookedItem");
            if (TrackExitPlaceMesh.isExecuting)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //this is for a completely separate problem
        //this adds the water to the bottles after picking it up from a cooking pot
        private static void Postfix() 
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    //End Replacements

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

    //[HarmonyPatch(typeof(CookingPotItem), "StartMeltingSnow")] //inlined
    [HarmonyPatch(typeof(Panel_Cooking), "OnMeltSnow")]
    internal class CookingPotItem_StartMeltingSnow
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            //Implementation.Log("CookingPotItem -- StartMeltingSnow");
            ModUtils.GetOrCreateComponent<OverrideCookingState>(__instance).ForceReady = false;
        }
    }

    //[HarmonyPatch(typeof(CookingPotItem), "UpdateBoilingWater")] //inlined
    [HarmonyPatch(typeof(CookingPotItem), "Update")]
    internal class CookingPotItem_UpdateBoilingWater
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            //Implementation.Log("CookingPotItem -- UpdateBoilingWater");
            if (__instance.AttachedFireIsBurning())
            {
                return;
            }
            else if (__instance.m_LitersWaterBeingBoiled > 0)
            {
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
    }

    //[HarmonyPatch(typeof(CookingPotItem), "UpdateMeltingSnow")] //inlined
    [HarmonyPatch(typeof(CookingPotItem), "Update")] //replacement
    internal class CookingPotItem_UpdateMeltingSnow
    {
        internal static void Postfix(CookingPotItem __instance)
        {
            //Implementation.Log("CookingPotItem -- UpdateMeltingSnow");
            if (__instance.AttachedFireIsBurning())
            {
                return;
            }
            else if (__instance.m_LitersSnowBeingMelted > 0)
            {
                if (__instance.m_ParticlesSnowMelting.activeInHierarchy && WaterUtils.IsCooledDown(__instance))
                {
                    Utils.SetActive(__instance.m_ParticlesSnowMelting, false);
                }
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
                ModUtils.GetOrCreateComponent<CookingPotWaterSaveData>(__instance);
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
            //GearItem cookedItem = Traverse.Create(panel_Cooking).Method("GetSelectedFood").GetValue<GearItem>();
            GearItem cookedItem = panel_Cooking.GetSelectedFood();
            //CookingPotItem cookingPotItem = Traverse.Create(panel_Cooking).Field("m_CookingPotInteractedWith").GetValue<CookingPotItem>();
            CookingPotItem cookingPotItem = panel_Cooking.m_CookingPotInteractedWith;

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
            //Utils.GetComponentInChildren<UIButton>(button).onClick = new List<EventDelegate>() { new EventDelegate(Execute) };
            Il2CppSystem.Collections.Generic.List<EventDelegate> placeHolderList = new Il2CppSystem.Collections.Generic.List<EventDelegate>();
            placeHolderList.Add(new EventDelegate(new System.Action(Execute)));
            Utils.GetComponentInChildren<UIButton>(button).onClick = placeHolderList;

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
            //List<GearItem> foodList = Traverse.Create(__instance).Field("m_FoodList").GetValue<List<GearItem>>();
            Il2CppSystem.Collections.Generic.List<GearItem> foodList = __instance.m_FoodList;
            if (foodList == null)
            {
                return;
            }

            foreach (GearItem eachGearItem in foodList)
            {
                CookingModifier cookingModifier = ModUtils.GetComponent<CookingModifier>(eachGearItem);
                cookingModifier?.Revert();
                //if(cookingModifier) Implementation.Log("{0} reverted from Melt and Cook", eachGearItem.name);
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
            //GearItem cookedItem = Traverse.Create(__instance).Method("GetSelectedFood").GetValue<GearItem>();
            GearItem cookedItem = __instance.GetSelectedFood();
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

            //GearItem cookedItem = Traverse.Create(__instance).Method("GetSelectedFood").GetValue<GearItem>();
            GearItem cookedItem = __instance.GetSelectedFood();
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
            //GearItem cookedItem = Traverse.Create(__instance).Method("GetSelectedFood").GetValue<GearItem>();
            GearItem cookedItem = __instance.GetSelectedFood();
            if (cookedItem == null || cookedItem.m_Cookable == null)
            {
                return;
            }

            //CookingPotItem cookingPotItem = Traverse.Create(__instance).Field("m_CookingPotInteractedWith").GetValue<CookingPotItem>();
            CookingPotItem cookingPotItem = __instance.m_CookingPotInteractedWith;
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