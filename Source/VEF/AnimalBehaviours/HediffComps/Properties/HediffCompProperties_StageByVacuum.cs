using System;
using Verse;
using System.Collections.Generic;


namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_StageByVacuum : HediffCompProperties
    {

        public float notVacuumStageIndex = 0.1f;
        public float vacuumStageIndex = 1f;

        public HediffCompProperties_StageByVacuum()
        {
            this.compClass = typeof(HediffComp_StageByVacuum);
        }
    }
}
