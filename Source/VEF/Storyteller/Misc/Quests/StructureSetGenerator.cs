using Verse;
using RimWorld;
using KCSG;
using Verse.AI.Group;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace VEF.Storyteller
{
    public static class StructureSetGenerator
    {
        public static List<CellRect> Generate(Map map, StructureSetDef structureSetDef, Faction faction, float points = 0f)
        {
            var generatedRects = new List<CellRect>();
            var mapCenter = map.Center;
            var precalculatedLayouts = new List<(StructurePatternOffset layout, KCSG.StructureLayoutDef def)>();
            var usedDefs = new HashSet<KCSG.StructureLayoutDef>();

            foreach (var layout in structureSetDef.structureLayouts)
            {
                if (layout.pointsRange.HasValue && !layout.pointsRange.Value.Includes(points)) continue;

                var availableDefs = DefDatabase<KCSG.StructureLayoutDef>.AllDefsListForReading
                    .Where(def => !usedDefs.Contains(def) && Regex.IsMatch(def.defName, "^" + layout.pattern + "$"))
                    .ToList();

                if (!availableDefs.Any()) continue;
                
                var selectedDef = availableDefs.RandomElement();
                usedDefs.Add(selectedDef);
                precalculatedLayouts.Add((layout, selectedDef));
            }

            if (!precalculatedLayouts.Any()) return generatedRects;

            int minX = int.MaxValue, minZ = int.MaxValue, maxX = int.MinValue, maxZ = int.MinValue;

            foreach (var item in precalculatedLayouts)
            {
                var rect = CellRect.CenteredOn(new IntVec3(item.layout.offset.x * item.def.Sizes.x, 0, item.layout.offset.z * item.def.Sizes.z), item.def.Sizes);
                if (rect.minX < minX) minX = rect.minX;
                if (rect.minZ < minZ) minZ = rect.minZ;
                if (rect.maxX > maxX) maxX = rect.maxX;
                if (rect.maxZ > maxZ) maxZ = rect.maxZ;
            }

            var totalOffset = new IntVec3(-(minX + (maxX - minX) / 2), 0, -(minZ + (maxZ - minZ) / 2));

            foreach (var item in precalculatedLayouts)
            {
                var spawnPos = mapCenter + totalOffset + new IntVec3(item.layout.offset.x * item.def.Sizes.x, 0, item.layout.offset.z * item.def.Sizes.z);
                var structureRect = CellRect.CenteredOn(spawnPos, item.def.Sizes);
                
                GenOption.GetAllMineableIn(structureRect, map);
                var spawnedThings = new List<Thing>();
                item.def.Generate(structureRect, map, spawnedThings, map.ParentFaction);
                
                generatedRects.Add(structureRect);
                if (item.layout.spawnPawns != null || item.layout.spawnThings != null) SpawnPawnsAndThings(map, structureRect, item.layout, faction);
            }
            
            var usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
            usedRects.AddRange(generatedRects);
            return generatedRects;
        }

        private static void SpawnPawnsAndThings(Map map, CellRect structureRect, StructurePatternOffset layout, Faction faction)
        {
            var walkableCells = structureRect.Cells.Where(cell => cell.Walkable(map) && (!layout.forceSpawnEnemiesIndoor || (cell.Roofed(map) && !cell.UsesOutdoorTemperature(map)))).ToList();
            if (!walkableCells.Any()) return;

            var pawns = new List<Pawn>();
            if (layout.spawnPawns != null)
            {
                foreach (var spawnOption in layout.spawnPawns)
                {
                    for (var i = 0; i < spawnOption.count.RandomInRange; i++)
                    {
                        var rootCell = walkableCells.RandomElement();
                        if (!rootCell.IsValid) rootCell = structureRect.CenterCell;
                        var spawnCell = CellFinder.RandomSpawnCellForPawnNear(rootCell, map, 5);
                        if (!spawnCell.IsValid) continue;
                        
                        var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(spawnOption.kind, faction, PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true));
                        if (pawn.RaceProps.Humanlike && layout.weapons.NullOrEmpty() is false)
                        {
                            pawn.equipment.DestroyAllEquipment();
                            pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(layout.weapons.RandomElement()));
                        }
                        if (layout.unwaveringlyLoyal && pawn.guest != null) pawn.guest.Recruitable = false;
                        GenSpawn.Spawn(pawn, spawnCell, map);
                        pawns.Add(pawn);
                    }
                }
            }
            if (layout.spawnThings != null)
            {
                foreach (var spawnOption in layout.spawnThings)
                {
                    int remaining = spawnOption.count.RandomInRange;
                    while (remaining > 0)
                    {
                        int stackCount = Math.Min(remaining, spawnOption.thing.stackLimit);
                        var spawnCell = CellFinder.RandomSpawnCellForPawnNear(walkableCells.RandomElement(), map, 5);
                        if (!spawnCell.IsValid) break;
                        var thing = ThingMaker.MakeThing(spawnOption.thing);
                        thing.stackCount = stackCount;
                        GenSpawn.Spawn(thing, spawnCell, map);
                        if (thing is Hive hive) hive.SetFaction(Faction.OfInsects);
                        else if (thing.def.CanHaveFaction) thing.SetFaction(faction);
                        thing.SetForbidden(true, false);
                        remaining -= stackCount;
                    }
                }
            }
            if (pawns.Any()) LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(walkableCells.RandomElement()), map, pawns);
        }
    }
}