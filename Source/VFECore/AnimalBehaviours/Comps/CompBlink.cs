using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnimalBehaviours
{
    public class CompBlink : ThingComp
    {

        int tickCounter = 0;


        public CompProperties_Blink Props
        {
            get
            {
                return (CompProperties_Blink)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (AnimalBehaviours_Settings.flagBlinkMechanics) {
                tickCounter++;
                if (tickCounter > Props.blinkInterval)
                {

                    IntVec3 loc = IntVec3.Invalid;
                    Pawn pawn = this.parent as Pawn;
                    if (pawn.Map != null)
                    {
                        if (pawn.CurJob.def == JobDefOf.GotoWander || pawn.CurJob.def == JobDefOf.Wait_Wander || pawn.CurJob.def == JobDefOf.Wait_MaintainPosture)
                        {
                            if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => x.DistanceTo(this.parent.Position) < Props.distance.RandomInRange,
                            this.parent.Map, out loc))
                            {
                                if (Props.warpEffect && !Props.effectOnlyWhenManhunter)
                                {
                                    FleckMaker.Static(this.parent.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 10f);
                                }
                                pawn.pather.StopDead();
                                pawn.Position = loc;
                                pawn.pather.ResetToCurrentPosition();
                                IntVec3 loc2 = IntVec3.Invalid;
                                CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 10, null, out loc2);
                                pawn.pather.StartPath(loc2, PathEndMode.ClosestTouch);
                            }
                        }
                        else if ((pawn.CurJob.def == JobDefOf.AttackMelee || pawn.mindState.mentalStateHandler.InMentalState) && Props.blinkWhenManhunter)
                        {
                            if (this.parent.Position.DistanceTo(pawn.CurJob.targetA.Cell) > 2)
                            {
                                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => x.DistanceTo(pawn.CurJob.targetA.Cell) < Props.distance.RandomInRange,
                            this.parent.Map, out loc))
                                {
                                    if (Props.warpEffect)
                                    {
                                        FleckMaker.Static(this.parent.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 10f);
                                    }
                                    pawn.pather.StopDead();
                                    pawn.Position = loc;
                                    pawn.pather.ResetToCurrentPosition();
                                    pawn.pather.StartPath(pawn.CurJob.targetA.Cell, PathEndMode.ClosestTouch);
                                }

                            }


                        }

                    }


                    tickCounter = 0;
                }

            }
           
        }


    }
}

