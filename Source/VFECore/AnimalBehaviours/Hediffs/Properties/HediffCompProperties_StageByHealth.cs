using System;
using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class HediffCompProperties_StageByHealth : HediffCompProperties
    {

        public float lowHealthStageIndex = 0.1f;
        public float highHealthStageIndex = 1f;
        public float healthThreshold = 0.5f;

        public HediffCompProperties_StageByHealth()
        {
            this.compClass = typeof(HediffComp_StageByHealth);
        }
    }
}
