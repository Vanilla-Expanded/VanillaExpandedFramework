using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompAutoNutrition : ThingComp
    {


        public CompProperties_AutoNutrition Props
        {
            get
            {
                return (CompProperties_AutoNutrition)this.props;
            }
        }




        public override void CompTick()
        {
            base.CompTick();

            if (this.parent.IsHashIntervalTick(Props.tickInterval))
            {
                Pawn pawn = this.parent as Pawn;
               
                if ((this.parent.Map != null) && (pawn.needs?.food?.CurLevelPercentage < 0.5f) && (pawn.Awake()))
                {
                   
                    Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("VEF_AutoNutrition", true), this.parent);
                    job.count = 1;
                    job.def.reportString = Props.consumingFoodReportString;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                }

            }


        }


    }
}

