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
        public Effecter destEffecter;
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
                Origin.DoTeleportEffects(this);
            });
            wait.AddFinishAction(() => { this.destEffecter?.Cleanup(); });
            yield return wait;
            yield return Toils_General.DoAtomic(() =>
            {
                Pawn localPawn = this.pawn;
                IntVec3 cell = this.targetCell;
                Map map = this.job.globalTarget.Map;
                bool drafted = localPawn.Drafted;
                localPawn.teleporting = true;
                localPawn.ClearAllReservations(false);
                localPawn.ExitMap(false, Rot4.Invalid);
                localPawn.teleporting = false;
                GenSpawn.Spawn(localPawn, cell, map);
                localPawn.drafter.Drafted = drafted;
            });
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.targetCell, nameof(this.targetCell));
        }
    }
}
