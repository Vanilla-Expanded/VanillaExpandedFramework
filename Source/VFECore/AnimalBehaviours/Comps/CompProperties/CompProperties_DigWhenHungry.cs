
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompProperties_DigWhenHungry : CompProperties
    {

        //Similar to CompProperties_DigPeriodically, but only when hungry

        public string customThingToDig = "";
        public int customAmountToDig = 1;
        public int timeToDig = 40000;
        public List<string> acceptedTerrains = null;

        public CompProperties_DigWhenHungry()
        {
            this.compClass = typeof(CompDigWhenHungry);
        }


    }
}
