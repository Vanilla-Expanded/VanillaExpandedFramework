

using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using Verse.Sound;
using Verse.Noise;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_StageByTemperature : HediffComp
    {


        public HediffCompProperties_StageByTemperature Props
        {
            get
            {
                return (HediffCompProperties_StageByTemperature)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (this.parent.pawn.IsHashIntervalTick(500, delta)&& this.parent.pawn.Map!=null)
            {
                float tempNow = parent.pawn.Position.GetTemperature(parent.pawn.Map);

                float percentage = Mathf.Clamp((tempNow - Props.minTemp) / (Props.maxTemp - Props.minTemp),0,1);

                

                this.parent.Severity = percentage;



            }


        }



    }
}
