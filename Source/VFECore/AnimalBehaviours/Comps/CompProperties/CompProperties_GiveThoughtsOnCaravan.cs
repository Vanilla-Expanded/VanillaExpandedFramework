using Verse;
using System.Collections.Generic;
using RimWorld;


namespace AnimalBehaviours
{
    public class CompProperties_GiveThoughtsOnCaravan : CompProperties
    {

      
        public int intervalTicks = 30000;
        public ThoughtDef thought;
        public bool causeNegativeAtRandom = false;
        public float randomNegativeChance = 0.1f;
        public ThoughtDef negativeThought;

       
        public CompProperties_GiveThoughtsOnCaravan()
        {
            this.compClass = typeof(CompGiveThoughtsOnCaravan);
        }


    }
}
