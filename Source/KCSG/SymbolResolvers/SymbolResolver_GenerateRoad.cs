using System;
using System.Linq;
using RimWorld.BaseGen;
using Verse;
using static KCSG.SettlementGenUtils;

namespace KCSG
{
    internal class SymbolResolver_GenerateRoad : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            var delaunayStart = DateTime.Now;
            var edges = new Delaunay(doors).GetEdges();
            Debug.Message($"Delaunat time: {(DateTime.Now - delaunayStart).TotalMilliseconds}ms. Edges count: {edges.Count()}");
            /*foreach (var edge in edges)
            {
                var path = PathFinder.GetPath(edge.P.IntVec3, edge.Q.IntVec3, grid, map);
                if (path != null)
                {
                    Debug.Message($"Path cells count: {path.Count}");
                    for (int o = 0; o < path.Count; o++)
                    {
                        var cell = path[o];
                        map.terrainGrid.SetTerrain(cell, GenOption.settlementLayoutDef.roadDef);

                        var things = map.thingGrid.ThingsListAtFast(cell);
                        for (int p = 0; p < things.Count; p++)
                        {
                            var thing = things[p];
                            if (thing.def.passability == Traversability.Impassable)
                            {
                                thing.DeSpawn();
                            }
                        }
                    }
                }
            }

            doors.ForEach(d =>
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Gold), d, map, WipeMode.Vanish);
            });*/

            // TODO: Field gen
            // BaseGen.symbolStack.Push("kcsg_addfields", rp, null);

            // rp.rect.EdgeCells.ToList().ForEach(cell => SpawnConduit(cell, map));
            Debug.Message($"Total time (without pawn gen): {(DateTime.Now - startTime).TotalSeconds}s.");
        }
    }
}