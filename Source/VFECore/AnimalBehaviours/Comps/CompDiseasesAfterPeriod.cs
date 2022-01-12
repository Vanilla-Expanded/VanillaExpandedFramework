using RimWorld;
using Verse;
using Verse.Sound;

namespace AnimalBehaviours

{
    public class CompDiseasesAfterPeriod : ThingComp
    {
        public int tickCounter = 0;

        public CompProperties_DiseasesAfterPeriod Props
        {
            get
            {
                return (CompProperties_DiseasesAfterPeriod)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;

            if (tickCounter >= Props.timeToApplyInTicks)
            {
                Pawn pawn = this.parent as Pawn;

                if (pawn != null && pawn.Map != null)
                {
                    HediffDef randomHediff = Props.hediffsToApply.RandomElement();
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(randomHediff);
                    if (hediff == null)
                    {
                        pawn.health.AddHediff(hediff);
                    }
                    
                    
                }
                tickCounter = (int)(Props.timeToApplyInTicks*0.8);
            }
        }

        

    }
}
