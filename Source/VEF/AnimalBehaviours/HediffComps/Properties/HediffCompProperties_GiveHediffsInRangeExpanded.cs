using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_GiveHediffsInRangeExpanded : HediffCompProperties
    {
        public float range;

        public TargetingParameters targetingParameters;

        public HediffDef hediff;

        public ThingDef mote;

        public bool hideMoteWhenNotDrafted;

        public float initialSeverity = 1f;

        public bool affectSameDef;

        public bool needLOS = true;

        public HediffCompProperties_GiveHediffsInRangeExpanded()
        {
            compClass = typeof(HediffComp_GiveHediffsInRangeExpanded);
        }
    }
}
