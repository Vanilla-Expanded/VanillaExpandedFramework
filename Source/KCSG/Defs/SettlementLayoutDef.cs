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
        public int MainRoadWidth => mainRoadWidth - 1;
        public int LinkRoadWidth => linkRoadWidth - 1;

        public bool addMainRoad = false;
        public int mainRoadCount = 1;
        public TerrainDef mainRoadDef = null;
        public int mainRoadWidth = 2;

        public bool addLinkRoad = false;
        public TerrainDef linkRoadDef = null;
        public int linkRoadWidth = 1;
    }

    public class StuffableOptions
    {
        public bool randomizeWall = false;
        public bool generalWallStuff = false;
        public List<ThingDef> allowedWallStuff = new List<ThingDef>();
        public List<ThingDef> disallowedWallStuff = new List<ThingDef>();

        public bool randomizeFurniture = false;
        public List<ThingDef> allowedFurnitureStuff = new List<ThingDef>();
        public List<ThingDef> disallowedFurnitureStuff = new List<ThingDef>();
        public List<ThingDef> excludedFunitureDefs = new List<ThingDef>();
    }

    public class PropsOptions
    {
        public bool addRoadProps = false;
        public List<ThingDef> mainRoadPropsDefs = new List<ThingDef>();
        public float mainRoadPropsChance = 0.25f;
        public List<ThingDef> linkRoadPropsDefs = new List<ThingDef>();
        public float linkRoadPropsChance = 0.25f;

        public bool scatterProps = false;
        public List<ThingDef> scatterPropsDefs = new List<ThingDef>();
        public float scatterPropsChance = 0.25f;
    }

    public class SettlementLayoutDef : Def
    {
        public IntVec2 settlementSize = new IntVec2(42, 42);

        public List<StructOption> allowedStructures = new List<StructOption>();
        public List<string> centralBuildingTags = new List<string>();

        public int spaceAround = 1;
        public bool avoidBridgeable = false;
        public bool avoidMountains = false;

        public RoadOptions roadOptions;

        public StuffableOptions stuffableOptions;

        public PropsOptions propsOptions;

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
            if (stuffableOptions == null)
                stuffableOptions = new StuffableOptions();
            if (roadOptions == null)
                roadOptions = new RoadOptions();
        }
    }
}