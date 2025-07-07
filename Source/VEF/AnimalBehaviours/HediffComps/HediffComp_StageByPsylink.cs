

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_StageByPsylink : HediffComp
    {


        public HediffCompProperties_StageByPsylink Props
        {
            get
            {
                return (HediffCompProperties_StageByPsylink)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (this.parent.pawn.IsHashIntervalTick(500, delta) && this.parent.pawn.Map != null && ModsConfig.RoyaltyActive)
            {

                float psyLinkLevel = this.parent.pawn.GetPsylinkLevel();

                float severity = psyLinkLevel / 6;

                if (severity == 0) { severity = 0.01f; }

                 this.parent.Severity = severity;
            }


        }



    }
}
