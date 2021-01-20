using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_ExplodingHatcher : CompProperties
    {
        public CompProperties_ExplodingHatcher()
        {
            this.compClass = typeof(CompExplodingHatcher);
        }

        public float hatcherDaystoHatch = 1f;
        public PawnKindDef hatcherPawn;
        public float range = 3f;
        public int damage = 10;
        public string damageDef = "Flame";
        public string soundDef = "AA_GooPop";
    }
}

