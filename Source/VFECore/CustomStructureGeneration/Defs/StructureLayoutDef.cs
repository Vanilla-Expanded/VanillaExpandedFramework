using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class StructureLayoutDef : Def
    {
        public List<List<string>> layouts = new List<List<string>>();
        public bool roofOver = true;
        public bool isStockpile = false;
        public List<ThingDef> allowedThingsInStockpile = new List<ThingDef>();

        public List<string> roofGrid = new List<string>();
        public List<string> terrainGrid = new List<string>();
    }
}
