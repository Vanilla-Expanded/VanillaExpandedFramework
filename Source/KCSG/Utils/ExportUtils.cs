using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public class ExportUtils
    {
        /// <summary>
        /// All blacklisted things
        /// </summary>
        private static readonly List<string> symbolBlacklist = new List<string> { "KCSG_PowerConduit", "MVCF_Dummy" };

        /// <summary>
        /// Return a struct def coresponding to area exported
        /// </summary>
        public static StructureLayoutDef CreateStructureDef(Map map, Area area)
        {
            var sld = new StructureLayoutDef
            {
                defName = Dialog_ExportWindow.defName,
                isStorage = Dialog_ExportWindow.isStorage,
                spawnConduits = Dialog_ExportWindow.spawnConduits,
                forceGenerateRoof = Dialog_ExportWindow.forceGenerateRoof,
                needRoofClearance = Dialog_ExportWindow.needRoofClearance,
                randomizeWallStuffAtGen = Dialog_ExportWindow.randomizeWallStuffAtGen,
                tags = Dialog_ExportWindow.tags.ToList(),
                terrainGrid = CreateTerrainlayout(area, map),
                terrainColorGrid = CreateTerrainColorLayout(area, map),
                roofGrid = CreateRoofGrid(area, map),
                modRequirements = GetNeededMods(),
                spawnAt = new List<IntVec2>(),
                _layouts = new List<SymbolDef[,]>()
            };

            int numOfLayout = GetMaxThings();
            for (int i = 0; i < numOfLayout; i++)
            {
                sld.layouts.Add(CreateIndexLayout(area, i));
            }
            sld.ResolveLayouts();

            return sld;
        }

        /// <summary>
        /// Create layout for things
        /// </summary>
        private static List<string> CreateIndexLayout(Area area, int index)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;

            List<Thing> addedThings = new List<Thing>();

            IntVec3 cell = cellExport.First();
            // For each row
            for (int z = 0; z < hw.z; z++)
            {
                string temp = "";
                // For each cell of the row
                for (int x = 0; x < hw.x; x++)
                {
                    // Get the thing on the cell
                    List<Thing> things = pairsCellThingList.TryGetValue(cell);
                    // Remove motes & blacklisted
                    things.RemoveAll(t => t.def.category == ThingCategory.Mote || symbolBlacklist.Contains(t.def.defName));
                    // Remove filth if needed
                    if (!Dialog_ExportWindow.exportFilth)
                        things.RemoveAll(t => t.def.category == ThingCategory.Filth);
                    // Remove plant if needed
                    if (!Dialog_ExportWindow.exportPlant)
                        things.RemoveAll(t => t.def.category == ThingCategory.Plant);

                    // Shouldn't be exported
                    if (things.Count < index + 1 || (area != null && !active.Contains(cell)))
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else
                    {
                        Thing thing = things[index];
                        // It's a pawn
                        if (thing is Pawn pawn && pawn != null)
                        {
                            SymbolDef symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.pawnKindDefNS == pawn.kindDef);
                            if (symbolDef == null)
                            {
                                AddToString(ref temp, pawn.kindDef.defName, x, hw.x);
                            }
                            else
                            {
                                AddToString(ref temp, symbolDef.defName, x, hw.x);
                            }
                        }
                        // It's an item
                        else if (thing.def.category == ThingCategory.Item)
                        {
                            SymbolDef symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == things.First().def && s.thingDef.category == ThingCategory.Item);
                            if (symbolDef == null)
                            {
                                AddToString(ref temp, thing.def.defName, x, hw.x);
                            }
                            else
                            {
                                AddToString(ref temp, symbolDef.defName, x, hw.x);
                            }
                        }
                        // It's something else
                        // Make sure we don't add big building multiple times/add them on the right cell
                        else if (!symbolBlacklist.Contains(thing.def.defName) && !addedThings.Contains(thing) && thing.Position == cell)
                        {
                            var allSymbols = DefDatabase<SymbolDef>.AllDefsListForReading;

                            SymbolDef symbolDef = null;
                            // Get stuff
                            ThingDef stuff = thing.Stuff;
                            // Get color
                            ColorDef colorDef = null;
                            if (thing is Building building && building.PaintColorDef is ColorDef c)
                                colorDef = c;
                            // Get style
                            StyleCategoryDef styleCategoryDef = null;
                            if (thing.StyleDef is ThingStyleDef styleDef)
                                styleCategoryDef = styleDef.Category;

                            for (int i = 0; i < allSymbols.Count; i++)
                            {
                                var symb = allSymbols[i];
                                if (symb.thingDef != thing.def)
                                    continue;
                                if (thing.def.rotatable && symb.rotation != thing.Rotation)
                                    continue;
                                if (stuff != null && symb.stuffDef != stuff)
                                    continue;
                                if (colorDef != null && symb.colorDef != colorDef)
                                    continue;
                                if (styleCategoryDef != null && symb.styleCategoryDef != styleCategoryDef)
                                    continue;

                                symbolDef = symb;
                            }

                            if (symbolDef == null)
                            {
                                AddToString(ref temp, GetSymbolNameFor(thing), x, hw.x);
                            }
                            else
                            {
                                AddToString(ref temp, symbolDef.defName, x, hw.x);
                            }
                            // Add to treated things
                            addedThings.Add(thing);
                        }
                        // We added it, skip
                        else
                        {
                            AddToString(ref temp, ".", x, hw.x);
                        }
                    }

                    cell.x++;
                }

                cell.x -= hw.x;
                cell.z++;

                ll.Add(temp);
            }

            return ll;
        }

        /// <summary>
        /// Add a string to a string, add comma if necessary
        /// </summary>
        private static void AddToString(ref string str, string add, int x, int rx)
        {
            if (x + 1 == rx)
            {
                str += add;
            }
            else
            {
                str += add;
                str += ",";
            }
        }

        /// <summary>
        /// Create layout for terrains
        /// </summary>
        private static List<string> CreateTerrainlayout(Area area, Map map)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;
            var add = false;

            IntVec3 cell = cellExport.First();
            for (int z = 0; z < hw.z; z++)
            {
                string temp = "";
                for (int x = 0; x < hw.x; x++)
                {
                    TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                    // Shouldn't be exported
                    if (area != null && !active.Contains(cell))
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else if (!Dialog_ExportWindow.exportNatural && terrain.natural)
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else
                    {
                        AddToString(ref temp, terrain.defName, x, hw.x);
                        add = true;
                    }

                    cell.x++;
                }

                cell.x -= hw.x;
                cell.z++;

                ll.Add(temp);
            }

            if (add)
                return ll;

            return new List<string>();
        }

        /// <summary>
        /// Create layout for terrains color
        /// </summary>
        private static List<string> CreateTerrainColorLayout(Area area, Map map)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;
            var add = false;

            var cell = cellExport.First();
            for (int z = 0; z < hw.z; z++)
            {
                string temp = "";
                for (int x = 0; x < hw.x; x++)
                {
                    var color = map.terrainGrid.ColorAt(cell);
                    if (color == null || (area != null && !active.Contains(cell)))
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else
                    {
                        AddToString(ref temp, color.defName, x, hw.x);
                        add = true;
                    }

                    cell.x++;
                }

                cell.x -= hw.x;
                cell.z++;

                ll.Add(temp);
            }

            if (add)
                return ll;

            return new List<string>();
        }

        /// <summary>
        /// Create roof grid
        /// </summary>
        private static List<string> CreateRoofGrid(Area area, Map map)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;
            var add = false;

            IntVec3 cell = cellExport.First();
            for (int z = 0; z < hw.z; z++)
            {
                string temp = "";
                for (int x = 0; x < hw.x; x++)
                {
                    if (area != null && !active.Contains(cell))
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }
                    else if (cell.GetRoof(map) is RoofDef roofDef && roofDef != null)
                    {
                        var roofType = roofDef == RoofDefOf.RoofRockThick ? "3" : (roofDef == RoofDefOf.RoofRockThin ? "2" : "1");
                        AddToString(ref temp, roofType, x, hw.x);
                        add = true;
                    }
                    else
                    {
                        AddToString(ref temp, ".", x, hw.x);
                    }

                    cell.x++;
                }

                cell.x -= hw.x;
                cell.z++;

                ll.Add(temp);
            }

            return add ? ll : new List<string>();
        }

        /// <summary>
        /// Create cache dic for things on area cells
        /// </summary>
        public static Dictionary<IntVec3, List<Thing>> FillCellThingsList(Map map)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var list = new Dictionary<IntVec3, List<Thing>>();
            for (int i = 0; i < cellExport.Count; i++)
            {
                var cell = cellExport[i];
                list.Add(cell, cell.GetThingList(map).ToList());
            }
            return list;
        }

        /// <summary>
        /// Make an Area into a square
        /// </summary>
        public static List<IntVec3> AreaToSquare(Area a)
        {
            List<IntVec3> list = a.ActiveCells.ToList();
            MinMaxXZ(list, out int zMin, out int zMax, out int xMin, out int xMax);

            List<IntVec3> listOut = new List<IntVec3>();

            for (int zI = zMin; zI <= zMax; zI++)
            {
                for (int xI = xMin; xI <= xMax; xI++)
                {
                    listOut.Add(new IntVec3(xI, 0, zI));
                }
            }
            listOut.Sort((x, y) => x.z.CompareTo(y.z));
            return listOut;
        }

        /// <summary>
        /// Get smallest X and Z value out of a list
        /// </summary>
        private static void MinMaxXZ(List<IntVec3> list, out int zMin, out int zMax, out int xMin, out int xMax)
        {
            zMin = list[0].z;
            zMax = 0;
            xMin = list[0].x;
            xMax = 0;

            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c.z < zMin) zMin = c.z;
                if (c.z > zMax) zMax = c.z;
                if (c.x < xMin) xMin = c.x;
                if (c.x > xMax) xMax = c.x;
            }
        }

        /// <summary>
        /// Get height/width from list
        /// </summary>
        private static IntVec2 EdgeFromList(List<IntVec3> cellExport)
        {
            var vec = new IntVec2();
            IntVec3 first = cellExport[0];

            for (int i = 0; i < cellExport.Count; i++)
            {
                var cell = cellExport[i];
                if (first.z == cell.z) vec.x++;
                if (first.x == cell.x) vec.z++;
            }

            return vec;
        }

        /// <summary>
        /// Get the maximum amount of things on one cell of the list
        /// </summary>
        private static int GetMaxThings()
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;
            int max = 1;
            for (int i = 0; i < cellExport.Count; i++)
            {
                var things = pairsCellThingList.TryGetValue(cellExport[i]);
                var count = 0;

                for (int o = 0; o < things.Count; o++)
                {
                    var thing = things[o];
                    if (thing.def.category == ThingCategory.Mote)
                        continue;
                    if (!Dialog_ExportWindow.exportFilth && thing.def.category == ThingCategory.Filth)
                        continue;
                    if (!Dialog_ExportWindow.exportPlant && thing.def.category == ThingCategory.Plant)
                        continue;
                    if (symbolBlacklist.Contains(thing.def.defName))
                        continue;

                    count++;
                }

                if (count > max) max = count;
            }

            return max;
        }

        /// <summary>
        /// Get the needed mods for an export
        /// </summary>
        private static List<string> GetNeededMods()
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;

            var modsId = new HashSet<string>();
            for (int i = 0; i < cellExport.Count; i++)
            {
                var things = pairsCellThingList.TryGetValue(cellExport[i]);
                for (int o = 0; o < things.Count; o++)
                {
                    var packageId = things[o]?.def?.modContentPack?.PackageId;
                    if (packageId != null && packageId != "ludeon.rimworld")
                        modsId.Add(packageId);
                }
            }

            return modsId.ToList();
        }

        /// <summary>
        /// Create needed symbols
        /// </summary>
        public static List<SymbolDef> CreateSymbolIfNeeded(Area area)
        {
            var cellExport = Dialog_ExportWindow.cells;
            var pairsCellThingList = Dialog_ExportWindow.pairsCellThingList;
            var symbols = new List<SymbolDef>();
            var activeCells = area?.ActiveCells;

            foreach (IntVec3 c in cellExport)
            {
                if (activeCells == null || activeCells.Contains(c))
                {
                    List<Thing> things = pairsCellThingList.TryGetValue(c);
                    foreach (Thing t in things)
                    {
                        if (symbolBlacklist.Contains(t.def.defName))
                            continue;

                        if (t is Corpse corpse)
                        {
                            var sym = CreateCorpseSymbol(corpse);
                            if (sym != null)
                                symbols.Add(sym);
                        }
                        else if (t is Pawn pawn)
                        {
                            var sym = CreatePawnSymbol(pawn);
                            if (sym != null)
                                symbols.Add(sym);
                        }
                        else if (t.def.category == ThingCategory.Item)
                        {
                            var sym = CreateItemSymbol(t);
                            if (sym != null)
                                symbols.Add(sym);
                        }
                        else if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant)
                        {
                            var sym = CreateThingSymbol(t);
                            if (sym != null)
                                symbols.Add(sym);
                        }
                    }
                }
            }

            return symbols;
        }

        /// <summary>
        /// Create symbol from item
        /// </summary>
        private static SymbolDef CreateItemSymbol(Thing thing)
        {
            var defName = thing.def.defName;

            if (!Dialog_ExportWindow.exportedSymbolsName.Contains(defName)
                && !DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defName).Any())
            {
                var symbol = new SymbolDef
                {
                    defName = defName,
                    thing = defName
                };

                symbol.ResolveReferences();
                Dialog_ExportWindow.exportedSymbolsName.Add(defName);
                DefDatabase<SymbolDef>.Add(symbol);

                return symbol;
            }

            return null;
        }

        /// <summary>
        /// Create symbol from pawn
        /// </summary>
        private static SymbolDef CreatePawnSymbol(Pawn pawn)
        {
            var defName = pawn.kindDef.defName;

            if (!Dialog_ExportWindow.exportedSymbolsName.Contains(defName)
                && !DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defName).Any())
            {
                var symbol = new SymbolDef
                {
                    defName = defName,
                    pawnKindDef = defName,
                    isSlave = pawn.IsSlave,
                };

                symbol.ResolveReferences();
                Dialog_ExportWindow.exportedSymbolsName.Add(defName);
                DefDatabase<SymbolDef>.Add(symbol);

                return symbol;
            }

            return null;
        }

        /// <summary>
        /// Create symbol from a corpse
        /// </summary>
        private static SymbolDef CreateCorpseSymbol(Corpse corpse)
        {
            var pawn = corpse.InnerPawn;
            var kindName = pawn.kindDef.defName;

            if (!Dialog_ExportWindow.exportedSymbolsName.Contains("Corpse_" + kindName)
                && DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => (s.pawnKindDef == kindName && s.spawnDead) || s.defName == "Corpse_" + kindName).Count == 0)
            {
                var symbol = new SymbolDef
                {
                    defName = "Corpse_" + kindName,
                    pawnKindDef = kindName,
                    isSlave = pawn.IsSlave,
                    spawnDead = true
                };

                var comp = corpse.GetComp<CompRottable>();
                if (comp != null)
                    symbol.spawnRotten = comp.RotProgress == 1f;

                symbol.ResolveReferences();
                Dialog_ExportWindow.exportedSymbolsName.Add(symbol.defName);
                DefDatabase<SymbolDef>.Add(symbol);

                return symbol;
            }

            return null;
        }

        /// <summary>
        /// Create symbol from thing
        /// </summary>
        private static SymbolDef CreateThingSymbol(Thing thing)
        {
            var defName = GetSymbolNameFor(thing);
            if (!Dialog_ExportWindow.exportedSymbolsName.Contains(defName)
                && !DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defName).Any())
            {
                var symbol = new SymbolDef
                {
                    defName = defName,
                    thing = thing.def.defName
                };

                if (thing.Stuff != null)
                    symbol.stuff = thing.Stuff.defName;

                if (thing.def.rotatable && thing.def.category != ThingCategory.Plant && !thing.def.IsFilth)
                    symbol.rotation = thing.Rotation;

                if (thing is Building building && building.PaintColorDef is ColorDef colorDef)
                    symbol.color = colorDef.defName;

                if (thing.StyleDef is ThingStyleDef styleDef)
                    symbol.styleCategory = styleDef.Category.defName;

                symbol.ResolveReferences();
                Dialog_ExportWindow.exportedSymbolsName.Add(defName);
                DefDatabase<SymbolDef>.Add(symbol);

                return symbol;
            }

            return null;
        }

        /// <summary>
        /// Create symbol defName from thing
        /// </summary>
        /// <param name="thing">Thing</param>
        /// <returns>string</returns>
        private static string GetSymbolNameFor(Thing thing)
        {
            string symbolString = thing.def.defName;

            if (thing.Stuff != null)
                symbolString += "_" + thing.Stuff.defName;

            if (thing is Building building && building.PaintColorDef is ColorDef colorDef)
                symbolString += "_" + colorDef.LabelCap.Replace(" ", "");

            if (thing.StyleDef is ThingStyleDef styleDef)
                symbolString += "_" + styleDef.Category.LabelCap;

            if (thing.def.rotatable && thing.def.category != ThingCategory.Plant)
                symbolString += "_" + StartupActions.Rot4ToStringEnglish(thing.Rotation);

            return symbolString;
        }
    }
}