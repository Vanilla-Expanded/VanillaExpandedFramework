using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.Collections;

namespace AnimalBehaviours
{
    public class CompExtremeXenophobia : ThingComp
    {
        public int tickCounter = 0;
        public List<Pawn> pawnList = new List<Pawn>();
        public Pawn thisPawn;
      
        public CompProperties_ExtremeXenophobia Props
        {
            get
            {
                return (CompProperties_ExtremeXenophobia)this.props;
            }
        }

     

        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;
            //Only do anything every berserkRate
            if (tickCounter > Props.berserkRate)
            {
                if (this.parent.Map != null) {
                    Pawn thisPawn = this.parent as Pawn;
                    foreach (Pawn pawn in this.parent.Map.mapPawns.FreeColonists)
                    {
                        if (pawn != null && pawn.IsColonist && !Props.AcceptedDefnames.Contains(pawn.def.defName))
                        {
                            
                            thisPawn.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("VEF_XenophobicRage", true), null, true, false, false,null, false);
                           
                        }
                    }

                }
                
                tickCounter = 0;
            }
        }
    }
}

