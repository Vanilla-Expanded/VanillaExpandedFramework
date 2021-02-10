
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompProperties_DigWhenHungry : CompProperties
    {

        //Similar to CompProperties_DigPeriodically, but only when hungry

        public string customThingToDig = "";
        public int customAmountToDig = 1;

        //timeToDig has a misleading name. It is a minimum counter. The user will not dig if less than timeToDig ticks have passed.
        //This is done to avoid an animal digging again if it's still hungry.
        public int timeToDig = 40000;
        //A list of acceptable terrains can be specified
        public List<string> acceptedTerrains = null;
        //Should items be spawned forbidden?
        public bool spawnForbidden = false;
        //Should the animal dig for items even if it's not hungry, every timeToDigForced ticks?
        public bool digAnywayEveryXTicks = true;
        public int timeToDigForced = 120000;

        public CompProperties_DigWhenHungry()
        {
            this.compClass = typeof(CompDigWhenHungry);
        }


    }
}
