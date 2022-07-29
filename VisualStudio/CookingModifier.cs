extern alias Hinterland;
using CustomSaveDataUtilities;
using Hinterland;
using UnhollowerBaseLib.Attributes;

namespace BetterWaterManagement;

public class CookingModifier : ModSaveBehaviour
{
	public float additionalMinutes;
	public float originalMinutes;
	public float potableWaterRequiredLiters;

	public CookingModifier(System.IntPtr intPtr) : base(intPtr) { }
	public void Apply()
	{
		Cookable cookable = GetComponent<Cookable>();
		if (cookable != null && additionalMinutes > 0)
		{
			potableWaterRequiredLiters = cookable.m_PotableWaterRequiredLiters;
			originalMinutes = cookable.m_CookTimeMinutes;

			cookable.m_PotableWaterRequiredLiters = 0;
			cookable.m_CookTimeMinutes += additionalMinutes;
		}
	}

	[HideFromIl2Cpp]
	public override void Deserialize(string data)
	{
		if (data == null)
		{
			return;
		}

		CookingModifierData cookingModifierData = MelonLoader.TinyJSON.JSON.Load(data).Make<CookingModifierData>();
		additionalMinutes = cookingModifierData.additionalMinutes;
		originalMinutes = cookingModifierData.originalMinutes;
		potableWaterRequiredLiters = cookingModifierData.potableWaterRequiredLiters;

		Apply();
	}

	public void Revert()
	{
		Cookable cookable = GetComponent<Cookable>();
		if (cookable != null && additionalMinutes > 0)
		{
			//Implementation.Log("OriginalLiters: {0}", cookable.m_PotableWaterRequiredLiters);
			cookable.m_PotableWaterRequiredLiters = potableWaterRequiredLiters;
			//Implementation.Log("RevertedLiters: {0}", cookable.m_PotableWaterRequiredLiters);
			//Implementation.Log("OriginalCookTime: {0}", cookable.m_CookTimeMinutes);
			//cookable.m_CookTimeMinutes -= additionalMinutes;
			cookable.m_CookTimeMinutes = originalMinutes;
			//Implementation.Log("RevertedCookTime: {0}", cookable.m_CookTimeMinutes);
		}
	}

	[HideFromIl2Cpp]
	public override string Serialize()
	{
		CookingModifierData cookingModifierData = new CookingModifierData
		{
			additionalMinutes = additionalMinutes,
			originalMinutes = originalMinutes,
			potableWaterRequiredLiters = potableWaterRequiredLiters
		};

		return MelonLoader.TinyJSON.JSON.Dump(cookingModifierData);
	}
}

internal class CookingModifierData
{
	public float additionalMinutes;
	public float originalMinutes;
	public float potableWaterRequiredLiters;
}