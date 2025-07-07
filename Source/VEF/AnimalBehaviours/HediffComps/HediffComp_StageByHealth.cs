

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_StageByHealth : HediffComp
    {


        public HediffCompProperties_StageByHealth Props
        {
            get
            {
                return (HediffCompProperties_StageByHealth)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (this.parent.pawn.IsHashIntervalTick(500, delta))
            {
                if (this.parent.pawn.Map != null && this.parent.pawn.health.summaryHealth.SummaryHealthPercent>=Props.healthThreshold)
                {
                    this.parent.Severity = Props.highHealthStageIndex;
                }
                else this.parent.Severity = Props.lowHealthStageIndex;
            }


        }



    }
}
