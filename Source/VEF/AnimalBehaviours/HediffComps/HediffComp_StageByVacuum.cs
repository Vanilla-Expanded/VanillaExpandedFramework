

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_StageByVacuum : HediffComp
    {


        public HediffCompProperties_StageByVacuum Props
        {
            get
            {
                return (HediffCompProperties_StageByVacuum)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (this.parent.pawn.IsHashIntervalTick(500, delta))
            {
                if (parent.pawn.Map?.BiomeAt(parent.pawn.Position)?.inVacuum == true)
                {
                    this.parent.Severity = Props.vacuumStageIndex;
                }
                else this.parent.Severity = Props.notVacuumStageIndex;
            }


        }



    }
}
