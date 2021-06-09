using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_DropOnDeath : CompProperties
    {

        //CompDropOnDeath makes an animal drop a resource when killed

        public int resourceAmount = 1;
        public string resourceDef = null;
        public float dropChance = 1f;

        //CompProperties_DropOnDeath allows an animal to produce random items

        public bool isRandom = false;
        public List<string> randomItems = null;



        public CompProperties_DropOnDeath()
        {
            this.compClass = typeof(CompDropOnDeath);
        }


    }
}
