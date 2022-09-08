using Verse;
using System.Collections.Generic;
using RimWorld;


namespace AnimalBehaviours
{
    public class CompProperties_GiveThoughtsOnCaravan : CompProperties
    {

        //CompGiveThoughtsOnCaravan scans the pawn list of a caravan this animal is a part of, 
        //and confers "thought" on each of them. A "negativeThought" can be configured to be created at random

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
