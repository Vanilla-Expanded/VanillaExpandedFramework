using System.Collections.Generic;
using RimWorld.BaseGen;
using RimWorld;
using Verse;
using System.Linq;

namespace KCSG
{
    public class CustomGenOption : DefModExtension
    {
        public bool UsingTiledStructure => tiledStructures.Count > 0;
        public bool UsingSingleLayout => chooseFromlayouts.Count > 0;

        /* Nomadic faction */
        public bool canSpawnSettlements = true;

        /* Structure generation */
        public List<StructureLayoutDef> chooseFromlayouts = new List<StructureLayoutDef>();
        public List<SettlementLayoutDef> chooseFromSettlements = new List<SettlementLayoutDef>();
        public List<TiledStructureDef> tiledStructures = new List<TiledStructureDef>();

        public string symbolResolver = null;

        public bool tryFindFreeArea = false;
        public bool preGenClear = true;
        public bool fullClear = false;
        public bool preventBridgeable = false;
        public bool clearFogInRect = false;

        public List<string> symbolResolvers = null;
        public List<ThingDef> scatterThings = new List<ThingDef>();
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public float scatterChance = 0.4f;

        public bool scaleWithQuest = false;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
                yield return error;

            for (int i = 0; i < filthTypes.Count; i++)
            {
                if (filthTypes[i].category != ThingCategory.Filth)
                    yield return $"{filthTypes[i].defName} in filthTypes, but isn't in category Filth.";
            }
        }

        public void Generate(IntVec3 loc, Map map)
        {
            GenOption.customGenExt = this;
            // Tiled
            if (UsingTiledStructure)
            {
                TileUtils.Generate(tiledStructures.RandomElement(), loc, map, scaleWithQuest ? GetRelatedQuest(map) : null);
                return;
            }
            // Single/Settlement
            bool single = UsingSingleLayout;
            if (single)
            {
                GenOption.structureLayout = RandomUtils.RandomLayoutFrom(chooseFromlayouts);
            }
            else
            {
                GenOption.settlementLayout = chooseFromSettlements.RandomElement();
            }

            // Get faction
            Faction faction = map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer ? Find.FactionManager.RandomEnemyFaction() : map.ParentFaction;

            // Get settlement size
            int width = single ? GenOption.structureLayout.sizes.x : GenOption.settlementLayout.settlementSize.x;
            int height = single ? GenOption.structureLayout.sizes.z : GenOption.settlementLayout.settlementSize.z;

            // Get spawn position
            IntVec3 spawn = loc;
            if (tryFindFreeArea)
            {
                if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(i => RectFreeValidator(CellRect.CenteredOn(i, width, height), map), map, out spawn))
                    Log.Warning($"[KCSG] Trying to find free spawn area failed");
            }

            // Create rect
            CellRect rect = CellRect.CenteredOn(spawn, width, height);
            rect.ClipInsideMap(map);

            GenOption.GetAllMineableIn(rect, map);
            // Pre-gen clean
            if (preGenClear)
                LayoutUtils.CleanRect(GenOption.structureLayout, map, rect, fullClear);

            // Push symbolresolver
            ResolveParams rp = default;
            rp.faction = faction;
            rp.rect = rect;
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push(symbolResolver ?? "kcsg_settlement", rp, null);
            BaseGen.Generate();
        }

        private bool RectFreeValidator(CellRect rect, Map map)
        {
            foreach (var cell in rect)
            {
                if (!cell.Walkable(map) || cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                    return false;
            }
            return true;
        }

        public static Quest GetRelatedQuest(Map map)
        {
            var quests = Find.QuestManager.QuestsListForReading;
            for (int j = 0; j < quests.Count; j++)
            {
                var quest = quests[j];
                if (!quest.hidden && !quest.Historical && !quest.dismissed && quest.QuestLookTargets.Contains(map.Parent))
                {
                    Debug.Message($"Found {quest} related to {map}");
                    return quest;
                }
            }
            Debug.Message($"Didn't find any quest related to {map}");
            return null;
        }
    }
}