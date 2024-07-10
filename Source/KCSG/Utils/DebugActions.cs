using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using LudeonTK;

namespace KCSG
{
    public static class DebugActions
    {
        [DebugAction("KCSG", "Quickspawn structure", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        private static List<DebugActionNode> SpawnStructure()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();

            if (DefDatabase<StructureLayoutDef>.AllDefsListForReading is List<StructureLayoutDef> slDefs && !slDefs.NullOrEmpty())
            {
               
                    
                    foreach (var layoutDef in slDefs)
                    {
                        list.Add(new DebugActionNode(layoutDef.defName, DebugActionType.ToolMap, () =>
                        {
                            var map = Find.CurrentMap;
                            if (UI.MouseCell().InBounds(map))
                            {
                                var cellRect = CellRect.CenteredOn(UI.MouseCell(), layoutDef.sizes.x, layoutDef.sizes.z);
                                GenOption.GetAllMineableIn(cellRect, map);
                                LayoutUtils.CleanRect(layoutDef, map, cellRect, true);
                                layoutDef.Generate(cellRect, map);
                            }
                        }));
                    }
                   
                
            }

            return list;
        }


        [DebugAction("KCSG", "Quickspawn...", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Quickspawn()
        {
            var mainList = new List<DebugMenuOption>();

           
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
                                var cellRect = CellRect.CenteredOn(UI.MouseCell(), layoutDef.sizes.x, layoutDef.sizes.z);
                                GenOption.GetAllMineableIn(cellRect, map);
                                LayoutUtils.CleanRect(layoutDef, map, cellRect, true);
                                layoutDef.Generate(cellRect, map);
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
                                    sym.Generate(null, map, UI.MouseCell(), map.ParentFaction, null);
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
                                    sym.Generate(null, map, UI.MouseCell(), map.ParentFaction, null);
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
            // Quickspawn tiled structure
            if (DefDatabase<TiledStructureDef>.AllDefsListForReading is List<TiledStructureDef> tsDefs && !tsDefs.NullOrEmpty())
            {
                mainList.Add(new DebugMenuOption("Tiled structure...", DebugMenuOptionMode.Action, () =>
                {
                    List<DebugMenuOption> list = new List<DebugMenuOption>();
                    foreach (var def in tsDefs)
                    {
                        list.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Tool, () =>
                        {
                            var map = Find.CurrentMap;
                            if (UI.MouseCell().InBounds(map))
                            {
                                TileUtils.Generate(def, UI.MouseCell(), map);
                            }
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(mainList));
        }

        [DebugAction("KCSG", "Destroy hostile pawns", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DestroyHostilePawns()
        {
            var pawns = Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList();
            pawns.RemoveAll(p => p.Faction == null || p.Faction == Faction.OfPlayer || !p.Faction.HostileTo(Faction.OfPlayer));

            foreach (var pawn in pawns)
                pawn.Destroy(DestroyMode.KillFinalize);
        }

        [DebugAction("KCSG", "Destroy all pawns", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveAllPawns()
        {
            foreach (var pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.ToList())
                pawn.Destroy(DestroyMode.KillFinalize);
        }

        [DebugAction("KCSG", "Spawn rocks (rect)", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnRocks()
        {
            var list = new List<DebugMenuOption>();
            var rocks = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.category == ThingCategory.Building && t.building.isNaturalRock);
            var map = Find.CurrentMap;

            foreach (var rock in rocks)
            {
                list.Add(new DebugMenuOption(rock.LabelCap, DebugMenuOptionMode.Action, () =>
                {
                    DebugToolsGeneral.GenericRectTool(rock.LabelCap, rect =>
                    {
                        foreach (var cell in rect)
                        {
                            GenSpawn.Spawn(rock, cell, map);
                        }
                    });
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }
}