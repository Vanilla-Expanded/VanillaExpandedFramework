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

    public class RoadOptions
    {
        public bool addMainRoad = false;
        public TerrainDef mainRoadDef = null;

        public bool addLinkRoad = false;
        public TerrainDef linkRoadDef = null;
    }

    public class SettlementLayoutDef : Def
    {
        public IntVec2 settlementSize = new IntVec2(42, 42);

        public List<StructOption> allowedStructures = new List<StructOption>();
        public List<StructureLayoutDef> centerBuilding = new List<StructureLayoutDef>();

        public int spaceAround = 1;
        public bool avoidBridgeable = false;
        public bool avoidMountains = false;

        public RoadOptions roadOptions;

        public bool addLandingPad = false;
        public bool vanillaLikeDefense = false;
        public bool vanillaLikeDefenseNoSandBags = false;

        public PawnGroupKindDef groupKindDef = null;
        public float pawnGroupMultiplier = 1f;

        public float stockpileValueMultiplier = 1f;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (roadOptions == null)
                roadOptions = new RoadOptions();
        }
    }
}