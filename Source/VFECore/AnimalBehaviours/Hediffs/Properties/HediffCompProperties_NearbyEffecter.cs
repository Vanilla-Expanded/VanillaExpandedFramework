using Verse;
using System.Collections.Generic;
namespace AnimalBehaviours
{
    public class HediffCompProperties_NearbyEffecter : HediffCompProperties
    {

        public List<string> thingsToAffect = null;
        public List<string> thingsToConvertTo = null;

        public int ticksConversionRate = 1000;

        public int radius = 2;

        public bool feedCauser = false;
        public float nutritionGained = 0;

        public bool isForbidden = false;

        public HediffCompProperties_NearbyEffecter()
        {
            this.compClass = typeof(HediffComp_NearbyEffecter);
        }
    }
}

