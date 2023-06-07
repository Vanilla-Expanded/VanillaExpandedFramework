using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class GenStep_CustomStructureGen : GenStep
    {
        public bool fullClear = false;
        public bool clearFogInRect = false;
        public bool preventBridgeable = false;

        public List<StructureLayoutDef> structureLayoutDefs = new List<StructureLayoutDef>();
        public List<TiledStructureDef> tiledStructures = new List<TiledStructureDef>();

        public List<string> symbolResolvers = new List<string>();

        public List<ThingDef> scatterThings = new List<ThingDef>();
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public float scatterChance = 0.4f;

        public override int SeedPart => 916595355;

        public override void Generate(Map map, GenStepParams parms)
        {
            GenOption.customGenExt = new CustomGenOption
            {
                symbolResolvers = symbolResolvers,
                filthTypes = filthTypes,
                scatterThings = scatterThings,
                scatterChance = scatterChance,
            };

            // Tiled
            if (!tiledStructures.NullOrEmpty())
            {
                var quest = Find.QuestManager.QuestsListForReading.Find(q => q.QuestSelectTargets.Any(t => t.Map == map));
                TileUtils.Generate(tiledStructures.RandomElement(), map.Center, map, quest);
                return;
            }
            // Normal
            StructureLayoutDef layoutDef = structureLayoutDefs.RandomElement();

            var cellRect = CellRect.CenteredOn(map.Center, layoutDef.sizes.x, layoutDef.sizes.z);
            GenOption.GetAllMineableIn(cellRect, map);
            LayoutUtils.CleanRect(layoutDef, map, cellRect, fullClear);
            layoutDef.Generate(cellRect, map);

            if (GenOption.customGenExt.symbolResolvers?.Count > 0)
            {
                Debug.Message("GenStep_CustomStructureGen - Additional symbol resolvers");
                BaseGen.symbolStack.Push("kcsg_runresolvers", new ResolveParams
                {
                    faction = map.ParentFaction,
                    rect = cellRect
                }, null);
            }

            // Flood refog
            if (map.mapPawns.FreeColonistsSpawned.Count > 0)
            {
                FloodFillerFog.DebugRefogMap(map);
            }
            // Clear fog in rect if wanted
            if (clearFogInRect)
            {
                foreach (var c in cellRect)
                {
                    if (map.fogGrid.IsFogged(c))
                        map.fogGrid.Unfog(c);
                    else
                        MapGenerator.rootsToUnfog.Add(c);
                }
            }
        }
    }
}