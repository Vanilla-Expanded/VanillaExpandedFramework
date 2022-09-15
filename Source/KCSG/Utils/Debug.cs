using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public static class Debug
    {
        public static bool Enabled => VFECore.VFEGlobal.settings.enableVerboseLogging;

        public static void Message(string message)
        {
            if (VFECore.VFEGlobal.settings.enableVerboseLogging)
                Log.Message($"<color=orange>[KCSG]</color> {message}");
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
                            GenOption.mineables = new Dictionary<IntVec3, Mineable>();
                            foreach (var cell in cellRect)
                            {
                                if (cell.InBounds(map))
                                    GenOption.mineables.Add(cell, cell.GetFirstMineable(map));
                            }
                            GenUtils.GenerateLayout(layoutDef, cellRect, map);
                        }
                    }));
                }
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            }
        }

        [DebugAction("KCSG", "Quickspawn temp structure...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void QuickspawnTempStructure()
        {
            if (Dialog_ExportWindow.exportedLayouts.Count > 0)
            {
                List<DebugMenuOption> list = new List<DebugMenuOption>();
                foreach (var pair in Dialog_ExportWindow.exportedLayouts)
                {
                    var layoutDef = pair.Value;
                    list.Add(new DebugMenuOption(layoutDef.defName, DebugMenuOptionMode.Tool, delegate ()
                    {
                        Map map = Find.CurrentMap;
                        if (UI.MouseCell().InBounds(map))
                        {
                            CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), layoutDef.width, layoutDef.height);
                            GenOption.mineables = new Dictionary<IntVec3, Mineable>();
                            foreach (var cell in cellRect)
                            {
                                if (cell.InBounds(map))
                                    GenOption.mineables.Add(cell, cell.GetFirstMineable(map));
                            }
                            GenUtils.GenerateLayout(layoutDef, cellRect, map);
                        }
                    }));
                }
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            }
        }

        [DebugAction("KCSG", "Quickspawn symbol...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void QuickspawnSymbol()
        {
            if (DefDatabase<SymbolDef>.AllDefs.Count() > 0)
            {
                List<DebugMenuOption> list = new List<DebugMenuOption>();
                foreach (SymbolDef sym in DefDatabase<SymbolDef>.AllDefs)
                {
                    if (sym.modContentPack != null && sym.modContentPack.Name != null)
                    {
                        list.Add(new DebugMenuOption(sym.defName, DebugMenuOptionMode.Tool, delegate ()
                        {
                            Map map = Find.CurrentMap;
                            if (UI.MouseCell().InBounds(map))
                            {
                                Message(sym.ToString());
                                GenUtils.GenerateSymbol(sym, null, map, UI.MouseCell(), map.ParentFaction, null);
                            }
                        }));
                    }
                }
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            }
        }

        [DebugAction("KCSG", "Quickspawn temp symbol...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void QuickspawnTempSymbol()
        {
            if (Dialog_ExportWindow.exportedSymbolsDef.Count > 0)
            {
                List<DebugMenuOption> list = new List<DebugMenuOption>();
                foreach (var sym in Dialog_ExportWindow.exportedSymbolsDef)
                {
                    if (sym.modContentPack != null && sym.modContentPack.Name != null)
                    {
                        list.Add(new DebugMenuOption(sym.defName, DebugMenuOptionMode.Tool, delegate ()
                        {
                            Map map = Find.CurrentMap;
                            if (UI.MouseCell().InBounds(map))
                            {
                                Message(sym.ToString());
                                GenUtils.GenerateSymbol(sym, null, map, UI.MouseCell(), map.ParentFaction, null);
                            }
                        }));
                    }
                }
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
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

        [DebugAction("KCSG", "Destroy all pawns", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveAllPawns()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
            {
                pawn.Destroy(DestroyMode.KillFinalize);
            }
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
            List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();
            var rocks = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.category == ThingCategory.Building && t.building.isNaturalRock);
            foreach (var rock in rocks)
            {
                var name = rock.LabelCap;
                debugMenuOptionList.Add(new DebugMenuOption($"{name} 1*1", DebugMenuOptionMode.Tool, () =>
                {
                    GenSpawn.Spawn(rock, UI.MouseCell(), Find.CurrentMap);
                }));
                debugMenuOptionList.Add(new DebugMenuOption($"{name} 3*3", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 cell in CellRect.CenteredOn(UI.MouseCell(), 1).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(rock, cell, Find.CurrentMap);
                    }
                }));
                debugMenuOptionList.Add(new DebugMenuOption($"{name} 5*5", DebugMenuOptionMode.Tool, () =>
                {
                    foreach (IntVec3 cell in CellRect.CenteredOn(UI.MouseCell(), 2).ClipInsideMap(Find.CurrentMap))
                    {
                        GenSpawn.Spawn(rock, cell, Find.CurrentMap);
                    }
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
        }
    }
}