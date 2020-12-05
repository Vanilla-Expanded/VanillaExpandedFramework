using Verse;


namespace AnimalBehaviours
{
    class CompProperties_NearbyEffecter : CompProperties
    {

        //This comp class makes the animal convert up to two things into a third one

        public string thingToAffect = "";
        public string secondaryThingToAffect = "";

        public string thingToTurnTo = "";
        public int ticksConversionRate = 1000;

        public int radius = 2;

        public CompProperties_NearbyEffecter()
        {
            this.compClass = typeof(CompNearbyEffecter);
        }
    }
}