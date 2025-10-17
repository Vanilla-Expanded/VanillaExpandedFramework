using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public static class LayoutUtils
    {
        /// <summary>
        /// Generate layoutDef in rect
        /// </summary>
        public static void Generate(this StructureLayoutDef layout, CellRect rect, Map map, Faction factionOverride = null, bool forceNullFaction = false)
        {
            // Get random wall stuff if randomizeWall is set to true
            ThingDef wallForRoom = null;
            if (layout.randomizeWallStuffAtGen || (GenOption.StuffableOptions != null && GenOption.StuffableOptions.randomizeWall))
                wallForRoom = RandomUtils.RandomWallStuffWeighted(ThingDefOf.Wall);
            // Generate all layouts
            Faction faction;
            if (forceNullFaction)
            {
                faction = null;
            }
            else
            {

                if (factionOverride != null)
                {
                    faction = factionOverride;
                }
                else
                {
                    faction = map.ParentFaction;
                }

            }

            Rot4 rot = Rot4.North;
            if (layout.randomRotation)
            {
                rot = new Rot4(Rand.Range(0, 4));
            }

            IntVec2 rotatedSizes = rot.IsHorizontal ? new IntVec2(layout.sizes.z, layout.sizes.x) : layout.sizes;

            int offsetX = (rect.Width - rotatedSizes.x) / 2;
            int offsetZ = (rect.Height - rotatedSizes.z) / 2;
            IntVec3 offset = new IntVec3(rect.minX + offsetX, 0, rect.minZ + offsetZ);

            for (int index = 0; index < layout.layouts.Count; index++)
            {
                for (int z = 0; z < rotatedSizes.z; z++)
                {
                    for (int x = 0; x < rotatedSizes.x; x++)
                    {
                        IntVec3 cell = new IntVec3(x, 0, z) + offset;
                        var srcCoords = GetSourceCoords(x, z, rot, layout.sizes);
                        var symbol = layout._layouts[index][srcCoords.z, srcCoords.x];

                        if (cell.InBounds(map) && symbol != null)
                        {
                            if (symbol.thingDef != null && !symbol.thingDef.rotatable)
                            {
                                IntVec2 size = symbol.thingDef.size;
                                GenAdj.AdjustForRotation(ref cell, ref size, symbol.rotation, rot);
                            }

                            var rotatedSymbol = CreateRotatedSymbol(symbol, rot);
                            rotatedSymbol.Generate(layout, map, cell, faction, wallForRoom);
                        }
                    }
                }
            }

            // Reconnect all power buildings
            try
            {
                ReconnectAllPowerBuildings(map);
            }
            catch (Exception ex)
            {
                Log.Message("[VEF] Failed to reconnect exported power buildings: " + ex);
            }
            GenerateTerrainGrid(layout, map, rot, rotatedSizes, offset);
            GenerateRoofGrid(layout, map, rot, rotatedSizes, offset);
        }

        private static IntVec3 GetSourceCoords(int x, int z, Rot4 rot, IntVec2 srcSize)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return new IntVec3(x, 0, z);
                case 1:
                    return new IntVec3(srcSize.x - 1 - z, 0, x);
                case 2:
                    return new IntVec3(srcSize.x - 1 - x, 0, srcSize.z - 1 - z);
                case 3:
                    return new IntVec3(z, 0, srcSize.z - 1 - x);
                default:
                    return IntVec3.Invalid;
            }
        }

        private static SymbolDef CreateRotatedSymbol(SymbolDef symbolToCopy, Rot4 rot)
        {
            if (rot == Rot4.North || symbolToCopy.thingDef == null)
            {
                return symbolToCopy;
            }
            SymbolDef newSymbol = symbolToCopy.ShallowClone();
            newSymbol.defName = $"{symbolToCopy.defName}_runtime_rotated_{rot.AsInt}";
            if (newSymbol.thingDef.rotatable)
            {
                newSymbol.rotation = symbolToCopy.rotation.Rotated(Rot4.GetRelativeRotation(Rot4.North, rot));
            }

            return newSymbol;
        }
        private static void GenerateRoofGrid(StructureLayoutDef layout, Map map, Rot4 rot, IntVec2 rotatedSizes, IntVec3 offset)
        {
            if (layout._roofGrid != null && layout._roofGrid.Length > 0)
            {
                for (int z = 0; z < rotatedSizes.z; z++)
                {
                    for (int x = 0; x < rotatedSizes.x; x++)
                    {
                        var cell = new IntVec3(x, 0, z) + offset;
                        var srcCoords = GetSourceCoords(x, z, rot, layout.sizes);
                        var roof = layout._roofGrid[srcCoords.z, srcCoords.x];

                        if (cell.InBounds(map) && roof != null)
                        {
                            if (roof == "0" && layout.forceGenerateRoof)
                            {
                                map.roofGrid.SetRoof(cell, null);
                                continue;
                            }

                            var currentRoof = cell.GetRoof(map);
                            if (roof == "1" && (layout.forceGenerateRoof || currentRoof == null))
                            {
                                map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
                                ClearInterferesWithRoof(cell, map);
                                continue;
                            }

                            if (roof == "2" && (layout.forceGenerateRoof || currentRoof == null || currentRoof == RoofDefOf.RoofConstructed))
                            {
                                map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThin);
                                ClearInterferesWithRoof(cell, map);
                                continue;
                            }

                            if (roof == "3")
                            {
                                map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
                                ClearInterferesWithRoof(cell, map);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Despawn everything that block/interfer with roofs
        /// </summary>
        private static void ClearInterferesWithRoof(IntVec3 cell, Map map)
        {
            var t = cell.GetPlant(map);
            if (t != null && t.def.plant != null && t.def.plant.interferesWithRoof)
                t.DeSpawn();

            GenOption.DespawnMineableAt(cell);
        }
        /// <summary>
        /// Generate terrain grid from layout
        /// </summary>
        private static void GenerateTerrainGrid(StructureLayoutDef layout, Map map, Rot4 rot, IntVec2 rotatedSizes, IntVec3 offset)
        {
            if (layout._foundationGrid != null && layout._foundationGrid.Length > 0)
            {
                for (int z = 0; z < rotatedSizes.z; z++)
                {
                    for (int x = 0; x < rotatedSizes.x; x++)
                    {
                        var cell = new IntVec3(x, 0, z) + offset;
                        var srcCoords = GetSourceCoords(x, z, rot, layout.sizes);
                        var foundation = layout._foundationGrid[srcCoords.z, srcCoords.x];
                        if (foundation == null || !cell.InBounds(map))
                            continue;

                        GenOption.DespawnMineableAt(cell);
                        map.terrainGrid.SetFoundation(cell, foundation);
                    }
                }
            }
            // Handle under grid
            if (layout._underGrid != null && layout._underGrid.Length > 0)
            {
                for (int z = 0; z < rotatedSizes.z; z++)
                {
                    for (int x = 0; x < rotatedSizes.x; x++)
                    {
                        var cell = new IntVec3(x, 0, z) + offset;
                        var srcCoords = GetSourceCoords(x, z, rot, layout.sizes);
                        var under = layout._underGrid[srcCoords.z, srcCoords.x];
                        if (under == null || !cell.InBounds(map))
                            continue;

                        map.terrainGrid.SetUnderTerrain(cell, under);
                    }
                }
            }
            // Handle temp grid
            if (layout._tempGrid != null && layout._tempGrid.Length > 0)
            {
                for (int z = 0; z < rotatedSizes.z; z++)
                {
                    for (int x = 0; x < rotatedSizes.x; x++)
                    {
                        var cell = new IntVec3(x, 0, z) + offset;
                        var srcCoords = GetSourceCoords(x, z, rot, layout.sizes);
                        var temp = layout._tempGrid[srcCoords.z, srcCoords.x];
                        if (temp == null || !cell.InBounds(map))
                            continue;

                        GenOption.DespawnMineableAt(cell);
                        map.terrainGrid.SetTempTerrain(cell, temp);
                    }
                }
            }
            // Handle terrain grid
            if (layout._terrainGrid == null || layout._terrainGrid.Length == 0)
                return;

            var useColorGrid = layout._terrainColorGrid != null && layout._terrainColorGrid.Length > 0;
            for (int z = 0; z < rotatedSizes.z; z++)
            {
                for (int x = 0; x < rotatedSizes.x; x++)
                {
                    var cell = new IntVec3(x, 0, z) + offset;
                    var srcCoords = GetSourceCoords(x, z, rot, layout.sizes);
                    var terrain = layout._terrainGrid[srcCoords.z, srcCoords.x];
                    if (terrain == null || !cell.InBounds(map))
                        continue;

                    GenOption.DespawnMineableAt(cell);
                    map.terrainGrid.SetTerrain(cell, terrain);

                    if (useColorGrid)
                    {
                        var color = layout._terrainColorGrid[srcCoords.z, srcCoords.x];
                        map.terrainGrid.SetTerrainColor(cell, color);
                    }
                }
            }
        }

        /// <summary>
        /// Clean structure spawn rect. Full clean remove everything but player/map pawns.
        /// Normal clean remove filth, non-natural buildings and stone/slag chunks
        /// </summary>
        public static void CleanRect(StructureLayoutDef layout, Map map, CellRect rect, bool fullClean)
        {
            var mapFaction = map.ParentFaction;
            var player = Faction.OfPlayer;
            var cells = rect.Cells?.ToList();

            if (cells.Count > 0)
            {
                if (layout == null || layout._roofGrid == null || layout._roofGrid.Length == 0)
                {
                    for (int i = 0; i < cells.Count; i++)
                    {
                        var cell = cells[i];
                        if (cell.InBounds(map))
                            CleanAt(cell, map, fullClean, mapFaction, player);
                    }
                }
                else
                {
                    var width = rect.Width;
                    var height = rect.Height;

                    for (int h = 0; h < height; h++)
                    {
                        for (int w = 0; w < width; w++)
                        {
                            var cell = cells[(h * width) + w];
                            var roof = layout._roofGrid[h, w];

                            if (cell.InBounds(map) && roof != ".")
                                CleanAt(cell, map, fullClean, mapFaction, player);
                        }
                    }
                }
                // Update roof grid if it was a fullclean
                if (fullClean)
                    map.roofGrid?.RoofGridUpdate();
            }

        }

        /// <summary>
        /// Clean at a cell. Terrain & things
        /// </summary>
        private static void CleanAt(IntVec3 c, Map map, bool fullClean, Faction mapFaction, Faction player)
        {
            // Clean things
            var things = c.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Thing thing && thing.Spawned)
                {
                    if (thing is Pawn p && p.Faction != mapFaction && p.Faction != player)
                    {
                        thing.DeSpawn();
                    }
                    else if (fullClean && thing.def.category != ThingCategory.Plant)
                    {
                        thing.DeSpawn();
                    }
                    // Clear filth, buildings, stone/slag chunks
                    else if (thing.def.category == ThingCategory.Filth ||
                             (thing.def.category == ThingCategory.Building && !thing.def.building.isNaturalRock) ||
                             (thing.def.thingCategories != null && thing.def.thingCategories.Contains(ThingCategoryDefOf.Chunks)))
                    {
                        thing.DeSpawn();
                    }
                }
            }
            // Clean terrain
            if (map.terrainGrid.UnderTerrainAt(c) is TerrainDef terrain && terrain != null)
            {
                map.terrainGrid.SetTerrain(c, terrain);
            }
            // Clean roof
            if (fullClean)
                map.roofGrid.SetRoof(c, null);
        }

        public static void ReconnectAllPowerBuildings(Map map)
        {
            map.powerNetManager.UpdatePowerNetsAndConnections_First();
            UpdateDesiredPowerOutputForAllGenerators(map);
            EnsureTransmittersConnected(map);
            EnsurePowerUsersConnected(map);
            map.powerNetManager.UpdatePowerNetsAndConnections_First();
        }

        private static List<Thing> tmpThings = new List<Thing>();
        private static void UpdateDesiredPowerOutputForAllGenerators(Map map)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (IsPowerGenerator(tmpThings[i]))
                {
                    tmpThings[i].TryGetComp<CompPowerPlant>()?.UpdateDesiredPowerOutput();
                }
            }
        }

        private static void EnsureTransmittersConnected(Map map)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                var transmitter = tmpThings[i].TryGetComp<CompPowerTransmitter>();
                if (transmitter != null && transmitter.PowerNet is null)
                {
                    if (TryFindClosestReachableNet(transmitter.parent.Position, (PowerNet x) => HasAnyPowerGenerator(x), map, out var foundNet, out var closestTransmitter))
                    {
                        transmitter.transNet = foundNet;
                        transmitter.TryManualReconnect();
                    }
                }
            }
        }
        private static bool HasAnyPowerGenerator(PowerNet net)
        {
            List<CompPowerTrader> powerComps = net.powerComps;
            for (int i = 0; i < powerComps.Count; i++)
            {
                if (IsPowerGenerator(powerComps[i].parent))
                {
                    return true;
                }
            }
            return false;
        }
        private static void EnsurePowerUsersConnected(Map map)
        {
            tmpThings.Clear();
            tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                if (!IsPowerUser(tmpThings[i]))
                {
                    continue;
                }
                CompPowerTrader powerComp = tmpThings[i].TryGetComp<CompPowerTrader>();
                PowerNet powerNet = powerComp.PowerNet;
                if (powerNet != null && powerNet.hasPowerSource)
                {
                    TryTurnOnImmediately(powerComp, map);
                    continue;
                }
                map.powerNetManager.UpdatePowerNetsAndConnections_First();
                TryTurnOnImmediately(powerComp, map);
            }
        }

        private static bool IsPowerUser(Thing thing)
        {
            CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
            if (compPowerTrader != null)
            {
                if (!(compPowerTrader.PowerOutput < 0f))
                {
                    if (!compPowerTrader.PowerOn)
                    {
                        return compPowerTrader.Props.PowerConsumption > 0f;
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        private static bool IsPowerGenerator(Thing thing)
        {
            if (thing.TryGetComp<CompPowerPlant>() != null)
            {
                return true;
            }
            CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
            if (compPowerTrader != null)
            {
                if (!(compPowerTrader.PowerOutput > 0f))
                {
                    if (!compPowerTrader.PowerOn)
                    {
                        return compPowerTrader.Props.PowerConsumption < 0f;
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        private static Dictionary<PowerNet, bool> tmpPowerNetPredicateResults = new Dictionary<PowerNet, bool>();
        private static bool TryFindClosestReachableNet(IntVec3 root, Predicate<PowerNet> predicate, Map map, out PowerNet foundNet, out IntVec3 closestTransmitter)
        {
            tmpPowerNetPredicateResults.Clear();
            PowerNet foundNetLocal = null;
            IntVec3 closestTransmitterLocal = IntVec3.Invalid;
            map.floodFiller.FloodFill(root, (IntVec3 x) => EverPossibleToTransmitPowerAt(x, map), delegate (IntVec3 x)
            {
                PowerNet powerNet = x.GetTransmitter(map)?.GetComp<CompPower>().PowerNet;
                if (powerNet == null)
                {
                    return false;
                }
                if (!tmpPowerNetPredicateResults.TryGetValue(powerNet, out var value))
                {
                    value = predicate(powerNet);
                    tmpPowerNetPredicateResults.Add(powerNet, value);
                }
                if (value)
                {
                    foundNetLocal = powerNet;
                    closestTransmitterLocal = x;
                    return true;
                }
                return false;
            }, int.MaxValue, rememberParents: true);
            tmpPowerNetPredicateResults.Clear();
            if (foundNetLocal != null)
            {
                foundNet = foundNetLocal;
                closestTransmitter = closestTransmitterLocal;
                return true;
            }
            foundNet = null;
            closestTransmitter = IntVec3.Invalid;
            return false;
        }
        private static bool EverPossibleToTransmitPowerAt(IntVec3 c, Map map)
        {
            if (c.GetTransmitter(map) == null)
            {
                return GenConstruct.CanBuildOnTerrain(ThingDefOf.PowerConduit, c, map, Rot4.North);
            }
            return true;
        }
        private static void TryTurnOnImmediately(CompPowerTrader powerComp, Map map)
        {
            if (!powerComp.PowerOn)
            {
                map.powerNetManager.UpdatePowerNetsAndConnections_First();
                if (powerComp.PowerNet != null)
                {
                    var flickComp = powerComp.parent.TryGetComp<CompFlickable>();
                    var compSchedule = powerComp.parent.TryGetComp<CompSchedule>();
                    if (compSchedule != null)
                    {
                        compSchedule.Allowed = true;
                    }

                    if (flickComp != null && !flickComp.SwitchIsOn)
                    {
                        flickComp.SwitchIsOn = true;
                    }



                    powerComp.PowerOn = true;
                    if (compSchedule != null)
                    {
                        compSchedule.RecalculateAllowed();
                    }
                }
                else
                {
                    Log.Message("Can't enable " + powerComp);
                }
            }
        }
    }
}
