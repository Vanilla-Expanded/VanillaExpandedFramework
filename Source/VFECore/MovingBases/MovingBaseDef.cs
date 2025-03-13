using RimWorld;
using Verse;

namespace VFECore
{
    public class MovingBaseDef : WorldObjectDef
    {
        public FactionDef baseFaction;

        public int ticksPerMove = 3300;

        public IntRange initialSpawnCount;

        public bool initialSpawnScalesWithPopulation;
    }
}