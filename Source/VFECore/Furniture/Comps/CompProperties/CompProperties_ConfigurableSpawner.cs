using System;
using Verse;

namespace VanillaFurnitureExpanded
{
	public class CompProperties_ConfigurableSpawner : CompProperties
	{
		public CompProperties_ConfigurableSpawner()
		{
			this.compClass = typeof(CompConfigurableSpawner);
		}


		public int spawnCount = 1;

		public bool spawnForbidden;

		public bool requiresPower;

		public bool requiresFuel;

		public bool writeTimeLeftToSpawn;

		public bool showMessageIfOwned;

		public string saveKeysPrefix;

		public bool inheritFaction;
	}
}

