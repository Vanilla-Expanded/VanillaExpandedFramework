
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours

{
    public class CompProperties_EatWeirdFood : CompProperties
    {

        public List<string> customThingToEat = null;
        public int nutrition = 2;
        public bool fullyDestroyThing = false;
        public float percentageOfDestruction = 0.1f;
        public bool digThingIfMapEmpty = false;
        public string thingToDigIfMapEmpty = "";
        public int customAmountToDig = 1;
        public string hediffWhenEaten = "";
        public bool advanceLifeStage = false;
        public int advanceAfterXFeedings = 1;
        public string defToAdvanceTo = "";
        public bool fissionAfterXFeedings = false;
        public string defToFissionTo = "";
        public int numberOfOffspring = 2;
        public bool fissionOnlyIfTamed = true;
        public bool drainBattery = false;
        public float percentageDrain = 0.1f;
        public bool needsWater = true;


        public CompProperties_EatWeirdFood()
        {
            this.compClass = typeof(CompEatWeirdFood);
        }
    }
}
