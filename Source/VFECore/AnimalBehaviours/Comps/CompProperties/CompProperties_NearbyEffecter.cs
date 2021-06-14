using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    class CompProperties_NearbyEffecter : CompProperties
    {

        //This comp class makes the animal convert up to two things into a third one

        public List<string> thingsToAffect = null;
        public List<string> thingsToConvertTo = null;

        public int ticksConversionRate = 1000;

        public int radius = 2;

        public bool feedCauser = false;
        public float nutritionGained = 0;

        public bool isForbidden = false;

        public CompProperties_NearbyEffecter()
        {
            this.compClass = typeof(CompNearbyEffecter);
        }
    }
}