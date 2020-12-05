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
        public List<string> roofGrid = new List<string>();

        public bool isStockpile = false;
        public List<ThingDef> allowedThingsInStockpile = new List<ThingDef>();

        [Obsolete("Not used anymore, only here for compatibility with mod that used the old CSG")]
        public bool roofOver = true;
        [Obsolete("Not used anymore, only here for compatibility with mod that used the old CSG")]
        public List<string> terrainGrid = new List<string>();
    }
}
