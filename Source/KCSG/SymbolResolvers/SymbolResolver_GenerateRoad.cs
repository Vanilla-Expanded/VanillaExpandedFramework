using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
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

            if (GenOption.RoadOptions.addLinkRoad)
            {
                var doorsLeadingOutside = new List<IntVec3>();
                for (int i = 0; i < doors.Count; i++)
                {
                    var door = doors[i];
                    // Only register door that lead outside
                    var adj = GenAdjFast.AdjacentCellsCardinal(door);
                    var anyLeadOutside = false;
                    for (int o = 0; o < adj.Count; o++)
                    {
                        var adjCell = adj[o];
                        if (adjCell.UsesOutdoorTemperature(map) || GenOption.GetMineableAt(adjCell) != null)
                        {
                            anyLeadOutside = true;
                            break;
                        }
                    }

                    if (anyLeadOutside)
                        doorsLeadingOutside.Add(door);
                }

                if (doorsLeadingOutside.Count > 0)
                {
                    var delaunayStart = DateTime.Now;
                    var edges = new Delaunay(doorsLeadingOutside).GetEdges();
                    Debug.Message($"Delaunay time: {(DateTime.Now - delaunayStart).TotalMilliseconds}ms. Edges count: {edges.Count()}");

                    var linkStart = DateTime.Now;
                    foreach (var edge in edges)
                    {
                        var road = PathFinder.DoPath(edge.P.IntVec3, edge.Q.IntVec3, map, rp.rect, GenOption.RoadOptions.linkRoadDef ?? TerrainDefOf.Concrete);

                        if (road != null)
                        {
                            WidenPath(road, map, GenOption.RoadOptions.linkRoadDef ?? TerrainDefOf.Concrete, GenOption.RoadOptions.LinkRoadWidth);

                            if (GenOption.PropsOptions.addLinkRoadProps)
                                SpawnLinkRoadProps(road);
                        }
                    }
                    Debug.Message($"Link road (+ props) time: {(DateTime.Now - linkStart).TotalMilliseconds}ms.");
                }
            }
        }
    }
}