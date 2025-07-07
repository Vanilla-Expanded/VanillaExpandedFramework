
using Verse;
using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;



namespace VEF.AnimalBehaviours
{
    public class HediffComp_Polluter : HediffComp
    {


        public HediffCompProperties_Polluter Props
        {
            get
            {
                return (HediffCompProperties_Polluter)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (this.parent.pawn.IsHashIntervalTick(Props.timer, delta) && this.parent.pawn.Map != null)
            {
                PollutionUtility.GrowPollutionAt(this.parent.pawn.Position, this.parent.pawn.Map, Props.amount,null,true);
            }

        }




    }
}
