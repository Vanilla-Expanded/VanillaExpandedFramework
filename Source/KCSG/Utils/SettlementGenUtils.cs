using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.Noise;

namespace KCSG
{
    public class SettlementGenUtils
    {
        public enum CellType
        {
            Free,
            Used,
            Sampling
        }

        public static CellRect rect;
        public static CellType[][] grid;
        public static IntVec3 size;

        public static List<IntVec3> usedSpots;

        public static List<TerrainDef> mapRoad;
        public static List<IntVec3> doors;

        public static DateTime startTime;
        public static int seed;

        public static ThingDef generalWallStuff;

        /// <summary>
        /// Generate settlement
        /// </summary>
        public static void Generate(ResolveParams rp, Map map, SettlementLayoutDef sld)
        {
            seed = map.Tile;
            startTime = DateTime.Now;
            // Setup
            rect = rp.rect;
            doors = new List<IntVec3>();
            size = map.Size;
            // Settlement height/width
            int height = rect.Height;
            int width = rect.Width;
            Debug.Message($"Generating in rect: {height}x{width}");
            // Init grid
            grid = new CellType[size.z][];
            for (int i = 0; i < size.z; i++)
            {
                grid[i] = new CellType[size.x];
            }
            // Get vanilla road if applicable
            mapRoad = SetRoadInfo(map);
            // Check all terrain for bridgeable, existing vanilla road(s), moutains
            foreach (var cell in rect)
            {
                TerrainDef t = map.terrainGrid.TerrainAt(cell);
                if ((mapRoad != null && mapRoad.Contains(t))
                    || (sld.avoidBridgeable && t.affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                    || (sld.avoidMountains && GenOption.GetMineableAt(cell) != null))
                {
                    grid[cell.z][cell.x] = CellType.Used;
                }
            }
            // Add main roads
            if (GenOption.RoadOptions.addMainRoad)
            {
                var mainRoadStart = DateTime.Now;

                var edgeCells = rect.EdgeCells.ToList();
                var terrain = GenOption.RoadOptions.mainRoadDef ?? TerrainDefOf.Concrete;
                var cellRect = CellRect.WholeMap(map);
                var mainRoadWidth = GenOption.RoadOptions.MainRoadWidth;

                for (int i = 0; i < GenOption.RoadOptions.mainRoadCount; i++)
                {
                    var start = edgeCells.RandomElement();
                    var target = edgeCells.FindAll(c => c.x != start.x || c.z != start.z).RandomElement();
                    var road = PathFinder.DoPath(start, target, map, rect, terrain);

                    WidenPath(road, map, terrain, mainRoadWidth);

                    if (GenOption.PropsOptions.addMainRoadProps && road != null)
                    {
                        usedSpots = new List<IntVec3>();
                        SpawnMainRoadProps(road);
                    }

                    if (GenOption.RoadOptions.mainRoadLinkToEdges)
                    {
                        if (CellFinder.TryFindRandomEdgeCellNearWith(start, 100, map, c => c.Walkable(map) && cellRect.Contains(c), out IntVec3 outOnetarget))
                        {
                            var outOne = PathFinder.DoPath(start, outOnetarget, map, cellRect, terrain);
                            if (outOne == null)
                                Debug.Message($"No path for {start} to {outOnetarget}");
                            WidenPath(outOne, map, terrain, mainRoadWidth);
                        }

                        if (CellFinder.TryFindRandomEdgeCellNearWith(target, 100, map, c => c.Walkable(map), out IntVec3 outTwotarget))
                        {
                            var outTwo = PathFinder.DoPath(target, outTwotarget, map, cellRect, terrain);
                            if (outTwo == null)
                                Debug.Message($"No path for {target} to {outTwotarget}");
                            WidenPath(outTwo, map, terrain, GenOption.RoadOptions.MainRoadWidth);
                        }
                    }
                }
                Debug.Message($"Main road time: {(DateTime.Now - mainRoadStart).TotalMilliseconds}ms.");
            }

            // Run poisson disk sampling
            var samplingStart = DateTime.Now;
            var rDistance = Math.Max(width, height);
            var vects = Sampling.Sample(rect, rect.CenterCell - new IntVec3(rDistance, 0, rDistance), rDistance, sld.samplingDistance);
            Debug.Message($"Sampling time: {(DateTime.Now - samplingStart).TotalMilliseconds}ms. Vects count: {vects?.Count}");

            if (sld.stuffableOptions.generalWallStuff)
                generalWallStuff = RandomUtils.RandomWallStuffWeighted(ThingDefOf.Wall);

            // Place and choose buildings.
            if (vects != null && vects.Count > 1)
            {
                var buildingStart = DateTime.Now;
                BuildingPlacement.Run(vects, sld, rp);
                Debug.Message($"Building time: {(DateTime.Now - buildingStart).TotalMilliseconds}ms. Doors count: {doors.Count}");
            }
        }

        /// <summary>
        /// Check a list to know if anything is near a given point
        /// </summary>
        public static bool NearUsedSpot(List<IntVec3> usedSpots, IntVec3 c, float dist)
        {
            if (usedSpots.NullOrEmpty())
                return false;

            for (int index = 0; index < usedSpots.Count; ++index)
            {
                if ((usedSpots[index] - c).LengthHorizontalSquared <= dist * dist)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Set terrain at pos if it's not a buildable, set bridge if needed.
        /// For road gen.
        /// </summary>
        public static void SetTerrainAt(IntVec3 c, Map map, TerrainDef roadDef)
        {
            GenOption.DespawnMineableAt(c);

            if (map.terrainGrid.TerrainAt(c) is TerrainDef terrainDef)
            {
                if (!terrainDef.BuildableByPlayer)
                {
                    //if (terrainDef.affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                    //    map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
                    //else
                        map.terrainGrid.SetTerrain(c, roadDef);
                }
            }
        }

        /// <summary>
        /// Widen a given path
        /// </summary>
        public static void WidenPath(List<IntVec3> path, Map map, TerrainDef terrain, int width)
        {
            if (width <= 0 || path.NullOrEmpty())
                return;

            IntVec3 last = path[0];
            int count = path.Count;
            for (int c = 0; c < count; c++)
            {
                IntVec3 curr = path[c];
                if (curr.InBounds(map))
                {
                    int widenBy = 0;
                    for (int i = 1; i <= width && widenBy < width; i++)
                    {
                        if (last.x != curr.x)
                        {
                            var c1 = new IntVec3(curr.x, 0, curr.z + i);
                            if (c1.InBounds(map))
                            {
                                grid[c1.z][c1.x] = CellType.Used;
                                SetTerrainAt(c1, map, terrain);
                                widenBy++;
                            }

                            var c2 = new IntVec3(curr.x, 0, curr.z - i);
                            if (widenBy < width && c2.InBounds(map))
                            {
                                grid[c2.z][c2.x] = CellType.Used;
                                SetTerrainAt(c2, map, terrain);
                                widenBy++;
                            }
                        }

                        if (last.z != curr.z)
                        {
                            var c1 = new IntVec3(curr.x + i, 0, curr.z);
                            if (c1.InBounds(map))
                            {
                                grid[c1.z][c1.x] = CellType.Used;
                                SetTerrainAt(c1, map, terrain);
                                widenBy++;
                            }

                            var c2 = new IntVec3(curr.x - i, 0, curr.z);
                            if (widenBy < width && c2.InBounds(map))
                            {
                                grid[c2.z][c2.x] = CellType.Used;
                                SetTerrainAt(c2, map, terrain);
                                widenBy++;
                            }
                        }
                    }
                    last = curr;
                }
            }
        }

        /// <summary>
        /// Spawn main road props
        /// </summary>
        public static void SpawnMainRoadProps(List<IntVec3> road)
        {
            if (road.NullOrEmpty())
                return;

            var map = BaseGen.globalSettings.map;
            for (int i = 0; i < road.Count; i++)
            {
                IntVec3 curr = road[i];
                IntVec3 last = i - 1 > 0 ? road[i - 1] : curr;

                if (Rand.Chance(GenOption.PropsOptions.mainRoadPropsChance)
                    && !NearUsedSpot(usedSpots, curr, GenOption.PropsOptions.mainRoadMinDistance))
                {
                    var rot = last.x != curr.x ? Rot4.East : Rot4.North;
                    var thingDef = GenOption.PropsOptions.RandomMainRoadProps();

                    if (thingDef != null)
                    {
                        var thing = ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef));
                        if (thingDef.rotatable)
                            GenSpawn.Spawn(thing, curr, map, rot);
                        else
                            GenSpawn.Spawn(thing, curr, map);

                        usedSpots.Add(curr);
                    }
                }
            }
        }

        /// <summary>
        /// Spawn link road props
        /// </summary>
        public static void SpawnLinkRoadProps(List<IntVec3> road)
        {
            if (road.NullOrEmpty())
                return;

            if (usedSpots == null)
                usedSpots = new List<IntVec3>();

            var map = BaseGen.globalSettings.map;
            for (int i = 0; i < road.Count; i++)
            {
                IntVec3 cell = road[i];

                if (Rand.Chance(GenOption.PropsOptions.linkRoadPropsChance))
                {
                    var thingDef = GenOption.PropsOptions.RandomLinkRoadProps();
                    if (thingDef == null)
                        return;

                    var cells = GenAdj.CellsAdjacent8Way(new TargetInfo(cell, map));

                    var matchingCells = new List<IntVec3>();
                    foreach (var c in cells)
                    {
                        if (grid[c.z][c.x] == CellType.Used)
                            continue;
                        if (NearUsedSpot(usedSpots, c, GenOption.PropsOptions.linkRoadMinDistance))
                            continue;

                        var rect = new CellRect(c.x, c.z, thingDef.size.x, thingDef.size.z);
                        foreach (var ce in rect)
                        {
                            if (grid[ce.z][ce.x] == CellType.Used || !ce.Walkable(map))
                                continue;
                        }

                        matchingCells.Add(c);
                    }

                    if (matchingCells.Count > 0)
                    {
                        var thing = ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef));
                        if (thingDef.rotatable)
                            GenSpawn.Spawn(thing, matchingCells.RandomElement(), map, Rot4.Random);
                        else
                            GenSpawn.Spawn(thing, matchingCells.RandomElement(), map);

                        usedSpots.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// Get road type already on map if applicable
        /// </summary>
        public static List<TerrainDef> SetRoadInfo(Map map)
        {
            if (map.TileInfo?.Roads?.Count > 0)
            {
                var preRoadTypes = new List<TerrainDef>();
                foreach (RimWorld.Planet.Tile.RoadLink roadLink in map.TileInfo.Roads)
                {
                    foreach (RoadDefGenStep rgs in roadLink.road.roadGenSteps)
                    {
                        if (rgs is RoadDefGenStep_Place rgsp && rgsp != null && rgsp.place is TerrainDef t && t != null && t != TerrainDefOf.Bridge)
                        {
                            preRoadTypes.Add(t);
                        }
                    }
                }
                return preRoadTypes;
            }
            return null;
        }

        /// <summary>
        /// Find all points spaced evenly
        /// </summary>
        public static class Sampling
        {
            /**
             * Poisson Disk Sampling
             *
             * Based from java source by Herman Tulleken
             * http://www.luma.co.za/labs/2008/02/27/poisson-disk-sampling/
             *
             * The algorithm is from the "Fast Poisson Disk Sampling in Arbitrary Dimensions" paper by Robert Bridson
             * http://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
             *
             **/

            public const int DefaultPointsPerIteration = 30;

            public static IntVec3 center;

            public static float rejectionSqDistance;
            public static float minimumSqDistance;

            public static List<IntVec3> activePoints;
            public static int activePointsCount;
            public static List<IntVec3> points;

            public static Random random;

            public static List<IntVec3> Sample(CellRect rect, IntVec3 topLeft, float rejectionDistance, float minimumDistance)
            {
                random = new Random();
                rejectionSqDistance = rejectionDistance * rejectionDistance;
                minimumSqDistance = minimumDistance * minimumDistance;
                center = rect.CenterCell;

                activePoints = new List<IntVec3>();
                points = new List<IntVec3>();

                AddFirstPoint(rect, topLeft);

                while (activePointsCount > 0)
                {
                    var listIndex = random.Next(activePointsCount - 1);

                    var point = activePoints[listIndex];
                    var found = false;

                    for (var k = 0; k < DefaultPointsPerIteration; k++)
                        found |= AddNextPoint(rect, point, minimumDistance);

                    if (!found)
                    {
                        activePoints.RemoveAt(listIndex);
                        activePointsCount--;
                    }
                }

                return points;
            }

            private static void AddFirstPoint(CellRect rect, IntVec3 topLeft)
            {
                while (true)
                {
                    var d = random.NextDouble();
                    int xr = (int)(topLeft.x + size.x * d);

                    d = random.NextDouble();
                    int yr = (int)(topLeft.z + size.z * d);

                    var p = new IntVec3(xr, 0, yr);
                    if (rect.Contains(p) && DistanceSquared(center, p) <= rejectionSqDistance)
                    {
                        grid[p.x][p.z] = CellType.Sampling;

                        activePoints.Add(p);
                        activePointsCount++;
                        points.Add(p);
                        return;
                    }
                }
            }

            private static bool AddNextPoint(CellRect rect, IntVec3 point, float minimumDistance)
            {
                var q = GenerateRandomAround(point, minimumDistance, random);

                if (rect.Contains(q) && DistanceSquared(center, q) <= rejectionSqDistance)
                {
                    int xSub = q.x - 2;
                    int xAdd = q.x + 3;
                    for (int x = xSub > 0 ? xSub : 0; x < (xAdd < size.x ? xAdd : size.x); x++)
                    {
                        int zSub = q.z - 2;
                        int zAdd = q.z + 3;
                        for (int z = zSub > 0 ? zSub : 0; z < (zAdd < size.z ? zAdd : size.z); z++)
                        {
                            if (grid[z][x] == CellType.Sampling)
                            {
                                var cell = new IntVec3(x, 0, z);
                                if (DistanceSquared(cell, q) < minimumSqDistance)
                                    return false;
                            }
                        }
                    }

                    activePoints.Add(q);
                    activePointsCount++;
                    points.Add(q);
                    grid[q.z][q.x] = CellType.Sampling;
                    return true;
                }
                return false;
            }

            private static IntVec3 GenerateRandomAround(IntVec3 center, float minimumDistance, Random random)
            {
                var d = random.NextDouble();
                var radius = minimumDistance + minimumDistance * d;

                d = random.NextDouble();
                var angle = (float)(Math.PI * 2) * d;

                int newX = (int)(radius * Math.Sin(angle));
                int newZ = (int)(radius * Math.Cos(angle));

                return new IntVec3(center.x + newX, 0, center.z + newZ);
            }

            private static float DistanceSquared(IntVec3 intVec3, IntVec3 other)
            {
                float x = intVec3.x - other.x;
                float y = intVec3.z - other.z;

                return (x * x) + (y * y);
            }
        }

        /// <summary>
        /// Place buildings on map, prevent overlap
        /// </summary>
        public static class BuildingPlacement
        {
            private const int maxTry = 100;

            private static readonly Dictionary<string, List<StructureLayoutDef>> structuresTagsCache = new Dictionary<string, List<StructureLayoutDef>>();

            /// <summary>
            /// Get structoptin weight
            /// </summary>
            private static float GetWeight(StructOption structOption, StructOption lastStructOption, Dictionary<string, int> structCount)
            {
                if (structCount.ContainsKey(structOption.tag))
                {
                    int count = structCount.TryGetValue(structOption.tag);

                    if (count >= structOption.count.max)
                        return 0;

                    if (lastStructOption != null && lastStructOption.tag == structOption.tag)
                        return 0.1f;

                    if (count < structOption.count.min)
                        return 2;

                    return 1;
                }

                if (structOption.count.min > 0)
                {
                    return 3;
                }

                return 1;
            }

            /// <summary>
            /// Check if all cell are free and if the layout need roof clearance, also check for roofed cells
            /// </summary>
            private static bool CanPlaceAt(IntVec3 point, StructureLayoutDef building, ResolveParams rp, int spaceAround, CellRect centerRect, bool inCenter)
            {
                var map = BaseGen.globalSettings.map;

                for (int x = point.x - spaceAround; x < building.sizes.x + point.x + spaceAround; x++)
                {
                    for (int z = point.z - spaceAround; z < building.sizes.z + point.z + spaceAround; z++)
                    {
                        var cell = new IntVec3(x, 0, z);
                        if (!inCenter && centerRect.Contains(cell))
                            return false;

                        if (InBound(x, z))
                        {
                            if (grid[z][x] == CellType.Used || !rp.rect.Contains(cell))
                            {
                                return false;
                            }

                            if (building.needRoofClearance)
                            {
                                var bRect = new CellRect(cell.x, cell.z, building.sizes.x, building.sizes.z);
                                foreach (var c in bRect)
                                {
                                    if (c.Roofed(map))
                                        return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }

            /// <summary>
            /// Set rect cells + space around to used
            /// </summary>
            private static void PlaceAt(IntVec3 point, StructureLayoutDef building, int spaceAround)
            {
                for (int x = point.x - spaceAround; x < building.sizes.x + point.x + spaceAround; x++)
                {
                    for (int z = point.z - spaceAround; z < building.sizes.z + point.z + spaceAround; z++)
                    {
                        grid[z][x] = CellType.Used;
                    }
                }
            }

            /// <summary>
            /// Grid bound check
            /// </summary>
            private static bool InBound(int x, int z) => x > 0 && x < grid[0].Length && z > 0 && z < grid.Length;

            /// <summary>
            /// Cache all structure in a dict per tag
            /// </summary>
            public static void CacheTags()
            {
                if (structuresTagsCache.NullOrEmpty())
                {
                    var layoutDefs = DefDatabase<StructureLayoutDef>.AllDefsListForReading;
                    for (int i = 0; i < layoutDefs.Count; i++)
                    {
                        var layout = layoutDefs[i];
                        if (!layout.tags.NullOrEmpty())
                        {
                            for (int o = 0; o < layout.tags.Count; o++)
                            {
                                string tag = layout.tags[o];
                                if (structuresTagsCache.ContainsKey(tag))
                                {
                                    structuresTagsCache[tag].Add(layout);
                                }
                                else
                                {
                                    structuresTagsCache.Add(tag, new List<StructureLayoutDef> { layout });
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Validate layout
            /// </summary>
            private static bool LayoutValidator(StructureLayoutDef def, Faction faction, HashSet<StructureLayoutDef> used)
            {
                if (!def.RequiredModLoaded)
                    return false;

                if (used.Contains(def))
                    return false;

                if (def.IsForSlaves && ModsConfig.IdeologyActive && faction.ideos != null && faction.ideos.PrimaryIdeo is Ideo p && !p.IdeoApprovesOfSlavery())
                    return false;

                return true;
            }

            /// <summary>
            /// Place buildings
            /// </summary>
            public static void Run(List<IntVec3> spawnPoints, SettlementLayoutDef sld, ResolveParams rp)
            {
                Dictionary<string, int> structCount = new Dictionary<string, int>();
                var usedLayoutDefs = new HashSet<StructureLayoutDef>();
                // Generate center building
                if (!sld.centerBuildings.centralBuildingTags.NullOrEmpty())
                {

                    var layout = RandomUtils.RandomLayoutFrom(structuresTagsCache[sld.centerBuildings.centralBuildingTags.RandomElement()]);
                    var cellRect = CellRect.CenteredOn(rect.CenterCell, layout.sizes.x, layout.sizes.z);

                    if (sld.centerBuildings.forceClean)
                    {
                        GenOption.GetAllMineableIn(cellRect, BaseGen.globalSettings.map);
                        LayoutUtils.CleanRect(layout, BaseGen.globalSettings.map, cellRect, false);
                    }

                    foreach (var cell in cellRect)
                        grid[cell.z][cell.x] = CellType.Used;

                    layout.Generate(cellRect, BaseGen.globalSettings.map);
                    usedLayoutDefs.Add(layout);
                }
                // Generate other buildings
                CellRect centerRect = CellRect.CenteredOn(rect.CenterCell, sld.centerBuildings.centerSize.x, sld.centerBuildings.centerSize.z);
                int periCount = sld.peripheralBuildings.allowedStructures.Count;

                StructOption lastOpt = null;
                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    IntVec3 vec = spawnPoints[i];
                    bool inCenter = centerRect.Contains(vec);
                    int spaceAround = inCenter ? sld.centerBuildings.spaceAround : sld.peripheralBuildings.spaceAround;

                    if (!inCenter && periCount == 0)
                        continue;

                    for (int o = 0; o < maxTry; o++)
                    {
                        StructOption opt;
                        if (inCenter)
                            sld.centerBuildings.allowedStructures.TryRandomElementByWeight(p => GetWeight(p, lastOpt, structCount), out opt);
                        else
                            sld.peripheralBuildings.allowedStructures.TryRandomElementByWeight(p => GetWeight(p, lastOpt, structCount), out opt);

                        if (opt == null)
                        {
                            Debug.Message($"No available structures. Allow more of one or multiples structure tags");
                            return;
                        }
                        lastOpt = opt;

                        var choices = new List<StructureLayoutDef>();
                        for (int s = 0; s < structuresTagsCache[opt.tag].Count; s++)
                        {
                            var layout = structuresTagsCache[opt.tag][s];
                            if (LayoutValidator(layout, rp.faction, usedLayoutDefs))
                                choices.Add(layout);
                        }

                        var layoutDef = choices.Count > 0 ? choices.RandomElement() : structuresTagsCache[opt.tag].RandomElement();

                        if (CanPlaceAt(vec, layoutDef, rp, spaceAround, centerRect, inCenter))
                        {
                            usedLayoutDefs.Add(layoutDef);

                            PlaceAt(vec, layoutDef, spaceAround);

                            if (structCount.ContainsKey(opt.tag))
                            {
                                structCount[opt.tag]++;
                            }
                            else
                            {
                                structCount.Add(opt.tag, 1);
                            }

                            CellRect rect = new CellRect(vec.x, vec.z, layoutDef.sizes.x, layoutDef.sizes.z);
                            layoutDef.Generate(rect, BaseGen.globalSettings.map);

                            if (layoutDef.isStorage)
                            {
                                ResolveParams rstock = rp;
                                rstock.rect = new CellRect(vec.x, vec.z, layoutDef.sizes.x, layoutDef.sizes.z);
                                BaseGen.symbolStack.Push("kcsg_storagezone", rstock, null);
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Make sure all map net are connected together
        /// </summary>
        public static class PowerNetManagement
        {
            private static readonly List<Thing> tmpThings = new List<Thing>();
            private static readonly List<IntVec3> tmpCells = new List<IntVec3>();
            private static readonly Dictionary<PowerNet, bool> tmpPowerNetPredicateResults = new Dictionary<PowerNet, bool>();

            public static void ManagePower(Map map)
            {
                map.skyManager.ForceSetCurSkyGlow(1f);
                map.powerNetManager.UpdatePowerNetsAndConnections_First();

                tmpThings.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial));

                UpdateDesiredPowerOutputForAllGenerators();
                EnsureBatteriesConnectedAndMakeSense(map);
                EnsurePowerUsersConnected(map);
                EnsureGeneratorsConnectedAndMakeSense(map);
                tmpThings.Clear();
            }

            private static void UpdateDesiredPowerOutputForAllGenerators()
            {
                for (int i = 0; i < tmpThings.Count; ++i)
                {
                    var thing = tmpThings[i];
                    if (IsPowerGenerator(thing))
                        thing.TryGetComp<CompPowerPlant>()?.UpdateDesiredPowerOutput();
                }
            }

            private static void EnsureBatteriesConnectedAndMakeSense(Map map)
            {
                for (int index = 0; index < tmpThings.Count; ++index)
                {
                    CompPowerBattery comp = tmpThings[index].TryGetComp<CompPowerBattery>();
                    if (comp != null)
                    {
                        PowerNet powerNet = comp.PowerNet;
                        if (powerNet == null || !HasAnyPowerGenerator(powerNet))
                        {
                            map.powerNetManager.UpdatePowerNetsAndConnections_First();
                            if (TryFindClosestReachableNet(comp.parent.Position, x => HasAnyPowerGenerator(x), map, out PowerNet foundNet, out IntVec3 closestTransmitter))
                            {
                                map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                                if (foundNet != null)
                                    SpawnTransmitters(tmpCells, map, comp.parent.Faction);
                            }
                        }
                    }
                }
            }

            private static void EnsurePowerUsersConnected(Map map)
            {
                for (int index = 0; index < tmpThings.Count; ++index)
                {
                    if (IsPowerUser(tmpThings[index]))
                    {
                        CompPowerTrader powerComp = tmpThings[index].TryGetComp<CompPowerTrader>();
                        PowerNet powerNet = powerComp.PowerNet;
                        if (powerNet != null && powerNet.hasPowerSource)
                        {
                            TryTurnOnImmediately(powerComp, map);
                        }
                        else
                        {
                            map.powerNetManager.UpdatePowerNetsAndConnections_First();
                            if (TryFindClosestReachableNet(powerComp.parent.Position, x => (double)x.CurrentEnergyGainRate() - powerComp.Props.PowerConsumption * (double)CompPower.WattsToWattDaysPerTick > 1.0000000116861E-07, map, out PowerNet foundNet, out IntVec3 closestTransmitter))
                            {
                                map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                                SpawnTransmitters(tmpCells, map, tmpThings[index].Faction);
                                TryTurnOnImmediately(powerComp, map);
                            }
                            else if (TryFindClosestReachableNet(powerComp.parent.Position, x => (double)x.CurrentStoredEnergy() > 1.0000000116861E-07, map, out foundNet, out closestTransmitter))
                            {
                                map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                                SpawnTransmitters(tmpCells, map, tmpThings[index].Faction);
                            }
                        }
                    }
                }
            }

            private static void EnsureGeneratorsConnectedAndMakeSense(Map map)
            {
                for (int index = 0; index < tmpThings.Count; ++index)
                {
                    if (IsPowerGenerator(tmpThings[index]))
                    {
                        PowerNet powerNet = tmpThings[index].TryGetComp<CompPower>().PowerNet;
                        if (powerNet == null || !HasAnyPowerUser(powerNet))
                        {
                            map.powerNetManager.UpdatePowerNetsAndConnections_First();
                            if (TryFindClosestReachableNet(tmpThings[index].Position, x => HasAnyPowerUser(x), map, out PowerNet _, out IntVec3 closestTransmitter))
                            {
                                map.floodFiller.ReconstructLastFloodFillPath(closestTransmitter, tmpCells);
                                SpawnTransmitters(tmpCells, map, tmpThings[index].Faction);
                            }
                        }
                    }
                }
            }

            private static bool IsPowerUser(Thing thing)
            {
                CompPowerTrader comp = thing.TryGetComp<CompPowerTrader>();
                if (comp == null)
                    return false;
                if ((double)comp.PowerOutput < 0.0)
                    return true;
                return !comp.PowerOn && comp.Props.PowerConsumption > 0.0;
            }

            private static bool IsPowerGenerator(Thing thing)
            {
                if (thing.TryGetComp<CompPowerPlant>() != null)
                    return true;

                CompPowerTrader comp = thing.TryGetComp<CompPowerTrader>();
                if (comp == null)
                    return false;
                if (comp.PowerOutput > 0)
                    return true;
                return !comp.PowerOn && comp.Props.PowerConsumption < 0;
            }

            private static bool HasAnyPowerGenerator(PowerNet net)
            {
                List<CompPowerTrader> powerComps = net.powerComps;
                for (int index = 0; index < powerComps.Count; ++index)
                {
                    if (IsPowerGenerator(powerComps[index].parent))
                        return true;
                }
                return false;
            }

            private static bool HasAnyPowerUser(PowerNet net)
            {
                List<CompPowerTrader> powerComps = net.powerComps;
                for (int index = 0; index < powerComps.Count; ++index)
                {
                    if (IsPowerUser(powerComps[index].parent))
                        return true;
                }
                return false;
            }

            private static bool TryFindClosestReachableNet(IntVec3 root, Predicate<PowerNet> predicate, Map map, out PowerNet foundNet, out IntVec3 closestTransmitter)
            {
                tmpPowerNetPredicateResults.Clear();

                PowerNet foundNetLocal = null;
                IntVec3 closestTransmitterLocal = IntVec3.Invalid;
                map.floodFiller.FloodFill(root, x => EverPossibleToTransmitPowerAt(x, map), x =>
                {
                    PowerNet powerNet = x.GetTransmitter(map)?.GetComp<CompPower>().PowerNet;
                    if (powerNet == null)
                        return false;

                    if (!tmpPowerNetPredicateResults.TryGetValue(powerNet, out bool flag))
                    {
                        flag = predicate(powerNet);
                        tmpPowerNetPredicateResults.Add(powerNet, flag);
                    }

                    if (!flag)
                        return false;

                    foundNetLocal = powerNet;
                    closestTransmitterLocal = x;
                    return true;
                }, rememberParents: true);

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

            private static void SpawnTransmitters(List<IntVec3> cells, Map map, Faction faction)
            {
                for (int index = 0; index < cells.Count; ++index)
                {
                    if (cells[index].GetTransmitter(map) == null)
                        GenSpawn.Spawn(AllDefOf.KCSG_PowerConduit, cells[index], map).SetFaction(faction);
                }
            }

            private static bool EverPossibleToTransmitPowerAt(IntVec3 c, Map map)
            {
                return c.GetTransmitter(map) != null || GenConstruct.CanBuildOnTerrain(ThingDefOf.PowerConduit, c, map, Rot4.North);
            }

            private static void TryTurnOnImmediately(CompPowerTrader powerComp, Map map)
            {
                if (powerComp.PowerOn)
                    return;

                map.powerNetManager.UpdatePowerNetsAndConnections_First();
                if (powerComp.PowerNet == null)
                    return;

                if (!FlickUtility.WantsToBeOn(powerComp.parent))
                    return;

                powerComp.PowerOn = true;
            }
        }

        /// <summary>
        /// Find all needed paths betweens doors
        /// </summary>
        public class Delaunay
        {
            /**
             * Delaunay class -only-
             *
             * MIT License
             *
             * Copyright (c) 2019 Patryk Grech
             *
             * Permission is hereby granted, free of charge, to any person obtaining a copy
             * of this software and associated documentation files (the "Software"), to deal
             * in the Software without restriction, including without limitation the rights
             * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
             * copies of the Software, and to permit persons to whom the Software is
             * furnished to do so, subject to the following conditions:
             *
             * The above copyright notice and this permission notice shall be included in all
             * copies or substantial portions of the Software.
             *
             **/
            private readonly double EPSILON = Math.Pow(2, -52);
            private readonly int[] EDGE_STACK = new int[512];

            public int[] Triangles { get; private set; }
            public int[] Halfedges { get; private set; }
            public IPoint[] Points { get; private set; }
            public int[] Hull { get; private set; }

            private readonly int hashSize;
            private readonly int[] hullPrev;
            private readonly int[] hullNext;
            private readonly int[] hullTri;
            private readonly int[] hullHash;

            private readonly double cx;
            private readonly double cy;

            private int trianglesLen;
            private readonly double[] coords;
            private readonly int hullStart;
            private readonly int hullSize;

            public interface IPoint
            {
                double X { get; set; }
                double Y { get; set; }

                IntVec3 IntVec3 { get; set; }
            }

            public interface IEdge
            {
                IPoint P { get; }
                IPoint Q { get; }
                int Index { get; }
            }

            public struct Point : IPoint
            {
                public double X { get; set; }
                public double Y { get; set; }
                public IntVec3 IntVec3 { get; set; }

                public Point(double x, double y)
                {
                    X = x;
                    Y = y;
                    IntVec3 = new IntVec3((int)x, 0, (int)y);
                }

                public override string ToString() => $"{X},{Y}";
            }

            public struct Edge : IEdge
            {
                public IPoint P { get; set; }
                public IPoint Q { get; set; }
                public int Index { get; set; }

                public Edge(int e, IPoint p, IPoint q)
                {
                    Index = e;
                    P = p;
                    Q = q;
                }
            }

            public Delaunay(List<IntVec3> doors)
            {
                List<IPoint> doorsC = new List<IPoint>();
                for (int i = 0; i < doors.Count; i++)
                {
                    var door = doors[i];
                    doorsC.Add(new Point(door.x, door.z));
                }
                IPoint[] points = doorsC.ToArray();

                Points = points;
                coords = new double[Points.Length * 2];

                for (var i = 0; i < Points.Length; i++)
                {
                    var p = Points[i];
                    coords[2 * i] = p.X;
                    coords[2 * i + 1] = p.Y;
                }

                var n = points.Length;
                var maxTriangles = 2 * n - 5;

                Triangles = new int[maxTriangles * 3];

                Halfedges = new int[maxTriangles * 3];
                hashSize = (int)Math.Ceiling(Math.Sqrt(n));

                hullPrev = new int[n];
                hullNext = new int[n];
                hullTri = new int[n];
                hullHash = new int[hashSize];

                var ids = new int[n];

                var minX = double.PositiveInfinity;
                var minY = double.PositiveInfinity;
                var maxX = double.NegativeInfinity;
                var maxY = double.NegativeInfinity;

                for (var i = 0; i < n; i++)
                {
                    var x = coords[2 * i];
                    var y = coords[2 * i + 1];
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                    ids[i] = i;
                }

                var cx = (minX + maxX) / 2;
                var cy = (minY + maxY) / 2;

                var minDist = double.PositiveInfinity;
                var i0 = 0;
                var i1 = 0;
                var i2 = 0;

                // pick a seed point close to the center
                for (int i = 0; i < n; i++)
                {
                    var d = Dist(cx, cy, coords[2 * i], coords[2 * i + 1]);
                    if (d < minDist)
                    {
                        i0 = i;
                        minDist = d;
                    }
                }
                var i0x = coords[2 * i0];
                var i0y = coords[2 * i0 + 1];

                minDist = double.PositiveInfinity;

                // find the point closest to the seed
                for (int i = 0; i < n; i++)
                {
                    if (i == i0) continue;
                    var d = Dist(i0x, i0y, coords[2 * i], coords[2 * i + 1]);
                    if (d < minDist && d > 0)
                    {
                        i1 = i;
                        minDist = d;
                    }
                }

                var i1x = coords[2 * i1];
                var i1y = coords[2 * i1 + 1];

                var minRadius = double.PositiveInfinity;

                // find the third point which forms the smallest circumcircle with the first two
                for (int i = 0; i < n; i++)
                {
                    if (i == i0 || i == i1) continue;
                    var r = Circumradius(i0x, i0y, i1x, i1y, coords[2 * i], coords[2 * i + 1]);
                    if (r < minRadius)
                    {
                        i2 = i;
                        minRadius = r;
                    }
                }
                var i2x = coords[2 * i2];
                var i2y = coords[2 * i2 + 1];

                if (minRadius == double.PositiveInfinity)
                {
                    throw new Exception("No Delaunay triangulation exists for this input.");
                }

                if (Orient(i0x, i0y, i1x, i1y, i2x, i2y))
                {
                    var i = i1;
                    var x = i1x;
                    var y = i1y;
                    i1 = i2;
                    i1x = i2x;
                    i1y = i2y;
                    i2 = i;
                    i2x = x;
                    i2y = y;
                }

                var center = Circumcenter(i0x, i0y, i1x, i1y, i2x, i2y);
                this.cx = center.X;
                this.cy = center.Y;

                var dists = new double[n];
                for (var i = 0; i < n; i++)
                {
                    dists[i] = Dist(coords[2 * i], coords[2 * i + 1], center.X, center.Y);
                }

                // sort the points by distance from the seed triangle circumcenter
                Quicksort(ids, dists, 0, n - 1);

                // set up the seed triangle as the starting hull
                hullStart = i0;
                hullSize = 3;

                hullNext[i0] = hullPrev[i2] = i1;
                hullNext[i1] = hullPrev[i0] = i2;
                hullNext[i2] = hullPrev[i1] = i0;

                hullTri[i0] = 0;
                hullTri[i1] = 1;
                hullTri[i2] = 2;

                hullHash[HashKey(i0x, i0y)] = i0;
                hullHash[HashKey(i1x, i1y)] = i1;
                hullHash[HashKey(i2x, i2y)] = i2;

                trianglesLen = 0;
                AddTriangle(i0, i1, i2, -1, -1, -1);

                double xp = 0;
                double yp = 0;

                for (var k = 0; k < ids.Length; k++)
                {
                    var i = ids[k];
                    var x = coords[2 * i];
                    var y = coords[2 * i + 1];

                    // skip near-duplicate points
                    if (k > 0 && Math.Abs(x - xp) <= EPSILON && Math.Abs(y - yp) <= EPSILON) continue;
                    xp = x;
                    yp = y;

                    // skip seed triangle points
                    if (i == i0 || i == i1 || i == i2) continue;

                    // find a visible edge on the convex hull using edge hash
                    var start = 0;
                    for (var j = 0; j < hashSize; j++)
                    {
                        var key = HashKey(x, y);
                        start = hullHash[(key + j) % hashSize];
                        if (start != -1 && start != hullNext[start]) break;
                    }

                    start = hullPrev[start];
                    var e = start;
                    var q = hullNext[e];

                    while (!Orient(x, y, coords[2 * e], coords[2 * e + 1], coords[2 * q], coords[2 * q + 1]))
                    {
                        e = q;
                        if (e == start)
                        {
                            e = int.MaxValue;
                            break;
                        }

                        q = hullNext[e];
                    }

                    if (e == int.MaxValue) continue; // likely a near-duplicate point; skip it

                    // add the first triangle from the point
                    var t = AddTriangle(e, i, hullNext[e], -1, -1, hullTri[e]);

                    // recursively flip triangles from the point until they satisfy the Delaunay condition
                    hullTri[i] = Legalize(t + 2);
                    hullTri[e] = t; // keep track of boundary triangles on the hull
                    hullSize++;

                    // walk forward through the hull, adding more triangles and flipping recursively
                    var next = hullNext[e];
                    q = hullNext[next];

                    while (Orient(x, y, coords[2 * next], coords[2 * next + 1], coords[2 * q], coords[2 * q + 1]))
                    {
                        t = AddTriangle(next, i, q, hullTri[i], -1, hullTri[next]);
                        hullTri[i] = Legalize(t + 2);
                        hullNext[next] = next; // mark as removed
                        hullSize--;
                        next = q;

                        q = hullNext[next];
                    }

                    // walk backward from the other side, adding more triangles and flipping
                    if (e == start)
                    {
                        q = hullPrev[e];

                        while (Orient(x, y, coords[2 * q], coords[2 * q + 1], coords[2 * e], coords[2 * e + 1]))
                        {
                            t = AddTriangle(q, i, e, -1, hullTri[e], hullTri[q]);
                            Legalize(t + 2);
                            hullTri[q] = t;
                            hullNext[e] = e; // mark as removed
                            hullSize--;
                            e = q;

                            q = hullPrev[e];
                        }
                    }

                    // update the hull indices
                    hullStart = hullPrev[i] = e;
                    hullNext[e] = hullPrev[next] = i;
                    hullNext[i] = next;

                    // save the two new edges in the hash table
                    hullHash[HashKey(x, y)] = i;
                    hullHash[HashKey(coords[2 * e], coords[2 * e + 1])] = e;
                }

                Hull = new int[hullSize];
                var s = hullStart;
                for (var i = 0; i < hullSize; i++)
                {
                    Hull[i] = s;
                    s = hullNext[s];
                }

                hullPrev = hullNext = hullTri = null; // get rid of temporary arrays

                //// trim typed triangle mesh arrays
                Triangles = Triangles.Take(trianglesLen).ToArray();
                Halfedges = Halfedges.Take(trianglesLen).ToArray();
            }

            private int Legalize(int a)
            {
                var i = 0;
                int ar;

                // recursion eliminated with a fixed-size stack
                while (true)
                {
                    var b = Halfedges[a];

                    /* if the pair of triangles doesn't satisfy the Delaunay condition
                     * (p1 is inside the circumcircle of [p0, pl, pr]), flip them,
                     * then do the same check/flip recursively for the new pair of triangles
                     *
                     *           pl                    pl
                     *          /||\                  /  \
                     *       al/ || \bl            al/    \a
                     *        /  ||  \              /      \
                     *       /  a||b  \    flip    /___ar___\
                     *     p0\   ||   /p1   =>   p0\---bl---/p1
                     *        \  ||  /              \      /
                     *       ar\ || /br             b\    /br
                     *          \||/                  \  /
                     *           pr                    pr
                     */
                    int a0 = a - a % 3;
                    ar = a0 + (a + 2) % 3;

                    if (b == -1)
                    { // convex hull edge
                        if (i == 0) break;
                        a = EDGE_STACK[--i];
                        continue;
                    }

                    var b0 = b - b % 3;
                    var al = a0 + (a + 1) % 3;
                    var bl = b0 + (b + 2) % 3;

                    var p0 = Triangles[ar];
                    var pr = Triangles[a];
                    var pl = Triangles[al];
                    var p1 = Triangles[bl];

                    var illegal = InCircle(
                        coords[2 * p0], coords[2 * p0 + 1],
                        coords[2 * pr], coords[2 * pr + 1],
                        coords[2 * pl], coords[2 * pl + 1],
                        coords[2 * p1], coords[2 * p1 + 1]);

                    if (illegal)
                    {
                        Triangles[a] = p1;
                        Triangles[b] = p0;

                        var hbl = Halfedges[bl];

                        // edge swapped on the other side of the hull (rare); fix the halfedge reference
                        if (hbl == -1)
                        {
                            var e = hullStart;
                            do
                            {
                                if (hullTri[e] == bl)
                                {
                                    hullTri[e] = a;
                                    break;
                                }
                                e = hullPrev[e];
                            } while (e != hullStart);
                        }
                        Link(a, hbl);
                        Link(b, Halfedges[ar]);
                        Link(ar, bl);

                        var br = b0 + (b + 1) % 3;

                        // don't worry about hitting the cap: it can only happen on extremely degenerate input
                        if (i < EDGE_STACK.Length)
                        {
                            EDGE_STACK[i++] = br;
                        }
                    }
                    else
                    {
                        if (i == 0) break;
                        a = EDGE_STACK[--i];
                    }
                }

                return ar;
            }

            private static bool InCircle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
            {
                var dx = ax - px;
                var dy = ay - py;
                var ex = bx - px;
                var ey = by - py;
                var fx = cx - px;
                var fy = cy - py;

                var ap = dx * dx + dy * dy;
                var bp = ex * ex + ey * ey;
                var cp = fx * fx + fy * fy;

                return dx * (ey * cp - bp * fy) -
                       dy * (ex * cp - bp * fx) +
                       ap * (ex * fy - ey * fx) < 0;
            }

            private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
            {
                var t = trianglesLen;

                Triangles[t] = i0;
                Triangles[t + 1] = i1;
                Triangles[t + 2] = i2;

                Link(t, a);
                Link(t + 1, b);
                Link(t + 2, c);

                trianglesLen += 3;
                return t;
            }

            private void Link(int a, int b)
            {
                Halfedges[a] = b;
                if (b != -1) Halfedges[b] = a;
            }

            private int HashKey(double x, double y)
            {
                return (int)(Math.Floor(PseudoAngle(x - cx, y - cy) * hashSize) % hashSize);
            }

            private static double PseudoAngle(double dx, double dy)
            {
                var p = dx / (Math.Abs(dx) + Math.Abs(dy));
                return (dy > 0 ? 3 - p : 1 + p) / 4; // [0..1]
            }

            private static void Quicksort(int[] ids, double[] dists, int left, int right)
            {
                if (right - left <= 20)
                {
                    for (var i = left + 1; i <= right; i++)
                    {
                        var temp = ids[i];
                        var tempDist = dists[temp];
                        var j = i - 1;
                        while (j >= left && dists[ids[j]] > tempDist) ids[j + 1] = ids[j--];
                        ids[j + 1] = temp;
                    }
                }
                else
                {
                    var median = (left + right) >> 1;
                    var i = left + 1;
                    var j = right;
                    Swap(ids, median, i);
                    if (dists[ids[left]] > dists[ids[right]]) Swap(ids, left, right);
                    if (dists[ids[i]] > dists[ids[right]]) Swap(ids, i, right);
                    if (dists[ids[left]] > dists[ids[i]]) Swap(ids, left, i);

                    var temp = ids[i];
                    var tempDist = dists[temp];
                    while (true)
                    {
                        do i++; while (dists[ids[i]] < tempDist);
                        do j--; while (dists[ids[j]] > tempDist);
                        if (j < i) break;
                        Swap(ids, i, j);
                    }
                    ids[left + 1] = ids[j];
                    ids[j] = temp;

                    if (right - i + 1 >= j - left)
                    {
                        Quicksort(ids, dists, i, right);
                        Quicksort(ids, dists, left, j - 1);
                    }
                    else
                    {
                        Quicksort(ids, dists, left, j - 1);
                        Quicksort(ids, dists, i, right);
                    }
                }
            }

            private static void Swap(int[] arr, int i, int j)
            {
                (arr[j], arr[i]) = (arr[i], arr[j]);
            }

            private static bool Orient(double px, double py, double qx, double qy, double rx, double ry) => (qy - py) * (rx - qx) - (qx - px) * (ry - qy) < 0;

            private static double Circumradius(double ax, double ay, double bx, double by, double cx, double cy)
            {
                var dx = bx - ax;
                var dy = by - ay;
                var ex = cx - ax;
                var ey = cy - ay;
                var bl = dx * dx + dy * dy;
                var cl = ex * ex + ey * ey;
                var d = 0.5 / (dx * ey - dy * ex);
                var x = (ey * bl - dy * cl) * d;
                var y = (dx * cl - ex * bl) * d;
                return x * x + y * y;
            }

            private static Point Circumcenter(double ax, double ay, double bx, double by, double cx, double cy)
            {
                var dx = bx - ax;
                var dy = by - ay;
                var ex = cx - ax;
                var ey = cy - ay;
                var bl = dx * dx + dy * dy;
                var cl = ex * ex + ey * ey;
                var d = 0.5 / (dx * ey - dy * ex);
                var x = ax + (ey * bl - dy * cl) * d;
                var y = ay + (dx * cl - ex * bl) * d;

                return new Point(x, y);
            }

            private static double Dist(double ax, double ay, double bx, double by)
            {
                var dx = ax - bx;
                var dy = ay - by;
                return dx * dx + dy * dy;
            }

            public IEnumerable<IEdge> GetEdges()
            {
                for (var e = 0; e < Triangles.Length; e++)
                {
                    if (e > Halfedges[e])
                    {
                        var p = Points[Triangles[e]];
                        var q = Points[Triangles[(e % 3 == 2) ? e - 2 : e + 1]];
                        yield return new Edge(e, p, q);
                    }
                }
            }
        }

        /// <summary>
        /// Path finding
        /// </summary>
        public static class PathFinder
        {
            public class Location
            {
                public IntVec3 vec3;
                public int distance;
                public int cost;

                public Location Parent { get; set; }
                public int CostDistance => cost + distance;
                public int X => vec3.x;
                public int Y => vec3.z;

                public void SetDistance(int targetX, int targetY)
                {
                    distance = Math.Abs(targetX - X) + Math.Abs(targetY - Y);
                }
            }

            public static List<IntVec3> DoPath(IntVec3 iStart, IntVec3 iTarget, Map map, CellRect rect, TerrainDef terrain)
            {
                // Setup
                var start = new Location { vec3 = iStart };
                var target = new Location { vec3 = iTarget };

                start.SetDistance(target.X, target.Y);

                var lActive = new List<Location>
                {
                    start
                };
                var lVisited = new List<Location>();
                var lActiveCount = 1;

                while (lActiveCount > 0)
                {
                    var checkCell = lActive.OrderByDescending(x => x.CostDistance).Last();
                    // Found target
                    if (checkCell.X == target.X && checkCell.Y == target.Y)
                    {
                        var cell = checkCell;
                        var cells = new List<IntVec3>();

                        while (cell != null)
                        {
                            IntVec3 vec3 = cell.vec3;

                            SetTerrainAt(vec3, map, terrain);
                            grid[vec3.z][vec3.x] = CellType.Used;

                            cells.Add(vec3);
                            cell = cell.Parent;
                        }

                        return cells;
                    }

                    lVisited.Add(checkCell);
                    lActive.Remove(checkCell);
                    lActiveCount--;

                    var adj = GetWalkableAdjacentCells(map, checkCell, target, rect);
                    for (int i = 0; i < adj.Count; i++)
                    {
                        var cell = adj[i];
                        if (lVisited.Any(x => x.vec3 == cell.vec3))
                            continue;

                        if (lActive.Find(x => x.vec3 == cell.vec3) is Location loc && loc.CostDistance > checkCell.CostDistance)
                        {
                            lActive.Remove(loc);
                            lActive.Add(cell);
                        }
                        else
                        {
                            lActive.Add(cell);
                            lActiveCount++;
                        }
                    }
                }

                return null;
            }

            private static List<Location> GetWalkableAdjacentCells(Map map, Location current, Location target, CellRect rect)
            {
                var result = new List<Location>();
                var adj = new List<Location>()
                {
                    new Location { vec3 = new IntVec3(current.X, 0, current.Y - 1), Parent = current },
                    new Location { vec3 = new IntVec3(current.X, 0, current.Y + 1), Parent = current },
                    new Location { vec3 = new IntVec3(current.X - 1, 0, current.Y), Parent = current },
                    new Location { vec3 = new IntVec3(current.X + 1, 0, current.Y), Parent = current }
                };

                bool InBound(IntVec3 cell) => cell.x >= rect.minX && cell.x <= rect.maxX && cell.z >= rect.minZ && cell.z <= rect.maxZ;

                var pathGrid = map.pathing.Normal.pathGrid;
                for (int i = 0; i < 4; i++)
                {
                    var loc = adj[i];
                    var vec = loc.vec3;

                    if (InBound(vec))
                    {
                        var mineable = GenOption.GetMineableAt(vec);
                        if ((pathGrid.WalkableFast(vec) || mineable != null))
                        {
                            loc.SetDistance(target.X, target.Y);
                            loc.cost = current.cost + 1 + (mineable != null ? 5 : pathGrid.PerceivedPathCostAt(vec));
                            result.Add(loc);
                        }
                    }
                }

                return result;
            }
        }
    }
}