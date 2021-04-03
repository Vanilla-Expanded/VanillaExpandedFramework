using Verse;
using System.Collections.Generic;


namespace VanillaFurnitureExpanded
{
    public class CompProperties_RandomBuildingGraphic : CompProperties
    {

        //A simple comp class that changes a building's graphic by using reflection
       
        public List<string> randomGraphics;

       

        public CompProperties_RandomBuildingGraphic()
        {
            this.compClass = typeof(CompRandomBuildingGraphic);
        }


    }
}
