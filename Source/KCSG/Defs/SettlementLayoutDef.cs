using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KCSG
{
    public class StructOption
    {
        public IntRange count = new IntRange(1, 20);
        public string tag;
    }

    public class SettlementLayoutDef : Def
    {
        public IntVec2 settlementSize = new IntVec2(42, 42);

        public List<StructOption> allowedStructures = new List<StructOption>();
        public List<StructureLayoutDef> centerBuilding = new List<StructureLayoutDef>();

        public int spaceAround = 2;
        public bool avoidBridgeable = false;
        public bool avoidMountains = false;

        public bool addMainRoad = false;
        public TerrainDef mainRoadDef = null;

        public bool addRoad = false;
        public TerrainDef roadDef = null;

        public bool addLandingPad = false;
        public bool vanillaLikeDefense = false;

        public PawnGroupKindDef groupKindDef = null;
        public float pawnGroupMultiplier = 1f;

        public float stockpileValueMultiplier = 1f;
    }
}