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

        public bool requireRoyalty = false;

        public bool isStorage = false;

        // Settings for SettlementDef

        public List<string> tags = new List<string>();
    }
}
