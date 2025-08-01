﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;


namespace VEF.AnimalBehaviours
{
    public class JobDriver_DestroyItem : JobDriver
    {




        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A).Thing, this.job, 1, -1, null);
        }



        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Thing itemToDestroy = (Thing)this.job.GetTarget(TargetIndex.A).Thing;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(1200).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            var toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                itemToDestroy.DeSpawn();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;


        }
    }
}
