using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModSettings;

namespace BetterWaterManagement
{
    internal enum WaterIncrement
    {
        Liters0_05,
        Liters0_10,
        Liters0_25,
        Liters0_50
    };
    internal enum StartingBottles
    {
        Nothing,
        One500mL,
        One750mL,
        One1000mL,
        Two500mL,
        Two750mL,
        Two1000mL,
        One3000mL
    }
    internal class Settings : JsonModSettings
    {
        [Name("Water Menu Increment")]
        [Description("The increment used in the menu to cook water.")]
        [Choice("0.05 Liters","0.10 Liters","0.25 Liters","0.50 Liters")]
        public WaterIncrement waterIncrement = WaterIncrement.Liters0_25;

        [Name("Starting Water Bottles")]
        [Description("The water bottles you start with when you initially spawn in the world.")]
        [Choice("Nothing", "One 500mL bottle", "One 750mL bottle", "One 1000mL bottle", "Two 500mL bottles", "Two 750mL bottles", "Two 1000mL bottles", "One Waterskin")]
        public StartingBottles startingBottles = StartingBottles.One500mL;
    }

    internal static class BetterWaterSettings
    {
        internal static readonly Settings settings = new Settings();
        public static void OnLoad()
        {
            settings.AddToModSettings("Better Water Management");
        }

        public static float GetWaterIncrement()
        {
            switch (settings.waterIncrement)
            {
                case WaterIncrement.Liters0_05:
                    return 0.05f;
                case WaterIncrement.Liters0_10:
                    return 0.1f;
                case WaterIncrement.Liters0_25:
                    return 0.25f;
                case WaterIncrement.Liters0_50:
                    return 0.5f;
                default:
                    return 0.5f;
            }
        }
    }
}
