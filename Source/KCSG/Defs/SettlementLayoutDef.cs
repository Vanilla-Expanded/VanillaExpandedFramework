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
        public bool mainRoadLinkToEdges = false;
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
        public bool addMainRoadProps = false;
        public List<ThingDef> mainRoadPropsDefs = new List<ThingDef>();
        public float mainRoadPropsChance = 0.25f;
        public int mainRoadMinDistance = 10;

        public bool addLinkRoadProps = false;
        public List<ThingDef> linkRoadPropsDefs = new List<ThingDef>();
        public float linkRoadPropsChance = 0.25f;
        public int linkRoadMinDistance = 10;

        public bool scatterProps = false;
        public int scatterMaxAmount = 100;
        public int scatterMinDistance = 10;
        public List<ThingDef> scatterPropsDefs = new List<ThingDef>();

        internal ThingDef RandomMainRoadProps()
        {
            if (mainRoadPropsDefs.NullOrEmpty())
                return null;

            return mainRoadPropsDefs.RandomElement();
        }

        internal ThingDef RandomLinkRoadProps()
        {
            if (linkRoadPropsDefs.NullOrEmpty())
                return null;

            return linkRoadPropsDefs.RandomElement();
        }
    }

    public class PeripheralBuildings
    {
        public int spaceAround = 1;

        public List<StructOption> allowedStructures = new List<StructOption>();
    }

    public class CenterBuildings
    {
        public IntVec2 centerSize = new IntVec2(42, 42);

        public int spaceAround = 1;

        public List<StructOption> allowedStructures = new List<StructOption>();
        public List<string> centralBuildingTags = new List<string>();
    }

    public class SettlementLayoutDef : Def
    {
        public IntVec2 settlementSize = new IntVec2(42, 42);

        public bool avoidBridgeable = false;
        public bool avoidMountains = false;

        public PeripheralBuildings peripheralBuildings;

        public CenterBuildings centerBuildings;

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
            if (propsOptions == null)
                propsOptions = new PropsOptions();
            if (peripheralBuildings == null)
                peripheralBuildings = new PeripheralBuildings();
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var err in base.ConfigErrors())
                yield return err;

            if (centerBuildings == null)
                yield return "Cannot use SettlementLayoutDef with null centerBuildings";
        }
    }
}