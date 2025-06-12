using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class GenStep_CustomStructureGen : GenStep
    {
        public bool fullClear = false;
        public bool clearFogInRect = false;
        public bool preventBridgeable = false;
        public bool spawnInRandomFreeLocation = false;

        public List<StructureLayoutDef> structureLayoutDefs = new List<StructureLayoutDef>();
        public List<SettlementLayoutDef> settlementLayoutDefs = new List<SettlementLayoutDef>();
        public List<TiledStructureDef> tiledStructures = new List<TiledStructureDef>();

        public List<string> symbolResolvers = new List<string>();

        public List<ThingDef> scatterThings = new List<ThingDef>();
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public float scatterChance = 0.4f;

        public bool scaleWithQuest = false;

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
                TileUtils.Generate(tiledStructures.RandomElement(), map.Center, map, scaleWithQuest ? CustomGenOption.GetRelatedQuest(map) : null);
                return;
            }
            CellRect cellRect = default;
            if (structureLayoutDefs.Any())
            {
                // Normal
                StructureLayoutDef layoutDef = structureLayoutDefs.RandomElement();
                if (spawnInRandomFreeLocation)
                {
                    List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");

                    if ((!MapGenUtility.TryGetRandomClearRect(layoutDef.sizes.x, layoutDef.sizes.z, out CellRect rect, -1, -1, (CellRect r) => !r.Cells.Any((IntVec3 c) => c.GetTerrain(map).IsWater) && !usedRects.Any((CellRect ur) => ur.Overlaps(r))) || !rect.CenterCell.InHorDistOf(map.Center, 75f)) && !MapGenUtility.TryGetClosestClearRectTo(out rect, new IntVec2(layoutDef.sizes.x, layoutDef.sizes.z), map.Center, (CellRect r) => !r.Cells.Any((IntVec3 c) => c.GetTerrain(map).IsWater) && !usedRects.Any((CellRect ur) => ur.Overlaps(r))))
                    {
                        Log.Error("Failed to find location for KCSG structure");
                        return;
                    }
                    else
                    {
                        cellRect = rect;
                    }

                }
                else { cellRect = CellRect.CenteredOn(map.Center, layoutDef.sizes.x, layoutDef.sizes.z); }



                GenOption.GetAllMineableIn(cellRect, map);
                LayoutUtils.CleanRect(layoutDef, map, cellRect, fullClear);
                layoutDef.Generate(cellRect, map);
            }
            else if (settlementLayoutDefs.Any())
            {
                var layoutDef = settlementLayoutDefs.RandomElement();
                GenOption.settlementLayout = layoutDef;
                if (spawnInRandomFreeLocation)
                {
                    List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");

                    if ((!MapGenUtility.TryGetRandomClearRect(layoutDef.settlementSize.x, layoutDef.settlementSize.z, out CellRect rect, -1, -1, (CellRect r) => !r.Cells.Any((IntVec3 c) => c.GetTerrain(map).IsWater) && !usedRects.Any((CellRect ur) => ur.Overlaps(r))) || !rect.CenterCell.InHorDistOf(map.Center, 75f)) && !MapGenUtility.TryGetClosestClearRectTo(out rect, new IntVec2(layoutDef.settlementSize.x, layoutDef.settlementSize.z), map.Center, (CellRect r) => !r.Cells.Any((IntVec3 c) => c.GetTerrain(map).IsWater) && !usedRects.Any((CellRect ur) => ur.Overlaps(r))))
                    {
                        Log.Error("Failed to find location for KCSG settlement");
                        return;
                    }
                    else
                    {
                        cellRect = rect;
                    }

                }
                else
                {
                    cellRect = CellRect.CenteredOn(map.Center, layoutDef.settlementSize.x, layoutDef.settlementSize.z);
                }
                GenOption.GetAllMineableIn(cellRect, map);
                BaseGen.globalSettings.map = map;
                var rp = new ResolveParams
                {
                    faction = map.ParentFaction,
                    rect = cellRect
                };
                SettlementGenUtils.Generate(rp, map, GenOption.settlementLayout);
            }

            if (GenOption.customGenExt.symbolResolvers?.Count > 0)
            {
                Debug.Message("GenStep_CustomStructureGen - Additional symbol resolvers");
                BaseGen.symbolStack.Push("kcsg_runresolvers", new ResolveParams
                {
                    faction = map.ParentFaction,
                    rect = cellRect
                }, null);
            }


            PostGenerate(cellRect, map, parms);

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

        public virtual void PostGenerate(CellRect rect, Map map, GenStepParams parms)
        {

        }
    }
}