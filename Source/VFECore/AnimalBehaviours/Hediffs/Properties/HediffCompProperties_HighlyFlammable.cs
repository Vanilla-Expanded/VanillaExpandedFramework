using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class HediffCompProperties_HighlyFlammable : HediffCompProperties
    {
        //This is equivalent to the CompHighlyFlammable class, but just adds things through a hediff

        public DamageDef damageToInflict = null;
        public float damageAmount = 15;
        public int tickInterval = 50;
        public bool sunlightBurns = false;

        public HediffCompProperties_HighlyFlammable()
        {
            this.compClass = typeof(HediffComp_HighlyFlammable);
        }
    }
}
