extern alias Hinterland;
using Hinterland;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace BetterWaterManagement;

public class CoolDown : MonoBehaviour
{
	private CookingPotItem CookingPotItem 
	{ 
		get
		{
			if (_cookingPotItem == null)
			{
				_cookingPotItem = GetComponent<CookingPotItem>();
				originalState = _cookingPotItem.GetCookingState();
				nextState = originalState - 1;
			}
			return _cookingPotItem;
		}
		set => _cookingPotItem = value; 
	}
	private float elapsed;
	private float lastUpdate;
	private CookingPotItem.CookingState nextState;
	private CookingPotItem.CookingState originalState;
	private CookingPotItem? _cookingPotItem;

	public CoolDown(System.IntPtr intPtr) : base(intPtr) { }
	public void FixedUpdate()
	{
		if (!isActiveAndEnabled)
		{
			return;
		}

		elapsed += Time.fixedDeltaTime;
		if (elapsed - lastUpdate > 5)
		{
			lastUpdate = elapsed;
		}

		if (elapsed < 20)
		{
			return;
		}

		if (nextState < CookingPotItem.CookingState.Cooking)
		{
			CookingPotItem.TurnOnParticles(null);
			enabled = false;
			return;
		}

		CookingPotItem.m_CookingState = nextState;
		CookingPotItem.UpdateParticles();
		CookingPotItem.m_CookingState = originalState;
		nextState--;
		elapsed = 0;
	}

	[HideFromIl2Cpp]
	public void SetEnabled(bool enable)
	{
		if (enabled == enable)
		{
			return;
		}

		if (enable)
		{
			enabled = true;
			elapsed = 0;
			lastUpdate = 0;
			originalState = CookingPotItem.GetCookingState();
			nextState = originalState - 1;
		}
		else
		{
			enabled = false;
		}
	}
}