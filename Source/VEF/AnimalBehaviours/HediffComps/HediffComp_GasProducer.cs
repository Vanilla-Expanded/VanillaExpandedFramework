
using Verse;
using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;



namespace VEF.AnimalBehaviours
{
    public class HediffComp_GasProducer : HediffComp
    {


        public HediffCompProperties_GasProducer Props
        {
            get
            {
                return (HediffCompProperties_GasProducer)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (this.parent.pawn.IsHashIntervalTick(Props.timer, delta) && this.parent.pawn.Map != null)
            {
                GasUtility.AddGas(this.parent.pawn.Position, this.parent.pawn.Map, Props.gasType, Props.amount);
            }

        }




    }
}
