using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class HediffCompProperties_PassiveRegenerator : HediffCompProperties
    {
        
        public int radius = 1;
        public int tickInterval = 1000;
        public float healAmount = 0.1f;
        public bool healAll = true;
        public bool showEffect = false;
        public bool needsToBeTamed = false;

        public HediffCompProperties_PassiveRegenerator()
        {
            this.compClass = typeof(HediffComp_PassiveRegenerator);
        }
    }
}
