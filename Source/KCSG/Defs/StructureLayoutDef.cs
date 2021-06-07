using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class StructureLayoutDef : Def
    {
        public bool isStorage = false;
        public List<List<string>> layouts = new List<List<string>>();
        public bool requireRoyalty = false;
        public List<string> roofGrid = new List<string>();
        // Settings for SettlementDef
        public List<string> tags = new List<string>();
    }
}
