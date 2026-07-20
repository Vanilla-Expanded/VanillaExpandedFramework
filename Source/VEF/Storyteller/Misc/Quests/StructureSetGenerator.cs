using RimWorld;
using KCSG;
using Verse;
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
            var usedDefs = new HashSet<Def>();
            var standardLayouts = new List<(StructurePatternOffset layout, Def def)>();
            var specialLayouts = new List<StructurePatternOffset>();

            foreach (var layout in structureSetDef.structureLayouts)
            {
                if (layout.pointsRange.HasValue && !layout.pointsRange.Value.Includes(points)) continue;

                if (layout.scatter || layout.radialCount > 0)
                {
                    specialLayouts.Add(layout);
                    continue;
                }

                var availableDefs = GetAvailableDefs(layout.pattern, usedDefs);

                if (!availableDefs.Any()) continue;
                
                var selectedDef = availableDefs.RandomElement();
                usedDefs.Add(selectedDef);
                standardLayouts.Add((layout, selectedDef));
            }

            var primaryRect = CellRect.CenteredOn(mapCenter, 1, 1);

            if (standardLayouts.Any())
            {
                var totalOffset = GetTotalOffset(standardLayouts);
                var first = standardLayouts[0];
                var firstSize = GetLayoutSize(first.def);
                var spawnPos = mapCenter + totalOffset + new IntVec3(first.layout.offset.x * firstSize.x, 0, first.layout.offset.z * firstSize.z);
                primaryRect = CellRect.CenteredOn(spawnPos, firstSize.x, firstSize.z);
            }

            foreach (var layout in specialLayouts)
            {
                int count = layout.count.RandomInRange;
                for (int i = 0; i < count; i++)
                {
                    int subSpawnCount = layout.radialCount > 0 ? layout.radialCount : 1;
                    for (int j = 0; j < subSpawnCount; j++)
                    {
                        var availableDefs = GetAvailableDefs(layout.pattern, usedDefs);
                        if (!availableDefs.Any())
                        {
                            availableDefs = GetAvailableDefs(layout.pattern, null);
                        }
                        if (!availableDefs.Any())
                            continue;
                        var selectedDef = availableDefs.RandomElement();
                        var defSize = GetLayoutSize(selectedDef);
                        usedDefs.Add(selectedDef);

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
                                if (layout.putAnywhere)
                                {
                                    spawnPos = new IntVec3(Rand.Range(0, map.Size.x), 0, Rand.Range(0, map.Size.z));
                                }
                                else if (k < 20)
                                {
                                    spawnPos = mapCenter + new IntVec3(Rand.Range(-40, 40), 0, Rand.Range(-40, 40));
                                }
                                else if (k < 50)
                                {
                                    spawnPos = mapCenter + new IntVec3(Rand.Range(-80, 80), 0, Rand.Range(-80, 80));
                                }
                                else
                                {
                                    spawnPos = new IntVec3(Rand.Range(0, map.Size.x), 0, Rand.Range(0, map.Size.z));
                                }
                                var checkRect = CellRect.CenteredOn(spawnPos, defSize.x + 2, defSize.z + 2);
                                if (checkRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)) && !generatedRects.Any(r => r.Overlaps(checkRect)))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) continue;

                            if (layout.randomRotated) rot = Rot4.Random;
                        }
                        var rotatedSizes = rot.HasValue && rot.Value.IsHorizontal ? new IntVec2(defSize.z, defSize.x) : defSize;
                        var structureRect = CellRect.CenteredOn(spawnPos, rotatedSizes.x, rotatedSizes.z);
                        var spawnedThings = new List<Thing>();
                        GenerateLayout(selectedDef, map, structureRect, faction, rot ?? Rot4.North, spawnedThings);

                        generatedRects.Add(structureRect);
                        SpawnPawnsAndThings(map, structureRect, layout, faction);
                    }
                }
            }
            if (standardLayouts.Any())
            {
                var totalOffset = GetTotalOffset(standardLayouts);
                var centerAssigned = false;
                foreach (var item in standardLayouts)
                {
                    var size = GetLayoutSize(item.def);
                    var spawnPos = mapCenter + totalOffset + new IntVec3(item.layout.offset.x * size.x, 0, item.layout.offset.z * size.z);
                    Rot4 rot = item.layout.randomRotated ? Rot4.Random : Rot4.North;
                    IntVec2 sizes = item.layout.randomRotated && rot.IsHorizontal ? new IntVec2(size.z, size.x) : size;
                    var structureRect = CellRect.CenteredOn(spawnPos, sizes.x, sizes.z);
                    var spawnedThings = new List<Thing>();
                    GenerateLayout(item.def, map, structureRect, faction, rot, spawnedThings);

                    generatedRects.Add(structureRect);
                    SpawnPawnsAndThings(map, structureRect, item.layout, faction);

                    if (!centerAssigned)
                    {
                        primaryRect = structureRect;
                        centerAssigned = true;
                    }
                }
            }
            
            var usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
            usedRects.AddRange(generatedRects);
            return generatedRects;
        }

        private static List<Def> GetAvailableDefs(string pattern, HashSet<Def> usedDefs)
        {
            var list = new List<Def>();
            var regex = new Regex("^" + pattern + "$");
            foreach (var def in DefDatabase<KCSG.StructureLayoutDef>.AllDefsListForReading)
            {
                if ((usedDefs == null || !usedDefs.Contains(def)) && regex.IsMatch(def.defName))
                    list.Add(def);
            }
            foreach (var def in DefDatabase<PrefabDef>.AllDefsListForReading)
            {
                if ((usedDefs == null || !usedDefs.Contains(def)) && regex.IsMatch(def.defName))
                    list.Add(def);
            }
            return list;
        }

        private static IntVec2 GetLayoutSize(Def def)
        {
            if (def is KCSG.StructureLayoutDef kcsgDef) return kcsgDef.Sizes;
            if (def is PrefabDef prefabDef) return prefabDef.size;
            return IntVec2.One;
        }

        private static void GenerateLayout(Def def, Map map, CellRect structureRect, Faction faction, Rot4 rot, List<Thing> spawnedThings)
        {
            GenOption.GetAllMineableIn(structureRect, map);
            if (def is KCSG.StructureLayoutDef kcsgDef)
            {
                LayoutUtils.CleanRect(kcsgDef, map, structureRect, true, rot);
                kcsgDef.Generate(structureRect, map, spawnedThings, faction, false, rot);
            }
            else if (def is PrefabDef prefabDef)
            {
                foreach (var cell in structureRect.Cells)
                {
                    var edifice = cell.GetEdifice(map);
                    if (edifice != null && edifice.def.destroyable)
                    {
                        edifice.Destroy();
                    }
                }
                PrefabUtility.SpawnPrefab(prefabDef, map, structureRect.CenterCell, rot, faction, spawnedThings);
            }
        }

        private static IntVec3 GetTotalOffset(List<(StructurePatternOffset layout, Def def)> standardLayouts)
        {
            int minX = int.MaxValue, minZ = int.MaxValue, maxX = int.MinValue, maxZ = int.MinValue;
            foreach (var item in standardLayouts)
            {
                var size = GetLayoutSize(item.def);
                var rect = CellRect.CenteredOn(new IntVec3(item.layout.offset.x * size.x, 0, item.layout.offset.z * size.z), size);
                if (rect.minX < minX) minX = rect.minX;
                if (rect.minZ < minZ) minZ = rect.minZ;
                if (rect.maxX > maxX) maxX = rect.maxX;
                if (rect.maxZ > maxZ) maxZ = rect.maxZ;
            }
            return new IntVec3(-(minX + (maxX - minX) / 2), 0, -(minZ + (maxZ - minZ) / 2));
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