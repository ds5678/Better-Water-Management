using MelonLoader;
namespace BetterWaterManagement;

internal sealed class Implementation : MelonMod
{

	public override void OnApplicationStart()
	{
		Settings.OnLoad();
		SpawnProbabilities.AddToModComponent();
		UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.OverrideCookingState>();
		UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.CoolDown>();
		UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.CookingModifier>();
		UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.CookingPotWaterSaveData>();
	}

	internal static void LogError(string message, params object[] parameters) => MelonLogger.LogError(message, parameters);
}