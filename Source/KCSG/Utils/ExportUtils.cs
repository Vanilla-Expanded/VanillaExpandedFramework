using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public class ExportUtils
    {
        /// <summary>
        /// Return a struct def coresponding to area exported
        /// </summary>
        public static StructureLayoutDef CreateStructureDef(List<IntVec3> cellExport, Map map, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area)
        {
            var sld = new StructureLayoutDef();

            sld.defName = Dialog_ExportWindow.defName;
            sld.isStorage = Dialog_ExportWindow.isStorage;
            sld.spawnConduits = Dialog_ExportWindow.spawnConduits;
            sld.forceGenerateRoof = Dialog_ExportWindow.forceGenerateRoof;
            sld.needRoofClearance = Dialog_ExportWindow.needRoofClearance;
            sld.tags = Dialog_ExportWindow.tags.ToList();
            sld.terrainGrid = CreateTerrainlayout(cellExport, area, map);
            sld.roofGrid = CreateRoofGrid(cellExport, area, map);
            sld.modRequirements = GetNeededMods(cellExport, pairsCellThingList);

            int numOfLayout = GetMaxThings(cellExport, pairsCellThingList);
            for (int i = 0; i < numOfLayout; i++)
            {
                sld.layouts.Add(CreateIndexLayout(cellExport, pairsCellThingList, area, i));
            }

            sld.spawnAtPos = new List<Pos>();
            sld.spawnAt = new List<string>();
            sld.symbolsLists = new List<List<SymbolDef>>();
            sld.terrainGridResolved = new List<TerrainDef>();
            sld.roofGridResolved = new List<string>();
            sld.ResolveLayouts();

            return sld;
        }

        /// <summary>
        /// Create layout for things
        /// </summary>
        private static List<string> CreateIndexLayout(List<IntVec3> cellExport, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area, int index)
        {
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
                        else if (!addedThings.Contains(thing) && thing.Position == cell)
                        {
                            SymbolDef symbolDef;
                            if (thing.Stuff != null)
                            {
                                symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def
                                                                                                   && s.stuffDef == thing.Stuff
                                                                                                   && (thing.def.rotatable == false || s.rotation == thing.Rotation));
                            }
                            else
                            {
                                symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def
                                                                                                   && (thing.def.rotatable == false || s.rotation == thing.Rotation));
                            }

                            if (symbolDef == null)
                            {
                                string symbolString = thing.def.defName;
                                if (thing.Stuff != null) symbolString += "_" + thing.Stuff.defName;
                                if (thing.def.rotatable && thing.def.category != ThingCategory.Plant) symbolString += "_" + StartupActions.Rot4ToStringEnglish(thing.Rotation);

                                AddToString(ref temp, symbolString, x, hw.x);
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
        private static List<string> CreateTerrainlayout(List<IntVec3> cellExport, Area area, Map map)
        {
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
                    else if (!terrain.BuildableByPlayer && !terrain.HasTag("Road") && !Dialog_ExportWindow.exportNatural)
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
        /// Create roof grid
        /// </summary>
        private static List<string> CreateRoofGrid(List<IntVec3> cellExport, Area area, Map map)
        {
            var ll = new List<string>();
            var hw = EdgeFromList(cellExport);
            var active = area?.ActiveCells;

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

            return ll;
        }

        /// <summary>
        /// Create needed symbols
        /// </summary>
        public static List<XElement> CreateSymbolIfNeeded(List<IntVec3> cellExport, Map map, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area = null)
        {
            List<XElement> symbols = new List<XElement>();
            var activeCells = area?.ActiveCells;

            foreach (IntVec3 c in cellExport)
            {
                if (activeCells != null && activeCells.Contains(c))
                {
                    List<Thing> things = pairsCellThingList.TryGetValue(c);
                    foreach (Thing t in things)
                    {
                        if (t.def.category == ThingCategory.Item) CreateItemSymbolFromThing(t, symbols);
                        else if (t.def.category == ThingCategory.Pawn) CreateSymbolFromPawn(t as Pawn, symbols);
                        else if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant) CreateSymbolFromThing(t, symbols);
                    }
                }
            }

            return symbols;
        }

        /// <summary>
        /// Create cache dic for things on area cells
        /// </summary>
        public static Dictionary<IntVec3, List<Thing>> FillCellThingsList(List<IntVec3> cellExport, Map map)
        {
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
        private static int GetMaxThings(List<IntVec3> cellExport, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            int max = 1;
            for (int i = 0; i < cellExport.Count; i++)
            {
                var things = pairsCellThingList.TryGetValue(cellExport[i]);
                var count = 0;

                for (int o = 0; o < things.Count; o++)
                {
                    var thing = things[o];
                    if (!Dialog_ExportWindow.exportFilth && thing.def.category == ThingCategory.Filth)
                        continue;
                    if (!Dialog_ExportWindow.exportPlant && thing.def.category == ThingCategory.Plant)
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
        private static List<string> GetNeededMods(List<IntVec3> cellExport, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            var modsId = new HashSet<string>();
            for (int i = 0; i < cellExport.Count; i++)
            {
                var things = pairsCellThingList.TryGetValue(cellExport[i]);
                for (int o = 0; o < things.Count; o++)
                {
                    var packageId = things[o].def.modContentPack.PackageId;
                    if (packageId != "ludeon.rimworld")
                        modsId.Add(packageId);
                }
            }

            return modsId.ToList();
        }

        /// <summary>
        /// Create symbol from item
        /// </summary>
        private static void CreateItemSymbolFromThing(Thing thingT, List<XElement> symbols)
        {
            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.thingDef == thingT.def).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", thingT.def.defName));
                symbolDef.Add(new XElement("thing", thingT.def.defName));

                if (!symbols.Any(s => s.Value == symbolDef.Value))
                {
                    symbols.Add(symbolDef);
                }
            }
        }

        /// <summary>
        /// Create symbol from pawn
        /// </summary>
        private static void CreateSymbolFromPawn(Pawn pawn, List<XElement> symbols)
        {
            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.pawnKindDefNS == pawn.kindDef).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", pawn.kindDef.defName));
                symbolDef.Add(new XElement("pawnKindDef", pawn.kindDef.defName));

                if (!symbols.Any(s => s.Value == symbolDef.Value))
                {
                    symbols.Add(symbolDef);
                }
            }
        }

        /// <summary>
        /// Create symbol from thing
        /// </summary>
        private static void CreateSymbolFromThing(Thing thingT, List<XElement> symbols)
        {
            // Generate defName
            string defNameString = thingT.def.defName;
            if (thingT.Stuff != null) defNameString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant && !thingT.def.IsFilth) defNameString += "_" + StartupActions.Rot4ToStringEnglish(thingT.Rotation);

            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defNameString).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", defNameString)); // defName
                symbolDef.Add(new XElement("thing", thingT.def.defName)); // thing defName
                if (thingT.Stuff != null)
                    symbolDef.Add(new XElement("stuff", thingT.Stuff.defName)); // Add stuff
                if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant)
                    symbolDef.Add(new XElement("rotation", StartupActions.Rot4ToStringEnglish(thingT.Rotation))); // Add rotation

                if (!symbols.Any(s => s.Value == symbolDef.Value))
                {
                    symbols.Add(symbolDef);
                }
            }
        }
    }
}