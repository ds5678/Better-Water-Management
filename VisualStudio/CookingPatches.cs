extern alias Hinterland;
using HarmonyLib;
using Hinterland;
using ModComponent.Utils;

namespace BetterWaterManagement;

/*[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.ExitPlaceMesh))] // transpiler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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

[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.ExitPlaceMesh))]
internal class CookingPotItem_ExitPlaceMesh
{
	internal static void Prefix()
	{
		TrackExitPlaceMesh.isExecuting = true;
	}

	internal static void Postfix(CookingPotItem __instance)
	{
		TrackExitPlaceMesh.isExecuting = false;
		// This is used to instantly pick up food from pot/ cans, when using right-click (It is bugged otherwise).
		// It is not allowed for meat -> !__instance.IsDummyPot().
		if (!__instance.AttachedFireIsBurning() && WaterUtils.IsCookingItem(__instance) && !__instance.IsDummyPot())
		{
			__instance.PickUpCookedItem();
		}
	}
}

//Patch prevents PickUpCookedItem from running within the ExitPlaceMesh method
//In other words, it allows water to be stored in cooking pots
[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.PickUpCookedItem))]
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

[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.SetCookingState))]
internal class CookingPotItem_SetCookingState
{
	internal static void Prefix(CookingPotItem __instance, ref CookingPotItem.CookingState cookingState) // circumvent the transformation to "ruined" after a long time period. 
	{
		if (cookingState == CookingPotItem.CookingState.Cooking && !__instance.AttachedFireIsBurning() && WaterUtils.GetWaterAmount(__instance) > 0)
		{
			cookingState = __instance.GetCookingState();
		}
	}
}

[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.StartBoilingWater))]
internal class CookingPotItem_StartBoilingWater
{
	internal static void Postfix(CookingPotItem __instance)
	{
		Water.AdjustWaterToWaterSupply();

		ComponentUtils.GetOrCreateComponent<OverrideCookingState>(__instance).ForceReady = false;
	}
}

[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.StartCooking))]
internal class CookingPotItem_StartCooking
{
	internal static void Postfix(CookingPotItem __instance)
	{
		Water.AdjustWaterToWaterSupply();
	}
}

//[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.StartMeltingSnow))] //inlined
[HarmonyPatch(typeof(Panel_Cooking), nameof(Panel_Cooking.OnMeltSnow))]
internal class CookingPotItem_StartMeltingSnow
{
	internal static void Postfix(CookingPotItem __instance)
	{
		//Implementation.Log("CookingPotItem -- StartMeltingSnow");
		ComponentUtils.GetOrCreateComponent<OverrideCookingState>(__instance).ForceReady = false;
	}
}

//[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.UpdateBoilingWater))] //inlined
[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.Update))]
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
					ComponentUtils.GetOrCreateComponent<OverrideCookingState>(__instance).ForceReady = true;
					WaterUtils.SetElapsedCookingTimeForWater(__instance, WaterUtils.GetWaterAmount(__instance));
				}
			}
		}
	}
}

//[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.UpdateMeltingSnow))] //inlined
[HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.Update))] //replacement
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

[HarmonyPatch(typeof(GearItem), nameof(GearItem.Deserialize))]
internal class GearItem_Deserialize
{
	internal static void Postfix(GearItem __instance)
	{
		float waterRequired = __instance?.m_Cookable?.m_PotableWaterRequiredLiters ?? 0;
		if (waterRequired > 0)
		{
			ComponentUtils.GetOrCreateComponent<CookingModifier>(__instance);
		}
	}

	internal static void Prefix(GearItem __instance)
	{
		if (__instance.m_CookingPotItem)
		{
			ComponentUtils.GetOrCreateComponent<OverrideCookingState>(__instance);
			ComponentUtils.GetOrCreateComponent<CookingPotWaterSaveData>(__instance);
		}
	}
}

[HarmonyPatch(typeof(GearPlacePoint), nameof(GearPlacePoint.UpdateAttachedFire))]
internal class GearPlacePoint_UpdateAttachedFire
{
	internal static void Postfix(GearItem placedGearNew)
	{
		if (placedGearNew == null || placedGearNew.m_CookingPotItem == null || !placedGearNew.m_CookingPotItem.AttachedFireIsBurning())
		{
			return;
		}

		CookingPotItem cookingPotItem = placedGearNew.m_CookingPotItem;
		OverrideCookingState overrideCookingState = ComponentUtils.GetComponentSafe<OverrideCookingState>(cookingPotItem);

		if (overrideCookingState?.ForceReady ?? false)
		{
			WaterUtils.SetElapsedCookingTimeForWater(cookingPotItem, WaterUtils.GetWaterAmount(cookingPotItem));
		}
	}
}