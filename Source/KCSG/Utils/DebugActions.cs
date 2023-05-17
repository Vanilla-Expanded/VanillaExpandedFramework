using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    public static class DebugActions
    {
        [DebugAction("KCSG", "Quickspawn...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Quickspawn()
        {
            var mainList = new List<DebugMenuOption>();

            // Quickspawn structure
            if (DefDatabase<StructureLayoutDef>.AllDefsListForReading is List<StructureLayoutDef> slDefs && !slDefs.NullOrEmpty())
            {
                mainList.Add(new DebugMenuOption("Structure...", DebugMenuOptionMode.Action, () =>
                {
                    List<DebugMenuOption> list = new List<DebugMenuOption>();
                    foreach (var layoutDef in slDefs)
                    {
                        list.Add(new DebugMenuOption(layoutDef.defName, DebugMenuOptionMode.Tool, () =>
                        {
                            var map = Find.CurrentMap;
                            if (UI.MouseCell().InBounds(map))
                            {
                                var cellRect = CellRect.CenteredOn(UI.MouseCell(), layoutDef.size, layoutDef.size);
                                GenOption.GetAllMineableIn(cellRect, map);
                                GenUtils.PreClean(layoutDef, map, cellRect, true);
                                GenUtils.GenerateLayout(layoutDef, cellRect, map);
                            }
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }));
            }
            // Quickspawn temp structure
            if (Dialog_ExportWindow.exportedLayouts.Count > 0)
            {
                mainList.Add(new DebugMenuOption("Temp structure...", DebugMenuOptionMode.Action, () =>
                {
                    var list = new List<DebugMenuOption>();
                    foreach (var pair in Dialog_ExportWindow.exportedLayouts)
                    {
                        var layoutDef = pair.Value;
                        list.Add(new DebugMenuOption(layoutDef.defName, DebugMenuOptionMode.Tool, () =>
                        {
                            var map = Find.CurrentMap;
                            if (UI.MouseCell().InBounds(map))
                            {
                                var cellRect = CellRect.CenteredOn(UI.MouseCell(), layoutDef.size, layoutDef.size);
                                GenOption.GetAllMineableIn(cellRect, map);
                                GenUtils.PreClean(layoutDef, map, cellRect, true);
                                GenUtils.GenerateLayout(layoutDef, cellRect, map);
                            }
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }));
            }
            // Quickspawn symbol
            if (DefDatabase<SymbolDef>.AllDefsListForReading is List<SymbolDef> syDefs && !syDefs.NullOrEmpty())
            {
                mainList.Add(new DebugMenuOption("Symbol...", DebugMenuOptionMode.Action, () =>
                {
                    List<DebugMenuOption> list = new List<DebugMenuOption>();
                    foreach (var sym in syDefs)
                    {
                        if (sym.modContentPack != null && sym.modContentPack.Name != null)
                        {
                            list.Add(new DebugMenuOption(sym.defName, DebugMenuOptionMode.Tool, () =>
                            {
                                var map = Find.CurrentMap;
                                if (UI.MouseCell().InBounds(map))
                                {
                                    GenUtils.GenerateSymbol(sym, null, map, UI.MouseCell(), map.ParentFaction, null);
                                }
                            }));
                        }
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }));
            }
            // Quickspawn temp symbol
            if (Dialog_ExportWindow.exportedSymbolsDef.Count > 0)
            {
                mainList.Add(new DebugMenuOption("Temp symbol...", DebugMenuOptionMode.Action, () =>
                {
                    var list = new List<DebugMenuOption>();
                    foreach (var sym in Dialog_ExportWindow.exportedSymbolsDef)
                    {
                        if (sym.modContentPack != null && sym.modContentPack.Name != null)
                        {
                            list.Add(new DebugMenuOption(sym.defName, DebugMenuOptionMode.Tool, () =>
                            {
                                var map = Find.CurrentMap;
                                if (UI.MouseCell().InBounds(map))
                                {
                                    GenUtils.GenerateSymbol(sym, null, map, UI.MouseCell(), map.ParentFaction, null);
                                }
                            }));
                        }
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }));
            }
            // Quickspawn settlement
            if (DefDatabase<SettlementLayoutDef>.AllDefsListForReading is List<SettlementLayoutDef> seDefs && !seDefs.NullOrEmpty())
            {
                mainList.Add(new DebugMenuOption("Settement (rect)", DebugMenuOptionMode.Action, () =>
                {
                    var list = new List<DebugMenuOption>();
                    foreach (SettlementLayoutDef def in seDefs)
                    {
                        list.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Tool, () =>
                        {
                            DebugToolsGeneral.GenericRectTool(def.defName, (CellRect rect) =>
                            {
                                var map = Find.CurrentMap;
                                BaseGen.globalSettings.map = map;
                                var rp = new ResolveParams
                                {
                                    faction = map.ParentFaction,
                                    rect = rect
                                };
                                GenOption.settlementLayout = def;
                                GenOption.GetAllMineableIn(rect, map);
                                SettlementGenUtils.Generate(rp, map, def);
                            });
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(mainList));
        }

        [DebugAction("KCSG", "Destroy all hostile pawns", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveAllHostilePawns()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn.Faction != Faction.OfPlayer) pawn.Destroy(DestroyMode.KillFinalize);
            }
        }

        [DebugAction("KCSG", "Destroy all pawns", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveAllPawns()
        {
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
            {
                pawn.Destroy(DestroyMode.KillFinalize);
            }
        }

        [DebugAction("KCSG", "Spawn rocks...", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnRocks()
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