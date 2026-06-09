using Verse;
using System.Collections.Generic;
using RimWorld;

namespace VEF.Plants
{
  
    public class BloomingPlantExtension : DefModExtension
    {
        public int AgeBeautyModifier;
        public int MaxAgeBeautyModifier;
        public float BloomBeautyModifier;
        public int LeaflessBeauty;

        public Season BloomSeasonStart;
        public int BloomDayStart;

        public Season BloomSeasonStop;
        public int BloomDayEnd;

        public bool CanBloomAgain;

        public int BloomTemperatureMin;

        public string bloomGraphicPath;

        public int DeadlyColdTemperature;

    }

}
