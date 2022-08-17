using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace AnimalBehaviours
{
    public class CompApplyHediffWhenBound : ThingComp
    {

        public Pawn bondedPawn = null;
        public bool leavingMap = false;


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Pawn>(ref this.bondedPawn, "bondedPawn", false);


        }

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
            if (this.parent.IsHashIntervalTick(Props.checkingInterval) && this.parent.Map != null)
            {
                Pawn thisPawn = this.parent as Pawn;
                bool flag = false;
                foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
                {

                    if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, thisPawn))
                    {
                        bondedPawn = pawn;
                        flag = true;
                    }
                }
                if (flag)
                {
                    thisPawn.health.AddHediff(Props.hediffToApply);
                    if (Props.applyHediffToBonded)
                    {
                        bondedPawn.health.AddHediff(Props.hediffToApplyToBonded);
                    }
                }
                else
                {
                    Hediff hediff = thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffToApply);
                    if (hediff != null)
                    {
                        thisPawn.health.RemoveHediff(hediff);
                    }
                    if (Props.doJobIfBondedDies && bondedPawn != null && bondedPawn.Dead && !leavingMap)
                    {
                        if (RCellFinder.TryFindRandomExitSpot(thisPawn, out IntVec3 spot, TraverseMode.PassDoors))
                        {
                            thisPawn.MentalState.RecoverFromState();
                            thisPawn.SetFaction(null);
                            Job job = JobMaker.MakeJob(Props.jobToDoIfBondedDies, spot);
                            job.exitMapOnArrival = true;

                            thisPawn.jobs.TryTakeOrderedJob(job);

                            leavingMap = true;



                        }

                    }
                    if (Props.dieIfBondedDies && bondedPawn != null && bondedPawn.Dead)
                    {
                        this.parent.Kill();
                    }
                    
                }

            }
        }





    }
}

