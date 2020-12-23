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

        public bool clearEverything = true;

        public bool vanillaLikeDefense = false;

        // Path settings
        public bool path = false;
        public TerrainDef pathType = null;
        public int pathWidth = 2;


        public bool requireRoyalty = false;
    }
}
