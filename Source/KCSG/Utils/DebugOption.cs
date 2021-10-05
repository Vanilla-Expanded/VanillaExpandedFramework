using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    public static class DebugOption
    {
        [DebugAction("KCSG", "Quickspawn structure...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

        [DebugAction("KCSG", "Destroy all pawns", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveAllPawns()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
            {
                pawn.Destroy(DestroyMode.KillFinalize);
            }
        }

        [DebugAction("KCSG", "Destroy all hostile pawns", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveAllHostilePawns()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn.Faction != Faction.OfPlayer) pawn.Destroy(DestroyMode.KillFinalize);
            }
        }

        [DebugAction("KCSG", "Map infos", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PointCenter()
        {
            Log.Message($"Map faction: {Find.CurrentMap.ParentFaction}");
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("Map center", "Map center", LetterDefOf.NeutralEvent, new LookTargets(Find.CurrentMap.Center, Find.CurrentMap)));
        }

        [DebugAction("KCSG", "Spawn roof...", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnRoof()
        {
            List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>
            {
                // Constructed
                new DebugMenuOption($"{RoofDefOf.RoofConstructed.defName} 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    Find.CurrentMap.roofGrid.SetRoof(UI.MouseCell(), RoofDefOf.RoofConstructed);
                }),
                new DebugMenuOption($"{RoofDefOf.RoofConstructed.defName} 3*3", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 c in CellRect.CenteredOn(UI.MouseCell(), 1))
                        Find.CurrentMap.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
                }),
                // Thin
                new DebugMenuOption($"{RoofDefOf.RoofRockThin.defName} 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    Find.CurrentMap.roofGrid.SetRoof(UI.MouseCell(), RoofDefOf.RoofRockThin);
                }),
                new DebugMenuOption($"{RoofDefOf.RoofRockThin.defName} 3*3", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 c in CellRect.CenteredOn(UI.MouseCell(), 1))
                        Find.CurrentMap.roofGrid.SetRoof(c, RoofDefOf.RoofRockThin);
                }),
                // Thick
                new DebugMenuOption($"{RoofDefOf.RoofRockThick.defName} 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    Find.CurrentMap.roofGrid.SetRoof(UI.MouseCell(), RoofDefOf.RoofRockThick);
                }),
                new DebugMenuOption($"{RoofDefOf.RoofRockThick.defName} 3*3", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 c in CellRect.CenteredOn(UI.MouseCell(), 1))
                        Find.CurrentMap.roofGrid.SetRoof(c, RoofDefOf.RoofRockThick);
                })
            };

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }
    }
}