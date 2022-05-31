using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace KCSG
{
    public class SettlementGenUtils
    {
        public enum CellType
        {
            Free,
            Used
        }

        public static CellRect rect;
        public static CellType[][] grid;

        public static List<TerrainDef> mapRoad;
        public static List<IntVec3> doors;

        public static void StartGen(ResolveParams rp, Map map, SettlementLayoutDef settlementLayoutDef)
        {
            // Setup
            rect = rp.rect;
            doors = new List<IntVec3>();
            var size = map.Size;
            // Settlement height/width
            int height = rect.Height;
            int width = rect.Width;
            // Init grid
            grid = new CellType[size.z][];
            for (int i = 0; i < size.z; i++)
            {
                grid[i] = new CellType[size.x];
            }
            // Get vanilla road if applicable
            mapRoad = GenUtils.SetRoadInfo(map);
            // Check all terrain for bridgeable and existing vanilla road(s)
            foreach (var cell in rect)
            {
                TerrainDef t = map.terrainGrid.TerrainAt(cell);
                if (t.affordances.Contains(TerrainAffordanceDefOf.Bridgeable) || (mapRoad != null && mapRoad.Contains(t)))
                {
                    grid[cell.z][cell.x] = CellType.Used;
                }
            }

            // Get smallest allowed structure
            int radius = 9999;
            for (int i = 0; i < settlementLayoutDef.allowedStructures.Count; i++)
            {
                var allowedLayouts = DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Contains(settlementLayoutDef.allowedStructures[i].tag));
                for (int o = 0; o < allowedLayouts.Count; o++)
                {
                    var layout = allowedLayouts[o];
                    radius = Math.Min(radius, Math.Min(layout.height, layout.width));
                }
            }
            // Add to radius to ensure no building touch one another
            radius += 2;

            // Run poisson disk sampling
            var vects = PoissonDiskSampling.Run(rect.Corners.ElementAt(2), radius, new Random());
            Debug.Message($"Vects count: {vects?.Count}");

            // Place and choose buildings. Also push resolvers
            BuildingPlacement.Run(vects, settlementLayoutDef, rp);
        }

        public static class PoissonDiskSampling
        {
            public static List<IntVec3> Run(IntVec3 p0, int radius, Random r)
            {
                int mTries = 50;
                List<IntVec3> points = new List<IntVec3>();
                List<IntVec3> active = new List<IntVec3>();

                /* Initial point */
                points.Add(p0);
                active.Add(p0);

                while (active.Count > 0)
                {
                    int random_index = r.Next(active.Count);
                    IntVec3 p = active[random_index];

                    for (int tries = 1; tries <= mTries; tries++)
                    {
                        /* Pick a random angle */
                        int theta = r.Next(361);
                        /* Pick a random radius between r and 1.5r */
                        int new_radius = r.Next(radius, (int)(1.5f * radius));
                        /* Find X & Y coordinates relative to point p */
                        int pnewx = (int)(p.x + new_radius * Math.Cos(ConvertToRadians(theta)));
                        int pnewy = (int)(p.z + new_radius * Math.Sin(ConvertToRadians(theta)));
                        IntVec3 pnew = new IntVec3(pnewx, 0, pnewy);

                        if (rect.Contains(pnew) && InsideCircles(pnew, radius, points))
                        {
                            points.Add(pnew);
                            active.Add(pnew);
                            break;
                        }
                        else if (tries == mTries)
                        {
                            active.RemoveAt(random_index);
                        }
                    }
                }
                return points;
            }

            private static double ConvertToRadians(int angle) => Math.PI / 180 * angle;

            private static bool InsideCircle(IntVec3 center, IntVec3 tile, float radius)
            {
                float dx = center.x - tile.x;
                float dy = center.z - tile.z;
                float distance_squared = dx * dx + dy * dy;
                return distance_squared <= radius * radius;
            }

            private static bool InsideCircles(IntVec3 tile, float radius, List<IntVec3> allPoints)
            {
                for (int i = 0; i < allPoints.Count; i++)
                {
                    var point = allPoints[i];
                    if (InsideCircle(point, tile, radius))
                        return false;
                }
                return true;
            }
        }

        public static class BuildingPlacement
        {
            public static float GetWeight(StructOption structOption, Dictionary<string, int> structCount)
            {
                if (structCount.ContainsKey(structOption.tag))
                {
                    int count = structCount.TryGetValue(structOption.tag);
                    if (count < structOption.count.min)
                    {
                        return 2f;
                    }
                    else if (count == structOption.count.max)
                    {
                        return 0f;
                    }
                    else
                    {
                        return 1f;
                    }
                }
                else
                {
                    return 3f;
                }
            }

            public static bool CanPlaceAt(IntVec3 point, StructureLayoutDef building, ResolveParams rp)
            {
                for (int x = point.x - 1; x < building.width + point.x + 1; x++)
                {
                    for (int z = point.z - 1; z < building.height + point.z + 1; z++)
                    {
                        var cell = new IntVec3(x, 0, z);
                        if (!rp.rect.Contains(cell) || grid[z][x] == CellType.Used)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public static void PlaceAt(IntVec3 point, StructureLayoutDef building)
            {
                for (int x = point.x; x < building.width + point.x; x++)
                {
                    for (int z = point.z; z < building.height + point.z; z++)
                    {
                        grid[z][x] = CellType.Used;
                    }
                }
            }

            public static void Run(List<IntVec3> spawnPoints, SettlementLayoutDef sld, ResolveParams rp)
            {
                var layoutDefs = DefDatabase<StructureLayoutDef>.AllDefsListForReading;
                Dictionary<string, int> structCount = new Dictionary<string, int>();

                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    IntVec3 vector = spawnPoints[i];
                    for (int o = 0; o < 50; o++)
                    {
                        sld.allowedStructures.TryRandomElementByWeight(p => GetWeight(p, structCount), out StructOption option);

                        if (option == null)
                        {
                            Debug.Message($"No available structures. Allow more of one or multiples structure tags");
                            return;
                        }

                        var allWithTag = new List<StructureLayoutDef>();
                        for (int p = 0; p < layoutDefs.Count; p++)
                        {
                            var layout = layoutDefs[p];
                            if (layout.tags.Contains(option.tag))
                            {
                                allWithTag.Add(layout);
                            }
                        }
                        var layoutDef = GenUtils.ChooseStructureLayoutFrom(allWithTag);

                        if (CanPlaceAt(vector, layoutDef, rp))
                        {
                            PlaceAt(vector, layoutDef);

                            if (structCount.ContainsKey(option.tag))
                            {
                                structCount[option.tag]++;
                            }
                            else
                            {
                                structCount.Add(option.tag, 1);
                            }

                            CellRect rect = new CellRect(vector.x, vector.z, layoutDef.width, layoutDef.height);

                            for (int p = 0; p < layoutDef.layouts.Count; p++)
                            {
                                GenUtils.GenerateRoomFromLayout(layoutDef, p, rect, BaseGen.globalSettings.map);
                            }
                            GenUtils.GenerateRoofGrid(layoutDef, rect, BaseGen.globalSettings.map);

                            if (layoutDef.isStorage)
                            {
                                ResolveParams rstock = rp;
                                rstock.rect = new CellRect(vector.x, vector.z, layoutDef.width, layoutDef.height);
                                BaseGen.symbolStack.Push("kcsg_storagezone", rstock, null);
                            }
                            break;
                        }
                    }
                }
            }
        }

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
                            if (TryFindClosestReachableNet(powerComp.parent.Position, x => (double)x.CurrentEnergyGainRate() - powerComp.Props.basePowerConsumption * (double)CompPower.WattsToWattDaysPerTick > 1.0000000116861E-07, map, out PowerNet foundNet, out IntVec3 closestTransmitter))
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
                return !comp.PowerOn && comp.Props.basePowerConsumption > 0.0;
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
                return !comp.PowerOn && comp.Props.basePowerConsumption < 0;
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
                        GenSpawn.Spawn(ThingDefOf.PowerConduit, cells[index], map).SetFaction(faction);
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
                if (powerComp.PowerNet == null || (double)powerComp.PowerNet.CurrentEnergyGainRate() <= 1.0000000116861E-07)
                    return;
                powerComp.PowerOn = true;
            }
        }

        public static class Delaunay
        {
            public class Edge
            {
                public Edge(IntVec3 pointA, IntVec3 pointB)
                {
                    Point1 = pointA;
                    Point2 = pointB;
                }

                public IntVec3 Point1 { get; }
                public IntVec3 Point2 { get; }

                public bool Equals(Edge other)
                {
                    return (Point1 == other.Point1 && Point2 == other.Point2) || (Point1 == other.Point2 && Point2 == other.Point1);
                }
            }

            public class Triangle
            {
                public double RadiusSquared;

                public Triangle(IntVec3 pointA, IntVec3 pointB, IntVec3 pointC)
                {
                    Points[0] = pointA;
                    if (!IsCounterClockwise(pointA, pointB, pointC))
                    {
                        Points[1] = pointC;
                        Points[2] = pointB;
                    }
                    else
                    {
                        Points[1] = pointB;
                        Points[2] = pointC;
                    }
                    UpdateCircumcircle();
                }

                public IntVec3 Circumcenter { get; private set; }
                public IntVec3[] Points { get; } = new IntVec3[3];

                public bool IsPointInsideCircumcircle(IntVec3 point)
                {
                    double d_squared = (point.x - Circumcenter.x) * (point.x - Circumcenter.x) + (point.z - Circumcenter.z) * (point.z - Circumcenter.z);
                    return d_squared < RadiusSquared;
                }

                private bool IsCounterClockwise(IntVec3 pointA, IntVec3 pointB, IntVec3 pointC)
                {
                    double result = (pointB.x - pointA.x) * (pointC.z - pointA.z) - (pointC.x - pointA.x) * (pointB.z - pointA.z);
                    return result > 0;
                }

                private void UpdateCircumcircle()
                {
                    IntVec3 p0 = Points[0];
                    IntVec3 p1 = Points[1];
                    IntVec3 p2 = Points[2];

                    float dA = p0.x * p0.x + p0.z * p0.z;
                    float dB = p1.x * p1.x + p1.z * p1.z;
                    float dC = p2.x * p2.x + p2.z * p2.z;

                    int aux1 = (int)(dA * (p2.z - p1.z) + dB * (p0.z - p2.z) + dC * (p1.z - p0.z));
                    int aux2 = (int)-((dA * (p2.x - p1.x)) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
                    int div = 2 * (p0.x * (p2.z - p1.z) + p1.x * (p0.z - p2.z) + p2.x * (p1.z - p0.z));

                    if (div == 0)
                    {
                        Log.Error(new DivideByZeroException().ToString());
                    }

                    IntVec3 center = new IntVec3(aux1 / div, 0, aux2 / div);
                    Circumcenter = center;
                    RadiusSquared = (center.x - p0.x) * (center.x - p0.x) + (center.z - p0.z) * (center.z - p0.z);
                }
            }

            public static List<Edge> Run(IntVec3 origin, List<IntVec3> points, int maxX, int maxY)
            {
                var point0 = origin;
                var point1 = new IntVec3(origin.x, 0, origin.z + maxY);
                var point2 = new IntVec3(origin.x + maxX, 0, origin.z + maxY);
                var point3 = new IntVec3(origin.x + maxX, 0, origin.z);

                var triangle1 = new Triangle(point0, point1, point2);
                var triangle2 = new Triangle(point0, point2, point3);

                var triangulation = new List<Triangle> { triangle1, triangle2 };

                for (int i = 0; i < points.Count; i++)
                {
                    var point = points[i];
                    List<Triangle> badTriangles = FindBadTriangles(point, triangulation);
                    List<Edge> polygon = FindAllEdges(badTriangles);

                    triangulation.RemoveAll(t => badTriangles.Contains(t));

                    foreach (Edge edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
                    {
                        Triangle triangle = new Triangle(point, edge.Point1, edge.Point2);
                        triangulation.Add(triangle);
                    }
                }

                return FindAllEdges(triangulation);
            }

            private static List<Triangle> FindBadTriangles(IntVec3 point, List<Triangle> triangles)
            {
                List<Triangle> badTriangles = new List<Triangle>();
                for (int i = 0; i < triangles.Count; i++)
                {
                    var triangle = triangles[i];
                    if (triangle.IsPointInsideCircumcircle(point))
                        badTriangles.Add(triangle);
                }

                return badTriangles;
            }

            private static List<Edge> FindAllEdges(List<Triangle> triangles)
            {
                HashSet<Edge> edges = new HashSet<Edge>();
                foreach (var triangle in triangles)
                {
                    edges.Add(new Edge(triangle.Points[0], triangle.Points[1]));
                    edges.Add(new Edge(triangle.Points[1], triangle.Points[2]));
                    edges.Add(new Edge(triangle.Points[2], triangle.Points[0]));
                }

                return edges.ToList();
            }
        }

        public static class PathFinder
        {
            private const int NumPathNodes = 8;
            private const float StepDistMin = 2f;
            private const float StepDistMax = 14f;

            private static readonly int StartRadialIndex = GenRadial.NumCellsInRadius(StepDistMax);
            private static readonly int EndRadialIndex = GenRadial.NumCellsInRadius(StepDistMin);
            private static readonly int RadialIndexStride = 3;

            public static List<IntVec3> TryFindWalkPath(Map map, IntVec3 root)
            {
                List<IntVec3> path = new List<IntVec3>
                {
                    root
                };

                IntVec3 start = root;
                for (int index1 = 0; index1 < NumPathNodes; ++index1)
                {
                    IntVec3 intVec3_1 = IntVec3.Invalid;
                    float num1 = -1f;
                    for (int startRadialIndex = StartRadialIndex; startRadialIndex > EndRadialIndex; startRadialIndex -= RadialIndexStride)
                    {
                        IntVec3 intVec3_2 = start + GenRadial.RadialPattern[startRadialIndex];
                        if (intVec3_2.InBounds(map)
                            /*&& intVec3_2.Standable(map)*/
                            && !intVec3_2.GetTerrain(map).avoidWander
                            /*&& GenSight.LineOfSight(start, intVec3_2, map)*/
                            /*&& !intVec3_2.Roofed(map)*/)
                        {
                            float num2 = 10000f;
                            IntVec3 intVec3_3;
                            for (int index2 = 0; index2 < path.Count; ++index2)
                            {
                                double num3 = (double)num2;
                                intVec3_3 = path[index2] - intVec3_2;
                                double lengthManhattan = intVec3_3.LengthManhattan;
                                num2 = (float)(num3 + lengthManhattan);
                            }
                            intVec3_3 = intVec3_2 - root;
                            float lengthManhattan1 = intVec3_3.LengthManhattan;
                            if (lengthManhattan1 > 40f)
                                num2 *= Mathf.InverseLerp(70f, 40f, lengthManhattan1);
                            if (path.Count >= 2)
                            {
                                intVec3_3 = path[path.Count - 1] - path[path.Count - 2];
                                float angleFlat1 = intVec3_3.AngleFlat;
                                intVec3_3 = intVec3_2 - start;
                                float angleFlat2 = intVec3_3.AngleFlat;
                                float num4;
                                if ((double)angleFlat2 > (double)angleFlat1)
                                {
                                    num4 = angleFlat2 - angleFlat1;
                                }
                                else
                                {
                                    float num5 = angleFlat1 - 360f;
                                    num4 = angleFlat2 - num5;
                                }
                                if ((double)num4 > 110.0)
                                    num2 *= 0.01f;
                            }
                            if (path.Count >= 4)
                            {
                                intVec3_3 = start - root;
                                int lengthManhattan2 = intVec3_3.LengthManhattan;
                                intVec3_3 = intVec3_2 - root;
                                int lengthManhattan3 = intVec3_3.LengthManhattan;
                                if (lengthManhattan2 < lengthManhattan3)
                                    num2 *= 1E-05f;
                            }
                            if ((double)num2 > (double)num1)
                            {
                                intVec3_1 = intVec3_2;
                                num1 = num2;
                            }
                        }
                    }
                    if ((double)num1 < 0.0)
                    {
                        return null;
                    }
                    path.Add(intVec3_1);
                    start = intVec3_1;
                }
                path.Add(root);
                return path;
            }
        }
    }
}