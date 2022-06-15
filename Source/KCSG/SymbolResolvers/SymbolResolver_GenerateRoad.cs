using System;
using System.Collections.Generic;
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

            var doorsLeadingOutside = new List<IntVec3>();
            for (int i = 0; i < doors.Count; i++)
            {
                var door = doors[i];
                // Only register door that lead outside
                var adj = GenAdjFast.AdjacentCellsCardinal(door);
                var anyLeadOutside = false;
                for (int o = 0; o < adj.Count; o++)
                {
                    if (adj[o].UsesOutdoorTemperature(map) || adj[o].GetFirstMineable(map) != null)
                    {
                        anyLeadOutside = true;
                        break;
                    }
                }

                if (anyLeadOutside)
                    doorsLeadingOutside.Add(door);
            }

            var delaunayStart = DateTime.Now;
            var edges = new Delaunay(doorsLeadingOutside).GetEdges();
            Debug.Message($"Delaunay time: {(DateTime.Now - delaunayStart).TotalMilliseconds}ms. Edges count: {edges.Count()}");

            foreach (var edge in edges)
            {
                PathFinder.DoPath(edge.P.IntVec3, edge.Q.IntVec3, map, GenOption.sld, rp.rect, GenOption.sld.roadOptions.addLinkRoad);
            }

            Debug.Message($"Total time (without pawn gen): {(DateTime.Now - startTime).TotalSeconds}s.");

            // Flood refog
            if (map.mapPawns.FreeColonistsSpawned.Count > 0)
            {
                Debug.Message($"Refog map.");
                FloodFillerFog.DebugRefogMap(map);
            }
        }
    }
}