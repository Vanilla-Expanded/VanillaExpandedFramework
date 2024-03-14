using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_GraphicByTerrain : CompProperties
    {

        //CompGraphicByTerrain changes a creature's graphic depending on the terrain it is positioned at


        public int changeGraphicsInterval = 240;

        //These three lists need to be the same length, as they are accessed by their indexes

        public List<string> terrains = null;
        public List<string> suffix = null;              
        public List<string> hediffToApply = null;

        //This will ignore the terrain and just check if "IsWater"

        public bool waterOverride = false;
        public string waterSuffix = "_Winter";
        public string waterHediffToApply = "";
        public int waterSeasonalItemsIndex = 0;

        //This will ignore the terrain and just check if temperature is less than temperatureThreshold

        public bool lowTemperatureOverride = false;
        public int temperatureThreshold = -10;
        public string lowTemperatureSuffix = "_Winter";
        public string lowTemperatureHediffToApply = "";
        public int lowTemperatureSeasonalItemsIndex = 0;

        //This will ignore the terrain and just check if there is snow on it

        public bool snowOverride = false;
        public string snowySuffix = "_Winter";
        public string snowyHediffToApply = "";
        public int snowySeasonalItemsIndex = 0;

        //This goes in tandem with CompAnimalProduct to allow different animal products depending on terrain

        public bool provideSeasonalItems = false;
        public List<int> seasonalItemsIndexes = null;



        public CompProperties_GraphicByTerrain()
        {
            this.compClass = typeof(CompGraphicByTerrain);
        }


    }
}
