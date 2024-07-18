using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;
using System.Collections;

namespace AnimalBehaviours
{
    public class CompHediffAfterHealthLoss : ThingComp
    {


        public CompProperties_HediffAfterHealthLoss Props
        {
            get
            {
                return (CompProperties_HediffAfterHealthLoss)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();


        }

        public override void CompTick()
        {
            base.CompTick();



            if (this.parent.IsHashIntervalTick(Props.tickInterval))
            {
                Pawn thisPawn = this.parent as Pawn;
                if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed)
                {
                    
                    if (thisPawn.health.summaryHealth.SummaryHealthPercent < ((float)(Props.healthPercent) / 100))
                    {
                        if (!thisPawn.health.hediffSet.HasHediff(Props.hediff))
                        {
                            thisPawn.health.AddHediff(Props.hediff, thisPawn.health.hediffSet.GetBodyPartRecord(Props.bodyPart));
                        }
                        thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff).Severity = Props.severity;
                        //this line resets the PawnRenderNodes to apply a graphic change
                        thisPawn.Drawer.renderer.SetAllGraphicsDirty();
                    }
                    else
                    {
                        if (thisPawn.health.hediffSet.HasHediff(Props.hediff))
                        {
                            thisPawn.health.RemoveHediff(thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff));
                            thisPawn.Drawer.renderer.SetAllGraphicsDirty();
                        }
                        
                    }
                }

            }
        }


    }
}


