using System;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_RegrowLimbs : HediffCompProperties
    {

        public HediffDef regeneratingHediff;

        public HediffCompProperties_RegrowLimbs()
        {
            this.compClass = typeof(HediffComp_RegrowLimbs);
        }
    }
}
