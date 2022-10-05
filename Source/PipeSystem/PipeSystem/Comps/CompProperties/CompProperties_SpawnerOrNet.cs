using Verse;

namespace PipeSystem
{
    public class CompProperties_SpawnerOrNet : CompProperties_Resource
    {
        public bool inheritFaction;
        public bool requiresPower;
        public string saveKeysPrefix;
        public bool showMessageIfOwned;
        public int spawnCount = 1;
        public bool spawnForbidden;
        public IntRange spawnIntervalRange = new IntRange(100, 100);
        public int spawnMaxAdjacent = -1;
        public ThingDef thingToSpawn;
        public bool writeTimeLeftToSpawn;

        public CompProperties_SpawnerOrNet() => compClass = typeof(CompSpawnerOrNet);
    }
}