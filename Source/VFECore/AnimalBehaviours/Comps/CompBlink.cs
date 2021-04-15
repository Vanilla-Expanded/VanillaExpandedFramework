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
            tickCounter++;
            if (tickCounter > Props.blinkInterval)
            {

                IntVec3 loc = IntVec3.Invalid;
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => x.DistanceTo(this.parent.Position) < Props.distance.RandomInRange,
                    this.parent.Map, out loc))
                {

                    Pawn pawn = this.parent as Pawn;
                    if (pawn.CurJob.def == JobDefOf.GotoWander)
                    {
                        if (Props.warpEffect)
                        {
                            MoteMaker.MakeStaticMote(this.parent.Position, this.parent.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
                        }
                        pawn.pather.StopDead();
                        pawn.Position = loc;
                        pawn.pather.ResetToCurrentPosition();
                        IntVec3 loc2 = IntVec3.Invalid;
                        CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 10, null, out loc2);
                        pawn.pather.StartPath(loc2, PathEndMode.ClosestTouch);
                    }

                }
                tickCounter = 0;
            }
        }


    }
}

