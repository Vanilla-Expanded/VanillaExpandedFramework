using Verse;
using System.Collections.Generic;
using RimWorld;

namespace VEF.Plants
{
  
    public class BloomingPlantExtension : DefModExtension
    {
        //Age, bloom and leafless beauty modifiers
        public int AgeBeautyModifier = 0;
        public int MaxAgeBeautyModifier = 0;
        public float BloomBeautyModifier = 0;
        public int LeaflessBeauty = 0;
        public int WeededBeauty = -4;

        //Bloom time variables
        public Season BloomSeasonStart;
        public int BloomDayStart = 1;

        public Season BloomSeasonStop;
        public int BloomDayEnd = 1;

        public bool CanBloomAgain=true;

        //Temperature variables
        public int BloomTemperatureMin = -250;
        public int BloomTemperatureMax = 999;
        public int DeadlyColdTemperature = 0;
        public int DamageWhenBelowDeadlyTemp = 30;

        //Light variables
        public float BloomLightMax = 1;
       
        //Bloom graphics
        public string bloomGraphicPath;
        public string alternateBloomGraphicPath="";

        //Used by Weeds incident
        public bool ImmuneToWeeds = false;

        //Behaviours when blooming
        public ThingDef itemProducedWhenBlooming = null;
        public int longTicksPerItemProduced = 1;
        public int itemProducedAmount = 1;

        public ThingDef filthProducedWhenBlooming = null;
        public int longTicksPerFilthProduced = 1;
        public IntRange filthProducedAmount = IntRange.One;
        public float filthProducedRadius = 1;

        public HediffDef hediffWhenBlooming = null;
        public float hediffRadius = 1;
        public float hediffSeverity = 1;
        public bool hediffOnlyAffectsColonists = true;

    }

}
