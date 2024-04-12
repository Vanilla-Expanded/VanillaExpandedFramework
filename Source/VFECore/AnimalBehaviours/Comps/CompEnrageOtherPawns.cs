using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace AnimalBehaviours
{
    public class CompEnrageOtherPawns : ThingComp
    {

      

        public CompProperties_EnrageOtherPawns Props
        {
            get
            {
                return (CompProperties_EnrageOtherPawns)this.props;
            }
        }

        

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(Props.checkingInterval))
            {
                if (this.parent.Map != null) {
                    Pawn pawn = this.parent as Pawn;
                    if (pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.Manhunter || pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.ManhunterPermanent)
                    {
                        List<Pawn> pawnsaffected = (from x in parent.Map.mapPawns.AllPawnsSpawned
                        where Props.pawnkinddefsToAffect.Contains(x.kindDef) select x).ToList();
                        foreach(Pawn pawnaffected in pawnsaffected)
                        {
                            pawnaffected.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, false,null, false, false, false);
                        }
                    }

                }
                


            }
        }


    }
}

