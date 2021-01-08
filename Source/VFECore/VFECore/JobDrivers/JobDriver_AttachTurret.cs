using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Buildings;

namespace VFE.Mechanoids.AI.JobDrivers
{
    class JobDriver_AttachTurret : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            return pawn.Reserve(job.GetTarget(TargetIndex.A),job,1,-1,null,errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil getNextIngredient = Toils_General.Label();
            yield return getNextIngredient;
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
            yield return findPlaceTarget;
            yield return JobDriver_RepairMachine.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, storageMode: false);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
            Toil waitForMachineToReturn = Toils_General.Label();
            yield return waitForMachineToReturn;
            yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);
            yield return Toils_Jump.JumpIf(waitForMachineToReturn, () => ((Building_BedMachine)TargetA).occupant == null);
            yield return Finalize(TargetIndex.A, TargetIndex.B);
        }

        static Toil Finalize(TargetIndex buildingIndex, TargetIndex materialIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Job curJob = toil.actor.CurJob;
                Thing thing = curJob.GetTarget(buildingIndex).Thing;
				foreach (ThingCountClass toDestroy in toil.actor.CurJob.placedThings)
					toDestroy.thing.Destroy();
                thing.TryGetComp<CompMachineChargingStation>().myPawn.TryGetComp<CompMachine>().AttachTurret(thing.TryGetComp<CompMachineChargingStation>().turretToInstall);
                thing.TryGetComp<CompMachineChargingStation>().wantsRest = false;
                thing.TryGetComp<CompMachineChargingStation>().turretToInstall = null;
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
	}
}

