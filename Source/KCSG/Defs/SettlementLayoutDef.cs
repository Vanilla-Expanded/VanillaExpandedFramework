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

        public TerrainDef mainRoadDef = null;
        public TerrainDef roadDef = null;

        public bool addLandingPad = false;
        public bool vanillaLikeDefense = false;

        public PawnGroupKindDef groupKindDef = null;
        public float pawnGroupMultiplier = 1f;

        public float stockpileValueMultiplier = 1f;
    }
}