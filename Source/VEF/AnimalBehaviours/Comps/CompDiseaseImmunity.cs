using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{
    public class CompDiseaseImmunity : ThingComp
    {

      
        public CompProperties_DiseaseImmunity Props
        {
            get
            {
                return (CompProperties_DiseaseImmunity)this.props;
            }
        }


        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (this.parent.IsHashIntervalTick(Props.tickInterval, delta))
            {
                this.TryRemoveDiseases();
            }
        }

        public void TryRemoveDiseases()
        {
            Pawn pawn = this.parent as Pawn;
            if (pawn.health != null && pawn.health.hediffSet != null)
            {
                foreach (string hediff in Props.hediffsToRemove)
                {
                    Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(hediff, false));
                    if (hediffToRemove != null)
                    {
                        pawn.health.RemoveHediff(hediffToRemove);
                    }

                }
            }


        }





    }
}
