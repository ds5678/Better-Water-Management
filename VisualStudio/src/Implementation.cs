using MelonLoader;
using UnityEngine;
namespace BetterWaterManagement
{
    internal class Implementation : MelonMod
    {
        public const string NAME = "Better-Water-Management";

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

        internal static void Log(string message)
        {
            MelonLogger.Log(message);
        }

        internal static void Log(string message, params object[] parameters)
        {
            string preformattedMessage = string.Format(message, parameters);
            Log(preformattedMessage);
        }

        internal static void LogError(string message)
        {
            MelonLogger.LogError(message);
        }
    }
}