using GearSpawner;
using System;

namespace BetterWaterManagement;

internal static class SpawnProbabilities
{
	internal static void AddToModComponent()
	{
		SpawnTagManager.AddToTaggedFunctions("BetterWaterManagement", new Func<DifficultyLevel, FirearmAvailability, GearSpawnInfo, float>(GetProbability));
	}
	
	private static float GetProbability(DifficultyLevel difficultyLevel, FirearmAvailability firearmAvailability, GearSpawnInfo gearSpawnInfo)
	{
		return difficultyLevel switch
		{
			DifficultyLevel.Pilgram => Settings.options.pilgramSpawnExpectation / 403f * 100f,
			DifficultyLevel.Voyager => Settings.options.voyagerSpawnExpectation / 403f * 100f,
			DifficultyLevel.Stalker => Settings.options.stalkerSpawnExpectation / 403f * 100f,
			DifficultyLevel.Interloper => Settings.options.interloperSpawnExpectation / 403f * 100f,
			DifficultyLevel.Challenge => Settings.options.challengeSpawnExpectation / 403f * 100f,
			DifficultyLevel.Storymode => Settings.options.storySpawnExpectation / 403f * 100f,
			_ => 0f,
		};
	}
}
