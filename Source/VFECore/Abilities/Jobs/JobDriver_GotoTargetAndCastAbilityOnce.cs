namespace VFECore.Abilities
{
    using System.Collections.Generic;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    public class JobDriver_GotoTargetAndCastAbilityOnce : JobDriver_CastAbilityOnce
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (this.pawn != this.TargetA.Thing)
            {
                foreach (var toil in GotoToils())
                {
                    yield return toil;
                }
            }
            foreach (var toil in base.MakeNewToils())
            {
                yield return toil;
            }
            AddFinishAction(delegate
            {
                if (job.targetA.Thing is Pawn victim && victim.CurJobDef == VFE_DefOf_Abilities.VFEA_StandAndFaceTarget)
                {
                    victim.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            });
        }

        private IEnumerable<Toil> GotoToils()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                pawn.pather.StopDead();
            };
            toil.tickAction = delegate
            {
                Thing target = job.targetA.Thing;
                pawn.rotationTracker.FaceTarget(target);
                Map map = pawn.Map;
                if (GenSight.LineOfSight(pawn.Position, target.Position, map, skipFirstCell: true) 
                    && pawn.Position.DistanceTo(target.Position) <= CompAbilities.currentlyCasting.def.distanceToTarget
                    && (!pawn.pather.Moving || pawn.pather.nextCell.GetDoor(map) == null))
                {
                    pawn.pather.StopDead();
                    pawn.rotationTracker.FaceTarget(target);
                    if (target is Pawn victim)
                    {
                        victim.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VFE_DefOf_Abilities.VFEA_StandAndFaceTarget, pawn));
                    }
                    ReadyForNextToil();
                }
                else if (!pawn.pather.Moving)
                {
                    if (CompAbilities.currentlyCasting.def.distanceToTarget <= 1.5f)
                    {
                        pawn.pather.StartPath(TargetA, PathEndMode.Touch);
                    }
                    else
                    {
                        IntVec3 intVec = IntVec3.Invalid;
                        for (int i = 0; i < 9 && (i != 8 || !intVec.IsValid); i++)
                        {
                            IntVec3 intVec2 = target.Position + GenAdj.AdjacentCellsAndInside[i];
                            if (intVec2.InBounds(map) && intVec2.Walkable(map) && intVec2 != pawn.Position &&
                            InteractionUtility.IsGoodPositionForInteraction(intVec2, target.Position, map)
                            && pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly)
                            && (!intVec.IsValid || pawn.Position.DistanceToSquared(intVec2) < pawn.Position.DistanceToSquared(intVec)))
                            {
                                intVec = intVec2;
                            }
                        }
                        if (intVec.IsValid)
                        {
                            pawn.pather.StartPath(intVec, PathEndMode.OnCell);
                        }
                        else
                        {
                            ReadyForNextToil();
                        }
                    }

                }
            };
            toil.handlingFacing = true;
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}
