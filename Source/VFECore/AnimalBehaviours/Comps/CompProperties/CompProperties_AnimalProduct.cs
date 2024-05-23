using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_AnimalProduct : CompProperties
    {

        public bool hideDisplayOnWildAnimals = false;

        //CompAnimalProduct builds upon both CompMilkable and CompShearable, with many more configuration options

        public int gatheringIntervalDays = 1;
        public int resourceAmount = 1;
        public ThingDef resourceDef = null;

        //customResourceString allows you to set a different string on the info panel

        public string customResourceString = "";

        //CompProperties_AnimalProduct allows an animal to produce random items

        public bool isRandom = false;
        public List<string> randomItems = null;

        //seasonalItems is only used by the Chameleon Yak
        public List<string> seasonalItems = null;

        //CompProperties_AnimalProduct allows an animal to produce the normal item, and a few additional items, chosen from a list

        public bool hasAditional = false;
        public int additionalItemsProb = 1;
        public int additionalItemsNumber = 1;
        public List<string> additionalItems = null;
        public bool goInOrder = false;

        public CompProperties_AnimalProduct()
        {
            this.compClass = typeof(CompAnimalProduct);
        }


    }
}
