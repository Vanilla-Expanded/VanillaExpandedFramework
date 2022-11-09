

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace AnimalBehaviours
{
    class HediffComp_StageByPsylink : HediffComp
    {


        public HediffCompProperties_StageByPsylink Props
        {
            get
            {
                return (HediffCompProperties_StageByPsylink)this.props;
            }
        }


        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);


            if (this.parent.pawn.IsHashIntervalTick(500) && this.parent.pawn.Map != null && ModsConfig.RoyaltyActive)
            {

                float psyLinkLevel = this.parent.pawn.GetPsylinkLevel();

                float severity = psyLinkLevel / 6;

                if (severity == 0) { severity = 0.01f; }

                 this.parent.Severity = severity;
            }


        }



    }
}
