using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public static class Debug
    {
        public static void Message(string message)
        {
            if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message($"<color=orange>[KCSG]</color> {message}");
        }

        public static void Error(string message, string mod = "")
        {
            Log.Error($"[KCSG] {(mod != "" ? "[" + mod + "] " : "")}{message}");
        }

        [DebugAction("KCSG", "Quickspawn structure...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void QuickspawnStructure()
        {
            if (DefDatabase<StructureLayoutDef>.AllDefs.Count() > 0)
            {
                List<DebugMenuOption> list = new List<DebugMenuOption>();
                foreach (StructureLayoutDef layoutDef in DefDatabase<StructureLayoutDef>.AllDefs)
                {
                    list.Add(new DebugMenuOption(layoutDef.defName, DebugMenuOptionMode.Tool, delegate ()
                    {
                        Map map = Find.CurrentMap;
                        if (UI.MouseCell().InBounds(map))
                        {
                            CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), layoutDef.width, layoutDef.height);
                            GenUtils.GenerateLayout(layoutDef, cellRect, map);
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
                new DebugMenuOption($"{RoofDefOf.RoofConstructed.defName} 9*9", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 c in CellRect.CenteredOn(UI.MouseCell(), 4))
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
                 new DebugMenuOption($"{RoofDefOf.RoofRockThin.defName} 9*9", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 c in CellRect.CenteredOn(UI.MouseCell(), 4))
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
                }),
                new DebugMenuOption($"{RoofDefOf.RoofRockThick.defName} 9*9", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 c in CellRect.CenteredOn(UI.MouseCell(), 4))
                        Find.CurrentMap.roofGrid.SetRoof(c, RoofDefOf.RoofRockThick);
                })
            };

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }
        [DebugAction("KCSG", "Spawn rocks...", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnRocks()
        {
            List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>
            {
                //Granite
                new DebugMenuOption("Granite 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    GenSpawn.Spawn(ThingDefOf.Granite, UI.MouseCell(), Find.CurrentMap);
                }),
                new DebugMenuOption("Granite 3*3", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 1).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(ThingDefOf.Granite, item, Find.CurrentMap);
                    }
                }),
                new DebugMenuOption("Granite 5*5", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 2).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(ThingDefOf.Granite, item, Find.CurrentMap);
                    }
                }),
                //Sandstone
                new DebugMenuOption("Sandstone 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    GenSpawn.Spawn(ThingDefOf.Sandstone, UI.MouseCell(), Find.CurrentMap);
                }),
                new DebugMenuOption("Sandstone 3*3", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 1).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(ThingDefOf.Sandstone, item, Find.CurrentMap);
                    }
                }),
                new DebugMenuOption("Sandstone 5*5", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 2).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(ThingDefOf.Sandstone, item, Find.CurrentMap);
                    }
                }),
                //Marble
                new DebugMenuOption("Marble 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    GenSpawn.Spawn(DefOfs.Marble, UI.MouseCell(), Find.CurrentMap);
                }),
                new DebugMenuOption("Marble 3*3", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 1).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(DefOfs.Marble, item, Find.CurrentMap);
                    }
                }),
                new DebugMenuOption("Marble 5*5", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 2).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(DefOfs.Marble, item, Find.CurrentMap);
                    }
                }),
                //Limestone
                new DebugMenuOption("Limestone 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    GenSpawn.Spawn(DefOfs.Limestone, UI.MouseCell(), Find.CurrentMap);
                }),
                new DebugMenuOption("Limestone 3*3", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 1).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(DefOfs.Limestone, item, Find.CurrentMap);
                    }
                }),
                new DebugMenuOption("Limestone 5*5", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 2).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(DefOfs.Limestone, item, Find.CurrentMap);
                    }
                }),
                //Slate
                new DebugMenuOption("Slate 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    GenSpawn.Spawn(DefOfs.Slate, UI.MouseCell(), Find.CurrentMap);
                }),
                new DebugMenuOption("Slate 3*3", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 1).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(DefOfs.Slate, item, Find.CurrentMap);
                    }
                }),
                new DebugMenuOption("Slate 5*5", DebugMenuOptionMode.Tool, () =>
                {
                   foreach (IntVec3 item in CellRect.CenteredOn(UI.MouseCell(), 2).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(DefOfs.Slate, item, Find.CurrentMap);
                    }
                })

            };

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }
    }
}