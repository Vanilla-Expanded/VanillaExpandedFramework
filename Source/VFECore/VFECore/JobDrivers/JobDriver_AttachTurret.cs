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
            this.FailOn(() => !TargetA.Thing.TryGetComp<CompPowerTrader>().PowerOn);
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
            Toil waitForMachineToReturn = Toils_General.Wait(60)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            waitForMachineToReturn.AddPreInitAction(delegate
            {
                var compMachineStation = TargetA.Thing.TryGetComp<CompMachineChargingStation>();
                compMachineStation.wantsRest = true;
            });
            yield return waitForMachineToReturn;
            yield return Toils_Jump.JumpIf(waitForMachineToReturn, delegate
            {
                if (TargetA.Thing is IBedMachine bedMachine)
                {
                    return bedMachine.occupant == null;
                }
                return false;
            });
            var compMachineStation = TargetA.Thing.TryGetComp<CompMachineChargingStation>();
            var compMachine = compMachineStation.myPawn.TryGetComp<CompMachine>();
            var attachTurret = new Toil();
            attachTurret.defaultDuration = (int)compMachine.turretToInstall.GetStatValueAbstract(StatDefOf.WorkToBuild, null);
            attachTurret.initAction = delegate
            {
                GenClamor.DoClamor(pawn, 15f, ClamorDefOf.Construction);
            };
            attachTurret.WithEffect(() =>
            {
                var def = compMachine.turretToInstall;
                if (def.constructEffect != null)
                {
                    return def.constructEffect;
                }
                return EffecterDefOf.ConstructMetal;
            }, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);
            attachTurret.defaultCompleteMode = ToilCompleteMode.Delay;
            attachTurret.activeSkill = () => SkillDefOf.Construction;
            yield return attachTurret;
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
                var compMachineStation = thing.TryGetComp<CompMachineChargingStation>();
                var compMachine = compMachineStation.myPawn.TryGetComp<CompMachine>();
                compMachine.AttachTurret();
                compMachineStation.wantsRest = false;
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
	}
}

