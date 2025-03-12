using RimWorld;

namespace VFECore
{
    public class MovingBaseDef : WorldObjectDef
    {
        public FactionDef baseFaction;

        public int ticksPerMove = 3300;

        public int initialSpawnCount;

        public bool initialSpawnScalesWithPopulation;
    }
}