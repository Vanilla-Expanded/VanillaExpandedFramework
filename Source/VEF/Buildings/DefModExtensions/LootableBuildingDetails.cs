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


    }

    public class ThingAndCount
    {
        public ThingDef thing;
        public int count = 1;

    }


}
