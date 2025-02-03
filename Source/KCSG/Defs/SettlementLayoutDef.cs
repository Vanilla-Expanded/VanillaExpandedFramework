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

        public bool forceClean = false;
    }

    public class DefenseOptions
    {
        public bool addEdgeDefense = false;

        public bool addSandbags = false;

        public bool addTurrets = false;
        public int cellsPerTurret = 30;
        public List<ThingDef> allowedTurretsDefs = new List<ThingDef>();

        public bool addMortars = false;
        public int cellsPerMortar = 75;
        public List<ThingDef> allowedMortarsDefs = new List<ThingDef>();

        public PawnGroupKindDef groupKindDef = null;
        public float pawnGroupMultiplier = 1f;

        public void ResolveReference()
        {
            if (allowedTurretsDefs.Count == 0)
                allowedTurretsDefs.Add(ThingDefOf.Turret_AutoMiniTurret);
            if (allowedMortarsDefs.Count == 0)
                allowedMortarsDefs.Add(ThingDefOf.Turret_Mortar);
            if (groupKindDef == null)
                groupKindDef = PawnGroupKindDefOf.Settlement;
        }
    }

    public class StockpileOptions
    {
        public float RefMarketValue { get; set; }

        public float stockpileValueMultiplier = 1f;

        public bool fillStorageBuildings = false;
        public float fillChance = 0.6f;
        public float maxValueStackIncrease = 40f;
        public bool replaceOtherThings = false;
        public List<ThingDef> fillWithDefs = new List<ThingDef>();

        public void ResolveReference()
        {
            RefMarketValue = 0;
            if (fillStorageBuildings && !fillWithDefs.NullOrEmpty())
            {
                for (int i = 0; i < fillWithDefs.Count; i++)
                {
                    var defMarketValue = fillWithDefs[i].BaseMarketValue;
                    if (defMarketValue > RefMarketValue)
                        RefMarketValue = defMarketValue;
                }

                RefMarketValue += 1f;
            }
        }
    }

    public class SettlementLayoutDef : Def
    {
        public IntVec2 settlementSize = new IntVec2(42, 42);

        public int samplingDistance = 8;
        public bool avoidBridgeable = false;
        public bool avoidMountains = false;

        public CenterBuildings centerBuildings;

        public PeripheralBuildings peripheralBuildings = new PeripheralBuildings();

        public RoadOptions roadOptions = new RoadOptions();

        public StuffableOptions stuffableOptions = new StuffableOptions();

        public PropsOptions propsOptions = new PropsOptions();

        public DefenseOptions defenseOptions = new DefenseOptions();

        public StockpileOptions stockpileOptions = new StockpileOptions();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            defenseOptions.ResolveReference();
            stockpileOptions.ResolveReference();
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var err in base.ConfigErrors())
                yield return err;

            if (centerBuildings == null)
                yield return "Cannot use SettlementLayoutDef with null centerBuildings";

            if (stockpileOptions.fillStorageBuildings && stockpileOptions.fillWithDefs.NullOrEmpty())
                yield return "Cannot use fillShelf with empty or null fillShelfDefs";
        }
    }
}