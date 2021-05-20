using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class SettlementLayoutDef : Def
    {
        public IntVec2 settlementSize = new IntVec2(42, 42);

        public List<string> atLeastOneOfTags = new List<string>();
        public List<string> allowedTags = new List<string>();

        public TerrainDef roadDef = null;
        public TerrainDef mainRoadDef = null;

        public bool vanillaLikeDefense = false;
    }
}
