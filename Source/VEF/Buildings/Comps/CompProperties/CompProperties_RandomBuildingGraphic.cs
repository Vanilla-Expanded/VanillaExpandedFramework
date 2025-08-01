﻿using Verse;
using System.Collections.Generic;


namespace VEF.Buildings
{
    public class CompProperties_RandomBuildingGraphic : CompProperties
    {

        //A simple comp class that changes a building's graphic by using reflection
       
        public List<string> randomGraphics;

        public List<string> optionalNames;

        public bool startAsRandom = true;

        public bool disableRandomButton = false;

        public bool disableGraphicChoosingButton = false;

        public bool disableAllButtons = false;

        public bool useSouthOrientation = false;



        public CompProperties_RandomBuildingGraphic()
        {
            this.compClass = typeof(CompRandomBuildingGraphic);
        }


    }
}
