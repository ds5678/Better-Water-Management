using ModSettings;

namespace BetterWaterManagement
{
	internal sealed class BetterWaterSettings : JsonModSettings
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
			return options.waterIncrement switch
			{
				WaterIncrement.Liters0_05 => 0.05f,
				WaterIncrement.Liters0_10 => 0.1f,
				WaterIncrement.Liters0_25 => 0.25f,
				WaterIncrement.Liters0_50 => 0.5f,
				_ => 0.5f,
			};
		}
	}
}
