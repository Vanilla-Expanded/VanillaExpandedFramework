

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace AnimalBehaviours
{
    public class HediffCompProperties_CorpseDecayer : HediffCompProperties
    {

        public int radius = 5;
        public int tickInterval = 500;
        public int decayOnHitPoints = 1;
        public float nutritionGained = 0.2f;
        public string corpseSound = "";

        public bool causeThoughtNearby = false;
        public int radiusForThought;
        public ThoughtDef thought;

        public HediffCompProperties_CorpseDecayer()
        {
            this.compClass = typeof(HediffComp_CorpseDecayer);
        }
    }
}
