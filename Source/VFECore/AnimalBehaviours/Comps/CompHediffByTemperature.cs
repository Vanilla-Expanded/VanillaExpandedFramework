using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;
using System.Collections;

namespace AnimalBehaviours
{
    public class CompHediffByTemperature : ThingComp
    {


        public CompProperties_HediffByTemperature Props
        {
            get
            {
                return (CompProperties_HediffByTemperature)this.props;
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

                    float temperatureNow = thisPawn.Position.GetTemperature(thisPawn.Map);


                    if(Props.doTemperatureAbove && temperatureNow > Props.temperatureAbove)
                    {

                        if (!thisPawn.health.hediffSet.HasHediff(Props.hediffAbove))
                        {
                            thisPawn.health.AddHediff(Props.hediffAbove, thisPawn.health.hediffSet.GetBodyPartRecord(Props.bodyPart));
                        }
                        thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffAbove).Severity = Props.severity;
                        //this line resets the PawnRenderNodes to apply a graphic change
                        thisPawn.Drawer.renderer.SetAllGraphicsDirty();

                    }
                    else
                    {
                        if (Props.hediffAbove!=null && thisPawn.health.hediffSet.HasHediff(Props.hediffAbove))
                        {
                            thisPawn.health.RemoveHediff(thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffAbove));
                            thisPawn.Drawer.renderer.SetAllGraphicsDirty();
                        }

                    }

                    if (Props.doTemperatureBelow && temperatureNow < Props.temperatureBelow)
                    {

                        if (!thisPawn.health.hediffSet.HasHediff(Props.hediffBelow))
                        {
                            thisPawn.health.AddHediff(Props.hediffBelow, thisPawn.health.hediffSet.GetBodyPartRecord(Props.bodyPart));
                        }
                        thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffBelow).Severity = Props.severity;
                        //this line resets the PawnRenderNodes to apply a graphic change
                        thisPawn.Drawer.renderer.SetAllGraphicsDirty();

                    }
                    else
                    {
                        if (Props.hediffBelow != null && thisPawn.health.hediffSet.HasHediff(Props.hediffBelow))
                        {
                            thisPawn.health.RemoveHediff(thisPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffBelow));
                            thisPawn.Drawer.renderer.SetAllGraphicsDirty();
                        }

                    }
                }

            }
        }


    }
}


