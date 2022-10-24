using System;
using Verse;
using System.Collections.Generic;

namespace VanillaGenesExpanded
{
    public class HediffCompProperties_HumanEggLayer : HediffCompProperties
    {
        public ThingDef eggUnfertilizedDef;
        public ThingDef eggFertilizedDef;
        public float eggLayIntervalDays = 1f;
    
        public bool eggLayFemaleOnly = true;
        public float eggProgressUnfertilizedMax = 1f;
        public bool maleDominant = false;
        public bool femaleDominant = false;

        public HediffCompProperties_HumanEggLayer()
        {
            this.compClass = typeof(HediffComp_HumanEggLayer);
        }
    }
}
