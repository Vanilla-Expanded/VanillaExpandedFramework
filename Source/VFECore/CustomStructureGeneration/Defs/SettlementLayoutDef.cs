using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class SettlementLayoutDef : Def
    {
        // With default size grid, allow for 3 rooms with 2 tiles path between them
        public IntVec2 settlementSize = new IntVec2(42, 42);
        public IntVec2 gridSize = new IntVec2(12, 12);
        public bool clearEverything = true;
        public bool placeNaturalTerrain = false;

        public bool path = false; // Add path between room
        public TerrainDef pathType = null;
        public int pathWidth = 2; // Space between room

        public bool vanillaLikeDefense = true; // Use vanilla generation for defense
        public bool customDefense = false;
        // public ExternalStruct externalStruct = null;

        public List<string> roomLayout = new List<string>();
    }
}
