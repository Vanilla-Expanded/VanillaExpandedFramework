
using Verse;
using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;



namespace AnimalBehaviours
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



        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (this.parent.pawn.IsHashIntervalTick(Props.timer) && this.parent.pawn.Map != null)
            {
                GasUtility.AddGas(this.parent.pawn.Position, this.parent.pawn.Map, Props.gasType, Props.amount);
            }

        }




    }
}
