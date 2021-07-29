
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_ApplyHediffWhenBound : CompProperties
    {

        public int checkingInterval = 1000;
        public HediffDef hediffToApply;
        public bool doJobIfBondedDies = false;
        public JobDef jobToDoIfBondedDies;

        public CompProperties_ApplyHediffWhenBound()
        {
            this.compClass = typeof(CompApplyHediffWhenBound);
        }


    }
}