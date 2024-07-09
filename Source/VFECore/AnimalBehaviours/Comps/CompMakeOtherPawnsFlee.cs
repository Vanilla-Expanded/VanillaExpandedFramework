using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Reflection;
using Verse.AI;

namespace AnimalBehaviours
{
    public class CompMakeOtherPawnsFlee : ThingComp
    {
        private static readonly List<Thing> tmpPawns = new List<Thing>();


        public CompProperties_MakeOtherPawnsFlee Props
        {
            get
            {
                return (CompProperties_MakeOtherPawnsFlee)this.props;
            }
        }



        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(Props.checkingInterval))
            {
                if (this.parent.Map != null)
                {
                    Pawn pawn = this.parent as Pawn;
                    if (pawn.CurJob.def == JobDefOf.AttackMelee || pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.Manhunter || pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.ManhunterPermanent)
                    {
                        List<Pawn> pawnsaffected = (from x in parent.Map.mapPawns.AllPawnsSpawned
                                                    where Props.pawnkinddefsToAffect.Contains(x.kindDef)
                                                    select x).ToList();
                        foreach (Pawn pawnaffected in pawnsaffected)
                        {

                            Region region = pawn.GetRegion();
                            if (region == null)
                            {
                                return;
                            }
                            RegionTraverser.BreadthFirstTraverse(region, (Region from, Region reg) => reg.door == null || reg.door.Open, delegate (Region reg)
                            {
                                List<Thing> list = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
                                for (int i = 0; i < list.Count; i++)
                                {
                                    IAttackTarget attackTarget;
                                    Pawn pawn2;
                                    if (list[i] != pawn && (attackTarget = list[i] as IAttackTarget) != null && !attackTarget.ThreatDisabled(null) && (pawn2 = list[i] as Pawn) != null && (pawn2.HostileTo(pawn) || pawn2.RaceProps.Humanlike) && GenSight.LineOfSightToThing(pawn.Position, pawn2, pawn.Map, skipFirstCell: true))
                                    {
                                        tmpPawns.Add(pawn2);
                                    }
                                }
                                return false;
                            }, 9);
                            if (tmpPawns.Any())
                            {
                                IntVec3 fleeDest = CellFinderLoose.GetFleeDest(pawn, tmpPawns,50);
                                tmpPawns.Clear();
                                if (fleeDest.IsValid && fleeDest != pawn.Position)
                                {
                                    Job job = JobMaker.MakeJob(JobDefOf.FleeAndCowerShort, fleeDest);
                                    job.checkOverrideOnExpire = true;
                                    job.expiryInterval = 600;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }
                            }



                        }
                    }

                }



            }
        }

       

    }
}

