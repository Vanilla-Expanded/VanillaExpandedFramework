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

        public TerrainDef pathThing = null;
        public int pathSize = 2;

        public bool vanillaLikeDefense = false;
    }
}
