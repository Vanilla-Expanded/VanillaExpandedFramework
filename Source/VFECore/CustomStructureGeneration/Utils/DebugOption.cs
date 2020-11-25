using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore.CustomStructureGeneration.Utils
{
    public static class DebugOption
    {
        [DebugAction("Vanilla Framework Expanded", "Destroy all hostile pawns on map", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveAllHostilePawns()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn.Faction != Faction.OfPlayer) pawn.Destroy(DestroyMode.KillFinalize);
            }
        }
    }
}
