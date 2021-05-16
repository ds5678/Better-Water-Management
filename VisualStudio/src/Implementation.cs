using MelonLoader;
using UnityEngine;
namespace BetterWaterManagement
{
	public static class BuildInfo
	{
		public const string Name = "Better-Water-Management"; // Name of the Mod.  (MUST BE SET)
		public const string Description = "A mod to handle water better."; // Description for the Mod.  (Set as null if none)
		public const string Author = "WulfMarius, ds5678"; // Author of the Mod.  (MUST BE SET)
		public const string Company = null; // Company that made the Mod.  (Set as null if none)
		public const string Version = "4.3.0"; // Version of the Mod.  (MUST BE SET)
		public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
	}
	internal class Implementation : MelonMod
	{

		public override void OnApplicationStart()
		{
			Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
			Settings.OnLoad();
			SpawnProbabilities.AddToModComponent();
			UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.OverrideCookingState>();
			UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.CoolDown>();
			UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.CookingModifier>();
			UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<BetterWaterManagement.CookingPotWaterSaveData>();
		}

		internal static void Log(string message, params object[] parameters) => MelonLogger.Log(message, parameters);
		internal static void LogWarning(string message, params object[] parameters) => MelonLogger.LogWarning(message, parameters);
		internal static void LogError(string message, params object[] parameters) => MelonLogger.LogError(message, parameters);
	}
}