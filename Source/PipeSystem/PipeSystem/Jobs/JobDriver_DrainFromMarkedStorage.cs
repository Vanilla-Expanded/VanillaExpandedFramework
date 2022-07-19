using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    public class JobDriver_DrainFromMarkedStorage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => Map.designationManager.DesignationOn(TargetThingA, PSDefOf.PS_Drain) == null);

            ThingWithComps thingWithComps = (ThingWithComps)pawn.CurJob.targetA.Thing;
            CompResourceStorage compRS = null;

            for (int i = 0; i < thingWithComps.AllComps.Count; i++)
            {
                if (thingWithComps.AllComps[i] is CompResourceStorage cps)
                    compRS = cps;
            }

            this.FailOn(() => compRS == null);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(compRS.Props.extractOptions.extractTime).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOnDestroyedOrNull(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);

            Toil finalize = new Toil
            {
                initAction = delegate
                {
                    compRS.DrawResource(compRS.extractResourceAmount);

                    var opt = compRS.Props.extractOptions;
                    Thing createdThing = ThingMaker.MakeThing(opt.thing);
                    createdThing.stackCount = opt.extractAmount;
                    GenSpawn.Spawn(createdThing, pawn.Position, Map, WipeMode.VanishOrMoveAside);

                    Map.designationManager.DesignationOn(thingWithComps, PSDefOf.PS_Drain)?.Delete();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return finalize;
        }
    }
}