using System;
using Verse;

namespace VanillaFurnitureExpanded
{
	public class CompProperties_RockSpawner : CompProperties
	{
		public CompProperties_RockSpawner()
		{
			this.compClass = typeof(CompRockSpawner);
		}

		
		public int spawnCount = 1;

		public IntRange spawnIntervalRange = new IntRange(100, 100);

		
		public bool spawnForbidden;

		public bool requiresPower;

		public bool requiresFuel;

		public bool writeTimeLeftToSpawn;

		public bool showMessageIfOwned;

		public string saveKeysPrefix;

		public bool inheritFaction;
	}
}

