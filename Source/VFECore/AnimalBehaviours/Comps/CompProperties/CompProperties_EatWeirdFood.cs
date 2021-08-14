
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours

{
    public class CompProperties_EatWeirdFood : CompProperties
    {

        //Makes the animal have customizable eating habits

        //A list of defNames the animal can eat. Note that they can not exist, making it possible for you
        //to add items from mods, and if the mod isn't there, the creature will just ignore it
        public List<string> customThingToEat = null;

        //Nutrition gained when eaten (overrides item's nutrition if it has one)
        public int nutrition = 2;

        //If set to true, it will destroy item after one feeding. If not, it will damage it
        public bool fullyDestroyThing = false;
        public float percentageOfDestruction = 0.1f;

        //If set to true, it will automatically deduct from a stack
        public bool ignoreUseHitPoints = false;

        //If no items are present, the animal can dig for them
        public bool digThingIfMapEmpty = false;
        public string thingToDigIfMapEmpty = "";
        public int customAmountToDig = 1;

        //Receive a hediff after eating
        public string hediffWhenEaten = "";

        //Advance life stages (change to a different animal def) after eating
        public bool advanceLifeStage = false;
        public int advanceAfterXFeedings = 1;
        public string defToAdvanceTo = "";
        public bool fissionAfterXFeedings = false;
        public string defToFissionTo = "";
        public int numberOfOffspring = 2;
        public bool fissionOnlyIfTamed = true;

        //Instead of eating a battery, drain its power
        public bool drainBattery = false;
        public float percentageDrain = 0.1f;

        //If food is a plant, receive nutrition according to growth
        public bool areFoodSourcesPlants = false;       

        //This is just for No Water No Life compatibility
        public bool needsWater = true;


        public CompProperties_EatWeirdFood()
        {
            this.compClass = typeof(CompEatWeirdFood);
        }
    }
}
