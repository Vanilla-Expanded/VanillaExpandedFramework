

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_StageBySunlight : HediffComp
    {


        public HediffCompProperties_StageBySunlight Props
        {
            get
            {
                return (HediffCompProperties_StageBySunlight)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (this.parent.pawn.IsHashIntervalTick(500, delta))
            {
                if (this.parent.pawn.Map != null && this.parent.pawn.Position.InSunlight(this.parent.pawn.Map))
                {
                    this.parent.Severity = Props.sunlightStageIndex;
                }else this.parent.Severity = Props.sunlessStageIndex;
            }


        }



    }
}
