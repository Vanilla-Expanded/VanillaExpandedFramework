﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace VEF.Apparels
{

    public class JobDriver_EquipShield : JobDriver_Equip
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            var toil = ToilMaker.MakeToil();
            toil.initAction = delegate ()
            {
                ThingWithComps equipmentStack = (ThingWithComps)job.targetB.Thing;
                ThingWithComps equippedThing;
                if (equipmentStack.def.stackLimit > 1 && equipmentStack.stackCount > 1)
                {
                    equippedThing = (ThingWithComps)equipmentStack.SplitOff(1);
                }
                else
                {
                    equippedThing = equipmentStack;
                    if (equippedThing.Spawned)
                    {
                        equippedThing.DeSpawn(DestroyMode.Vanish);
                    }
                    else
                    {
                        var parentHolder = equippedThing.ParentHolder;
                        parentHolder.GetDirectlyHeldThings().Remove(equippedThing);
                    }
                }

                pawn.MakeRoomForShield(equippedThing);
                pawn.apparel.Wear((Apparel)equippedThing);
                if (pawn.outfits != null && job.playerForced)
                {
                    pawn.outfits.forcedHandler.SetForced((Apparel)equippedThing, forced: true);
                }
                if (equipmentStack.def.soundInteract != null)
                {
                    equipmentStack.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }

    }

}
