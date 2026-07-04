using Verse;
using RimWorld;
using KCSG;
using Verse.AI.Group;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using UnityEngine;

namespace VEF.Storyteller
{
    public static class StructureSetGenerator
    {
        public static List<CellRect> Generate(Map map, StructureSetDef structureSetDef, Faction faction, float points = 0f)
        {
            var generatedRects = new List<CellRect>();
            var mapCenter = map.Center;
            var usedDefs = new HashSet<KCSG.StructureLayoutDef>();
            var standardLayouts = new List<(StructurePatternOffset layout, KCSG.StructureLayoutDef def)>();
            var specialLayouts = new List<StructurePatternOffset>();

            foreach (var layout in structureSetDef.structureLayouts)
            {
                if (layout.pointsRange.HasValue && !layout.pointsRange.Value.Includes(points)) continue;

                if (layout.scatter || layout.radialCount > 0)
                {
                    specialLayouts.Add(layout);
                    continue;
                }

                var availableDefs = DefDatabase<KCSG.StructureLayoutDef>.AllDefsListForReading
                    .Where(def => !usedDefs.Contains(def) && Regex.IsMatch(def.defName, "^" + layout.pattern + "$"))
                    .ToList();

                if (!availableDefs.Any()) continue;
                
                var selectedDef = availableDefs.RandomElement();
                usedDefs.Add(selectedDef);
                standardLayouts.Add((layout, selectedDef));
            }

            var primaryRect = CellRect.CenteredOn(mapCenter, 1, 1);

            if (standardLayouts.Any())
            {
                int minX = int.MaxValue, minZ = int.MaxValue, maxX = int.MinValue, maxZ = int.MinValue;
                foreach (var item in standardLayouts)
                {
                    var rect = CellRect.CenteredOn(new IntVec3(item.layout.offset.x * item.def.Sizes.x, 0, item.layout.offset.z * item.def.Sizes.z), item.def.Sizes);
                    if (rect.minX < minX) minX = rect.minX;
                    if (rect.minZ < minZ) minZ = rect.minZ;
                    if (rect.maxX > maxX) maxX = rect.maxX;
                    if (rect.maxZ > maxZ) maxZ = rect.maxZ;
                }

                var totalOffset = new IntVec3(-(minX + (maxX - minX) / 2), 0, -(minZ + (maxZ - minZ) / 2));
                foreach (var item in standardLayouts)
                {
                    var spawnPos = mapCenter + totalOffset + new IntVec3(item.layout.offset.x * item.def.Sizes.x, 0, item.layout.offset.z * item.def.Sizes.z);
                    Rot4 rot = item.layout.randomRotated ? Rot4.Random : Rot4.North;
                    IntVec2 sizes = item.layout.randomRotated && rot.IsHorizontal ? new IntVec2(item.def.Sizes.z, item.def.Sizes.x) : item.def.Sizes;
                    var structureRect = CellRect.CenteredOn(spawnPos, sizes.x, sizes.z);

                    GenOption.GetAllMineableIn(structureRect, map);
                    LayoutUtils.CleanRect(item.def, map, structureRect, true, rot);
                    var spawnedThings = new List<Thing>();
                    item.def.Generate(structureRect, map, spawnedThings, faction, false, rot);
                    
                    generatedRects.Add(structureRect);
                    SpawnPawnsAndThings(map, structureRect, item.layout, faction);
                }
                primaryRect = generatedRects[0];
            }

            foreach (var layout in specialLayouts)
            {
                int count = layout.count.RandomInRange;
                for (int i = 0; i < count; i++)
                {
                    var availableDefs = DefDatabase<KCSG.StructureLayoutDef>.AllDefsListForReading
                        .Where(def => !usedDefs.Contains(def) && Regex.IsMatch(def.defName, "^" + layout.pattern + "$"))
                        .ToList();
                    var selectedDef = availableDefs.RandomElement();
                    usedDefs.Add(selectedDef);

                    int subSpawnCount = layout.radialCount > 0 ? layout.radialCount : 1;
                    for (int j = 0; j < subSpawnCount; j++)
                    {
                        IntVec3 spawnPos;
                        Rot4? rot = null;
                        if (layout.radialCount > 0)
                        {
                            float angle = (360f / layout.radialCount) * j;
                            float distance = layout.radialDistance + Mathf.Max(primaryRect.Width, primaryRect.Height) / 2f;
                            Vector3 offset = Vector3.forward * distance;
                            offset = Quaternion.Euler(0, angle, 0) * offset;
                            spawnPos = primaryRect.CenterCell + offset.ToIntVec3();

                            if (layout.faceCenter)
                            {
                                rot = Rot4.FromAngleFlat(angle);
                                rot = new Rot4(rot.Value.AsInt + layout.rotationOffset);
                            }
                            if (layout.randomRotated) rot = Rot4.Random;
                        }
                        else
                        {
                            bool found = false;
                            spawnPos = IntVec3.Invalid;
                            for (int k = 0; k < 100; k++)
                            {
                                spawnPos = mapCenter + new IntVec3(Rand.Range(-40, 40), 0, Rand.Range(-40, 40));
                                var checkRect = CellRect.CenteredOn(spawnPos, selectedDef.Sizes.x + 2, selectedDef.Sizes.z + 2);
                                if (!generatedRects.Any(r => r.Overlaps(checkRect)) && checkRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) continue;

                            if (layout.randomRotated) rot = Rot4.Random;
                        }

                        var rotatedSizes = rot.HasValue && rot.Value.IsHorizontal ? new IntVec2(selectedDef.Sizes.z, selectedDef.Sizes.x) : selectedDef.Sizes;
                        var structureRect = CellRect.CenteredOn(spawnPos, rotatedSizes.x, rotatedSizes.z);

                        GenOption.GetAllMineableIn(structureRect, map);
                        LayoutUtils.CleanRect(selectedDef, map, structureRect, true, rot ?? Rot4.North);
                        var spawnedThings = new List<Thing>();
                        selectedDef.Generate(structureRect, map, spawnedThings, faction, false, rot);

                        generatedRects.Add(structureRect);
                        SpawnPawnsAndThings(map, structureRect, layout, faction);
                    }
                }
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