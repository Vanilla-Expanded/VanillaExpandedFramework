using System;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_PsyfocusRegeneration : HediffCompProperties
    {
        //Instead of health, this hediff comp regenerates psyfocus

        public int rateInTicks = 1000;
        public float regenAmount = 0.1f;
       
        public HediffCompProperties_PsyfocusRegeneration()
        {
            this.compClass = typeof(HediffComp_PsyfocusRegeneration);
        }
    }
}
