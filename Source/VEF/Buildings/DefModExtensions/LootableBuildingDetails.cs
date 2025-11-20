using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VEF.Buildings
{

    public class LootableBuildingDetails : DefModExtension
    {

        public bool randomFromContents = false;
        public IntRange totalRandomLoops = new IntRange(1, 1);
        public List<ThingAndCount> contents = null;
        public ThingDef buildingLeft = null;
        public SoundDef deconstructSound = null;
        public string gizmoTexture;
        public string gizmoText;
        public string gizmoDesc;
        public string cancelLootingGizmoTexture;
        public string cancelLootinggizmoText;
        public string cancelLootinggizmoDesc;
        public string requiredMod = "";
        public string overlayTexture;
        // This only affects LootableBuilding_Custom. IOpenable has its own systems
        public int secondsToOpen = 20;
        public bool useHackingSpeed = false;

        // Optionally, LootableBuilding_Custom can also use ThingDetMakerDefs
        public bool useThingSetMakerDef = false;
        public ThingSetMakerDetails setMakerDetails = null;


    }

    public class ThingAndCount
    {
        public ThingDef thing;
        public int count = 1;
        public IntRange randomCount = new IntRange(1, 1);

    }

    public class ThingSetMakerDetails
    {
        public ThingSetMakerDef thingSetMakerDef;
        public FloatRange totalMarketValueRange = new FloatRange(850, 1000);
        public float? minSingleItemMarketValuePct;
        public bool allowNonStackableDuplicates = true;
        public IntRange countRange = new IntRange(1, 1);

    }


}
