using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFurnitureExpanded
{

    public class LootableBuildingDetails : DefModExtension
    {
        public bool useBaseGameOpen = true;
        public bool randomFromContents = false;
        public int totalRandomLoops = 1;
        public List<ThingAndCount> contents = null;
        public ThingDef buildingLeft = null;
        public SoundDef deconstructSound = null;
        public string gizmoTexture;
        public string gizmoText;
        public string gizmoDesc;
        public string requiredMod = "";
      

    }

    public class ThingAndCount
    {
        public ThingDef thing;
        public int count = 1;

    }


}
