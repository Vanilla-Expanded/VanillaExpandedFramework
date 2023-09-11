
using Verse;
using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;



namespace AnimalBehaviours
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



        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (this.parent.pawn.IsHashIntervalTick(Props.timer) && this.parent.pawn.Map != null)
            {
                PollutionUtility.GrowPollutionAt(this.parent.pawn.Position, this.parent.pawn.Map, Props.amount,null,true);
            }

        }




    }
}
