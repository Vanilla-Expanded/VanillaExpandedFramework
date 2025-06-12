using RimWorld;
using Verse;

namespace VEF.Planet
{
    public class MovingBaseDef : WorldObjectDef
    {
        public FactionDef baseFaction;

        public int ticksPerMove = 3300;

        public IntRange initialSpawnCount;

        public bool initialSpawnScalesWithPopulation;

        public string attackConfirmationMessage;
    }
}