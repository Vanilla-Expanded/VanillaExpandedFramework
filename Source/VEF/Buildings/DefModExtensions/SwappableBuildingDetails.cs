
﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Buildings
{
    public class SwappableBuildingDetails : DefModExtension
    {

        public ThingDef buildingLeft = null;
        public SoundDef deconstructSound = null;
        public int swappingTimer = -1;

    }
}