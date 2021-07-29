using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompApplyHediffWhenBound : ThingComp
    {


        public CompProperties_ApplyHediffWhenBound Props
        {
            get
            {
                return (CompProperties_ApplyHediffWhenBound)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(Props.checkingInterval))
            {
                Pawn thisPawn = this.parent as Pawn;
                foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
                {

                    bool flag = false;

                    if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, thisPawn))
                    {                        
                        flag = true;
                    }

                    if (flag) {
                        thisPawn.health.AddHediff(Props.hediffToApply);
                    } else
                    {
                        Hediff hediff = thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffToApply);
                        if (hediff != null)
                        {
                            thisPawn.health.RemoveHediff(hediff);
                        }
                    }




                }

            }
        }





    }
}

