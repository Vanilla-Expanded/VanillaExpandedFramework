using Verse;
using System.Collections.Generic;
using RimWorld;

namespace VEF.Plants
{
  
    public class BloomingPlantExtension : DefModExtension
    {
        public int AgeBeautyModifier = 0;
        public int MaxAgeBeautyModifier = 0;
        public float BloomBeautyModifier = 0;
        public int LeaflessBeauty = 0;

        public Season BloomSeasonStart;
        public int BloomDayStart = 1;

        public Season BloomSeasonStop;
        public int BloomDayEnd = 1;

        public bool CanBloomAgain=true;

        public int BloomTemperatureMin = 0;

        public string bloomGraphicPath;

        public int DeadlyColdTemperature = 0;

    }

}
