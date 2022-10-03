using System;
using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class HediffCompProperties_StageBySunlight : HediffCompProperties
    {

        public float sunlightStageIndex = 0.1f;
        public float sunlessStageIndex = 1f;

        public HediffCompProperties_StageBySunlight()
        {
            this.compClass = typeof(HediffComp_StageBySunlight);
        }
    }
}
