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
        public static XElement CreateStructureDef(List<IntVec3> cellExport, Map map, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area, bool exportFilth, bool exportNatural)
        {
            cellExport.Sort((x, y) => x.z.CompareTo(y.z));
            XElement StructureLayoutDef = new XElement("KCSG.StructureLayoutDef", null);

            XElement layouts = new XElement("layouts", null);
            // Add pawns layout
            XElement pawnsL = CreatePawnlayout(cellExport, area, out bool add, pairsCellThingList);
            if (add) layouts.Add(pawnsL);
            // Add items layout
            XElement itemsL = CreateItemlayout(cellExport, area, out bool add2, pairsCellThingList);
            if (add2) layouts.Add(itemsL);
            // Add terrain layout
            XElement terrainL = CreateTerrainlayout(cellExport, area, map, exportNatural, out bool add3);
            if (add3) layouts.Add(terrainL);
            // Add things layouts
            int numOfLayout = GetMaxThingOnOneCell(cellExport, pairsCellThingList, exportFilth);
            for (int i = 0; i < numOfLayout; i++)
            {
                layouts.Add(CreateThinglayout(cellExport, i, area, pairsCellThingList, exportFilth));
            }

            StructureLayoutDef.Add(layouts);

            // Add roofGrid
            XElement roofGrid = CreateRoofGrid(cellExport, map, out bool add4, area);
            if (add4) StructureLayoutDef.Add(roofGrid);

            return StructureLayoutDef;
        }

        /// <summary>
        /// Create layout for things
        /// </summary>
        public static XElement CreateThinglayout(List<IntVec3> cellExport, int index, Area area, Dictionary<IntVec3, List<Thing>> pairsCellThingList, bool exportFilth)
        {
            XElement liMain = new XElement("li", null);
            EdgeFromList(cellExport, out int height, out int width);
            List<Thing> aAdded = new List<Thing>();

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    List<Thing> things = pairsCellThingList.TryGetValue(first).FindAll(t => t.def.category != ThingCategory.Pawn && t.def.category != ThingCategory.Item);
                    if (!exportFilth)
                    {
                        things.RemoveAll(t => t.def.category == ThingCategory.Filth);
                    }

                    Thing thing;
                    if (things.Count < index + 1 || (area != null && !area.ActiveCells.Contains(first)))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        thing = things[index];
                        if (!aAdded.Contains(thing) && thing.Position == first)
                        {
                            SymbolDef symbolDef;
                            if (thing.Stuff != null)
                            {
                                if (thing.def.rotatable) symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def && s.stuffDef == thing.Stuff && s.rotation == thing.Rotation);
                                else symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def && s.stuffDef == thing.Stuff);
                            }
                            else
                            {
                                if (thing.def.rotatable) symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def && s.rotation == thing.Rotation);
                                else symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == thing.def);
                            }

                            if (symbolDef == null)
                            {
                                string symbolString = thing.def.defName;
                                if (thing.Stuff != null) symbolString += "_" + thing.Stuff.defName;
                                if (thing.def.rotatable && thing.def.category != ThingCategory.Plant) symbolString += "_" + thing.Rotation.ToStringHuman();

                                if (i2 + 1 == width) temp += symbolString;
                                else temp += symbolString + ",";
                            }
                            else
                            {
                                if (i2 + 1 == width) temp += symbolDef.defName;
                                else temp += symbolDef.defName + ",";
                            }
                            aAdded.Add(thing);
                        }
                        else
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                liMain.Add(li);
                first.z++;
            }
            return liMain;
        }

        /// <summary>
        /// Create layout for terrains
        /// </summary>
        public static XElement CreateTerrainlayout(List<IntVec3> cellExport, Area area, Map map, bool exportNatural, out bool add)
        {
            XElement liMain = new XElement("li", null);
            EdgeFromList(cellExport, out int height, out int width);
            add = false;

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    if (area != null && !area.ActiveCells.Contains(first))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else if (map.terrainGrid.TerrainAt(first) is TerrainDef d && !d.BuildableByPlayer && !d.HasTag("Road") && !exportNatural)
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        // Find corresponding symbol
                        TerrainDef terrainD = map.terrainGrid.TerrainAt(first);
                        if (DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.isTerrain && s.terrainDef.defName == terrainD.defName) is SymbolDef symbolDef && symbolDef != null)
                        {
                            add = true;
                            if (i2 + 1 == width) temp += symbolDef.defName;
                            else temp += symbolDef.defName + ",";
                        }
                        else if (terrainD != null)
                        {
                            add = true;
                            if (i2 + 1 == width) temp += terrainD.defName;
                            else temp += terrainD.defName + ",";
                        }
                        else
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                liMain.Add(li);
                first.z++;
            }
            return liMain;
        }

        /// <summary>
        /// Create roof grid
        /// </summary>
        public static XElement CreateRoofGrid(List<IntVec3> cellExport, Map map, out bool add, Area area)
        {
            XElement roofGrid = new XElement("roofGrid", null);
            EdgeFromList(cellExport, out int height, out int width);
            add = false;

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    if (area != null && !area.ActiveCells.Contains(first))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else if (first.GetRoof(map) is RoofDef roofDef && roofDef != null)
                    {
                        add = true;
                        string roofType = roofDef == RoofDefOf.RoofRockThick ? "3" : (roofDef == RoofDefOf.RoofRockThin ? "2" : "1");
                        if (i2 + 1 == width) temp += roofType;
                        else temp += $"{roofType},";
                    }
                    else
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    first.x++;
                }
                first.x -= width;
                li.Add(temp);
                roofGrid.Add(li);
                first.z++;
            }
            return roofGrid;
        }

        /// <summary>
        /// Create layout for pawns
        /// </summary>
        public static XElement CreatePawnlayout(List<IntVec3> cellExport, Area area, out bool add, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            XElement liMain = new XElement("li", null);
            EdgeFromList(cellExport, out int height, out int width);
            add = false;

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    List<Thing> pawns = pairsCellThingList.TryGetValue(first).FindAll(t => t is Pawn p && p != null).ToList();
                    if (pawns.Count <= 0 || (area != null && !area.ActiveCells.Contains(first)))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        if (pawns.First() is Pawn pawn && pawn != null)
                        {
                            if (!add) add = true;
                            SymbolDef symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.pawnKindDefNS == pawn.kindDef);
                            if (symbolDef == null)
                            {
                                string symbolString = pawn.kindDef.defName;

                                if (i2 + 1 == width) temp += symbolString;
                                else temp += symbolString + ",";
                            }
                            else
                            {
                                if (i2 + 1 == width) temp += symbolDef.defName;
                                else temp += symbolDef.defName + ",";
                            }
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                liMain.Add(new XElement("li", temp));
                first.z++;
            }
            return liMain;
        }

        /// <summary>
        /// Create layout for items
        /// </summary>
        public static XElement CreateItemlayout(List<IntVec3> cellExport, Area area, out bool add, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            add = false;
            XElement liMain = new XElement("li", null);
            EdgeFromList(cellExport, out int height, out int width);

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    List<Thing> things = pairsCellThingList.TryGetValue(first).FindAll(t => t.def.category == ThingCategory.Item && t.def.category != ThingCategory.Filth).ToList();
                    if (things.Count == 0 || (area != null && !area.ActiveCells.Contains(first)))
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        if (!add) add = true;
                        SymbolDef symbolDef = DefDatabase<SymbolDef>.AllDefsListForReading.Find(s => s.thingDef == things.First().def && s.thingDef.category == ThingCategory.Item);
                        if (symbolDef == null)
                        {
                            string symbolString = things.First().def.defName;

                            if (i2 + 1 == width) temp += symbolString;
                            else temp += symbolString + ",";
                        }
                        else
                        {
                            if (i2 + 1 == width) temp += symbolDef.defName;
                            else temp += symbolDef.defName + ",";
                        }
                    }
                    first.x++;
                }
                first.x -= width;
                liMain.Add(new XElement("li", temp));
                first.z++;
            }
            return liMain;
        }

        /// <summary>
        /// Create needed symbols
        /// </summary>
        public static List<XElement> CreateSymbolIfNeeded(List<IntVec3> cellExport, Map map, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area = null)
        {
            List<XElement> symbols = new List<XElement>();

            foreach (IntVec3 c in cellExport)
            {
                if (area != null && !area.ActiveCells.Contains(c)) { }
                else
                {
                    TerrainDef terrainDef = c.GetTerrain(map);
                    if (terrainDef != null && terrainDef.BuildableByPlayer) CreateSymbolFromTerrain(terrainDef, symbols);

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
        public static void MinMaxXZ(List<IntVec3> list, out int zMin, out int zMax, out int xMin, out int xMax)
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
        public static void EdgeFromList(List<IntVec3> cellExport, out int height, out int width)
        {
            height = 0;
            width = 0;
            IntVec3 first = cellExport[0];

            for (int i = 0; i < cellExport.Count; i++)
            {
                var cell = cellExport[i];
                if (first.z == cell.z) width++;
                if (first.x == cell.x) height++;
            }
        }

        /// <summary>
        /// Get the maximum amount of things in one cell in this list
        /// </summary>
        public static int GetMaxThingOnOneCell(List<IntVec3> cellExport, Dictionary<IntVec3, List<Thing>> pairsCellThingList, bool exportFilth)
        {
            int max = 1;
            for (int i = 0; i < cellExport.Count; i++)
            {
                var things = pairsCellThingList.TryGetValue(cellExport[i]);
                var count = 0;

                for (int o = 0; o < things.Count; o++)
                {
                    var thing = things[o];
                    if (thing is Pawn
                        || thing.def.category == ThingCategory.Item
                        || (!exportFilth && thing.def.category == ThingCategory.Filth))
                    {
                        continue;
                    }

                    count++;
                }

                if (count > max) max = count;
            }

            return max;
        }

        /// <summary>
        /// Create symbol from item
        /// </summary>
        public static void CreateItemSymbolFromThing(Thing thingT, List<XElement> symbols)
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
        public static void CreateSymbolFromPawn(Pawn pawn, List<XElement> symbols)
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
        /// Create symbol from terrain
        /// </summary>
        public static void CreateSymbolFromTerrain(TerrainDef terrainD, List<XElement> symbols)
        {
            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.terrainDef == terrainD).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", terrainD.defName));
                symbolDef.Add(new XElement("isTerrain", "true"));
                symbolDef.Add(new XElement("terrain", terrainD.defName));

                if (!symbols.Any(s => s.Value == symbolDef.Value))
                {
                    symbols.Add(symbolDef);
                }
            }
        }

        /// <summary>
        /// Create symbol from thing
        /// </summary>
        public static void CreateSymbolFromThing(Thing thingT, List<XElement> symbols)
        {
            // Generate defName
            string defNameString = thingT.def.defName;
            if (thingT.Stuff != null) defNameString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant && !thingT.def.IsFilth) defNameString += "_" + thingT.Rotation.ToStringHuman();

            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.defName == defNameString).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", defNameString)); // defName
                symbolDef.Add(new XElement("thing", thingT.def.defName)); // thing defName
                if (thingT.Stuff != null)
                    symbolDef.Add(new XElement("stuff", thingT.Stuff.defName)); // Add stuff
                if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant)
                    symbolDef.Add(new XElement("rotation", thingT.Rotation.ToStringHuman())); // Add rotation

                if (!symbols.Any(s => s.Value == symbolDef.Value))
                {
                    symbols.Add(symbolDef);
                }
            }
        }
    }
}