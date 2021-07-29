
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_ApplyHediffWhenBound : CompProperties
    {

        public int checkingInterval = 1000;
        public HediffDef hediffToApply;

        public CompProperties_ApplyHediffWhenBound()
        {
            this.compClass = typeof(CompApplyHediffWhenBound);
        }


    }
}