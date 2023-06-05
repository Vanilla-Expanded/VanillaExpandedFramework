using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    internal class FallingStructureArrival : PawnsArrivalModeWorker
    {
        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            PawnsArrivalModeWorkerUtility.DropInDropPodsNearSpawnCenter(parms, pawns);
        }

        public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
        {
            if (!DropCellFinder.TryFindRaidDropCenterClose(out IntVec3 near, map, true, true, true, -1))
            {
                near = DropCellFinder.FindRaidDropCenterDistant(map, false);
            }
            TransportPodsArrivalActionUtility.DropTravelingTransportPods(dropPods, near, map);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            parms.spawnCenter = FindRect(map, GenOption.fallingLayout.sizes.z, GenOption.fallingLayout.sizes.x);
            parms.spawnRotation = Rot4.Random;
            return true;
        }

        public IntVec3 FindRect(Map map, int height, int width)
        {
            int maxSize = Math.Max(width, height);
            for (int tries = 0; tries < 100; tries++)
            {
                CellRect rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(maxSize, map), width, height);
                if (rect.Cells.ToList().Any(i => !i.Walkable(map) || !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium)))
                    continue;
                else
                    return rect.CenterCell;
            }
            return CellFinder.RandomNotEdgeCell(maxSize, map);
        }
    }
}