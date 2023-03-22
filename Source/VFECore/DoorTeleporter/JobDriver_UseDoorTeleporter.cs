using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VFECore
{
    public class JobDriver_UseDoorTeleporter : JobDriver
    {
        public IntVec3 targetCell;
        public DoorTeleporter Origin => this.job.targetA.Thing as DoorTeleporter;
        public DoorTeleporter Dest => this.job.globalTarget.Thing as DoorTeleporter;
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override string GetReport() =>
            JobUtility.GetResolvedJobReportRaw(this.job.def.reportString, this.Origin.Name, this.Origin, this.Dest.Name, this.Dest, null, null);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.AddEndCondition(() => this.Dest is null || !this.Dest.Spawned || this.Dest.Destroyed ? JobCondition.Incompletable : JobCondition.Ongoing);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil wait = Toils_General.Wait(16, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A).WithEffect(EffecterDefOf.Skip_Entry, TargetIndex.A);
            wait.AddPreTickAction(() =>
            {
                Origin.DoTeleportEffects(this.pawn, this.ticksLeftThisToil, this.job.globalTarget.Map, ref targetCell, Dest);
            });
            yield return wait;
            yield return Toils_General.DoAtomic(() =>
            {
                Origin.Teleport(pawn, this.job.globalTarget.Map, this.targetCell);
            });
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.targetCell, nameof(this.targetCell));
        }
    }
}
