

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace AnimalBehaviours
{
    class HediffCompProperties_Spawner : HediffCompProperties
    {

		public ThingDef thingToSpawn;

		public int spawnCount = 1;

		public int initialSpawnWait = 10000;

		public IntRange spawnIntervalRange = new IntRange(100, 100);

		public int spawnMaxAdjacent = -1;

		public bool spawnForbidden;

		public bool requiresPower;

		public bool writeTimeLeftToSpawn;

		public bool showMessageIfOwned;

		public string saveKeysPrefix;

		public bool inheritFaction;

		public HediffCompProperties_Spawner()
        {
            this.compClass = typeof(HediffComp_Spawner);
        }
    }
}
