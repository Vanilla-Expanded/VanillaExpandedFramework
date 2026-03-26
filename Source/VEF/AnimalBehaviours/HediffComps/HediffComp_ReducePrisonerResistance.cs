
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_ReducePrisonerResistance : HediffComp
    {
        public HediffCompProperties_ReducePrisonerResistance Props
        {
            get
            {
                return (HediffCompProperties_ReducePrisonerResistance)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Pawn.IsHashIntervalTick(Props.checkingInterval, delta))
            {
                Pawn.guest.resistance = Mathf.Max(0f, Pawn.guest.resistance - (Props.checkingInterval*Props.resistancePerTick));
            }

        }


    }
}
