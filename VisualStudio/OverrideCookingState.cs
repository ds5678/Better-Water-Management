extern alias Hinterland;
using CustomSaveDataUtilities;
using Hinterland;
using UnhollowerBaseLib.Attributes;

namespace BetterWaterManagement;

public class OverrideCookingState : ModSaveBehaviour
{
	public bool ForceReady;

	public OverrideCookingState(System.IntPtr intPtr) : base(intPtr) { }

	[HideFromIl2Cpp]
	public override void Deserialize(string data)
	{
		ForceReady = false;
		bool.TryParse(data, out ForceReady);

		if (!ForceReady)
		{
			return;
		}

		CookingPotItem cookingPotItem = GetComponent<CookingPotItem>();
		if (cookingPotItem == null)
		{
			return;
		}

		//Traverse.Create(cookingPotItem).Method("SetCookingState", new System.Type[] { typeof(CookingPotItem.CookingState) }).GetValue(CookingPotItem.CookingState.Ready);
		cookingPotItem.SetCookingState(CookingPotItem.CookingState.Ready);
		cookingPotItem.m_GrubMeshRenderer.sharedMaterials = cookingPotItem.m_BoilWaterReadyMaterialsList;
	}

	[HideFromIl2Cpp]
	public override string Serialize()
	{
		return ForceReady.ToString();
	}
}