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
	internal class BetterWaterSettings : JsonModSettings
	{
		[Section("Gameplay Settings")]
		[Name("Water Menu Increment")]
		[Description("The increment used in the menu to cook water.")]
		[Choice("0.05 Liters", "0.10 Liters", "0.25 Liters", "0.50 Liters")]
		public WaterIncrement waterIncrement = WaterIncrement.Liters0_25;

		[Name("Starting Water Bottles")]
		[Description("The water bottles you start with when you initially spawn in the world.")]
		[Choice("Nothing", "One 500mL bottle", "One 750mL bottle", "One 1000mL bottle", "Two 500mL bottles", "Two 750mL bottles", "Two 1000mL bottles", "One Waterskin")]
		public StartingBottles startingBottles = StartingBottles.One500mL;

		[Section("Spawn Settings")]
		[Name("Pilgram / Very High Loot Custom")]
		[Description("The expected number of times a metal water bottle will randomly spawn in the world based on statistics. Setting to zero disables them on this game mode.  Recommended is 80.")]
		[Slider(0f, 100f, 101)]
		public float pilgramSpawnExpectation = 80f;

		[Name("Voyager / High Loot Custom")]
		[Description("The expected number of times a metal water bottle will randomly spawn in the world based on statistics. Setting to zero disables them on this game mode.  Recommended is 45.")]
		[Slider(0f, 100f, 101)]
		public float voyagerSpawnExpectation = 45f;

		[Name("Stalker / Medium Loot Custom")]
		[Description("The expected number of times a metal water bottle will randomly spawn in the world based on statistics. Setting to zero disables them on this game mode.  Recommended is 30.")]
		[Slider(0f, 100f, 101)]
		public float stalkerSpawnExpectation = 30f;

		[Name("Interloper / Low Loot Custom")]
		[Description("The expected number of times a metal water bottle will randomly spawn in the world based on statistics. Setting to zero disables them on this game mode.  Recommended is 15.")]
		[Slider(0f, 100f, 101)]
		public float interloperSpawnExpectation = 15f;

		[Name("Wintermute")]
		[Description("The expected number of times a metal water bottle will randomly spawn in the world based on statistics. Setting to zero disables them on this game mode.  Recommended is 80.")]
		[Slider(0f, 100f, 101)]
		public float storySpawnExpectation = 80f;

		[Name("Challenges")]
		[Description("The expected number of times a metal water bottle will randomly spawn in the world based on statistics. Setting to zero disables them on this game mode.  Recommended is 80.")]
		[Slider(0f, 100f, 101)]
		public float challengeSpawnExpectation = 80f;
	}

	internal static class Settings
	{
		internal static readonly BetterWaterSettings options = new BetterWaterSettings();
		public static void OnLoad()
		{
			options.AddToModSettings("Better Water Management");
		}

		public static float GetWaterIncrement()
		{
			switch (options.waterIncrement)
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
