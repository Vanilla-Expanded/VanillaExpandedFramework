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
        public bool addHediffOnce = false;

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
            Scribe_Values.Look<bool>(ref this.addHediffOnce, "addHediffOnce", false, false);

        }

        public override void CompTick()
        {
            base.CompTick();
          
            if(!addHediffOnce)
            {
                //Only check things every tickInterval
                if (this.parent.IsHashIntervalTick(Props.tickInterval))
                {
                    Pawn thisPawn = this.parent as Pawn;
                    if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed)
                    {
                        //If pawn's health reaches a threshold
                        if (thisPawn.health.summaryHealth.SummaryHealthPercent < ((float)(Props.healthPercent) / 100))
                        {
                            //apply hediff
                            thisPawn.health.AddHediff(Props.hediff, thisPawn.health.hediffSet.GetBodyPartRecord(Props.bodyPart));
                            thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff).Severity = Props.severity;
                            //this line resets the PawnRenderNodes to apply a graphic change
                            thisPawn.Drawer.renderer.SetAllGraphicsDirty();
                            addHediffOnce = true;
                        }
                    }

                }
            }

            
        }
    }
}

