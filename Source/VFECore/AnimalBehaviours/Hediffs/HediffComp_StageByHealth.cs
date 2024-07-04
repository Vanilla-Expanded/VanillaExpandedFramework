

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace AnimalBehaviours
{
    class HediffComp_StageByHealth : HediffComp
    {


        public HediffCompProperties_StageByHealth Props
        {
            get
            {
                return (HediffCompProperties_StageByHealth)this.props;
            }
        }


        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (this.parent.pawn.IsHashIntervalTick(500))
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
