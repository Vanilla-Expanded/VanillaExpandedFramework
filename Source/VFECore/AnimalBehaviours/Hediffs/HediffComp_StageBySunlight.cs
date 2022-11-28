

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace AnimalBehaviours
{
    class HediffComp_StageBySunlight : HediffComp
    {


        public HediffCompProperties_StageBySunlight Props
        {
            get
            {
                return (HediffCompProperties_StageBySunlight)this.props;
            }
        }


        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (this.parent.pawn.IsHashIntervalTick(500))
            {
                if (this.parent.pawn.Map != null && this.parent.pawn.Position.InSunlight(this.parent.pawn.Map))
                {
                    this.parent.Severity = Props.sunlightStageIndex;
                }else this.parent.Severity = Props.sunlessStageIndex;
            }


        }



    }
}
