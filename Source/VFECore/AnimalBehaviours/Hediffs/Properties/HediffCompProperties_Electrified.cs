using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class HediffCompProperties_Electrified : HediffCompProperties
    {
        //This is equivalent to the CompElectrified class, but just adds things through a hediff

        public int electroRate = 0;
        public int electroRadius = 0;
        public int electroChargeAmount = 1;
        public List<string> batteriesToAffect = null;

        public HediffCompProperties_Electrified()
        {
            this.compClass = typeof(HediffComp_Electrified);
        }
    }
}
