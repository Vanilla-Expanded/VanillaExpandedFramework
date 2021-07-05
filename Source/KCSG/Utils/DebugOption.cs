using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    public static class DebugOption
    {
        [DebugAction("Custom Structure Generation", "Quickspawn structure", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void QuickspawnStructure()
        {
            if (DefDatabase<StructureLayoutDef>.AllDefs.Count() > 0)
            {
                List<DebugMenuOption> list = new List<DebugMenuOption>();
                foreach (StructureLayoutDef localDef2 in DefDatabase<StructureLayoutDef>.AllDefs)
                {
                    StructureLayoutDef localDef = localDef2;
                    list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Tool, delegate ()
                    {
                        if (UI.MouseCell().InBounds(Find.CurrentMap))
                        {
                            RectUtils.HeightWidthFromLayout(localDef, out int h, out int w);
                            CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), w, h);

                            foreach (List<string> item in localDef.layouts)
                            {
                                GenUtils.GenerateRoomFromLayout(item, cellRect, Find.CurrentMap, localDef);
                            }
                        }
                    }));
                }
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            }
        }

        [DebugAction("Custom Structure Generation", "Destroy all hostile pawns on map", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveAllHostilePawns()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn.Faction != Faction.OfPlayer) pawn.Destroy(DestroyMode.KillFinalize);
            }
        }
    }
}