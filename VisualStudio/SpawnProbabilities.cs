using GearSpawner;
using System;

namespace BetterWaterManagement
{
	internal static class SpawnProbabilities
	{
		internal static void AddToModComponent()
		{
			SpawnTagManager.AddToTaggedFunctions("BetterWaterManagement", new Func<DifficultyLevel, FirearmAvailability, GearSpawnInfo, float>(GetProbability));
		}
		private static float GetProbability(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo)
		{
			switch (difficultyLevel)
			{
				case DifficultyLevel.Pilgram:
					return Settings.options.pilgramSpawnExpectation / 403f * 100f;
				case DifficultyLevel.Voyager:
					return Settings.options.voyagerSpawnExpectation / 403f * 100f;
				case DifficultyLevel.Stalker:
					return Settings.options.stalkerSpawnExpectation / 403f * 100f;
				case DifficultyLevel.Interloper:
					return Settings.options.interloperSpawnExpectation / 403f * 100f;
				case DifficultyLevel.Challenge:
					return Settings.options.challengeSpawnExpectation / 403f * 100f;
				case DifficultyLevel.Storymode:
					return Settings.options.storySpawnExpectation / 403f * 100f;
				default:
					return 0f;
			}
		}
	}
}
