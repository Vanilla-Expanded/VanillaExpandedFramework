using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
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

        [DebugAction("Vanilla Framework Expanded", "Quickspawn structure", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void QuickspawnStructure()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (StructureLayoutDef localDef2 in DefDatabase<StructureLayoutDef>.AllDefs)
            {
                StructureLayoutDef localDef = localDef2;
                list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Tool, delegate ()
                {
                    if (UI.MouseCell().InBounds(Find.CurrentMap))
                    {
                        KCSG_Utilities.HeightWidthFromLayout(localDef, out int h, out int w);
                        CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), w, h);

                        int count = 0;
                        foreach (List<string> item in localDef.layouts)
                        {
                            KCSG_Utilities.GenerateRoomFromLayout(item, cellRect, Find.CurrentMap, localDef);
                        }
                    }
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }
}
