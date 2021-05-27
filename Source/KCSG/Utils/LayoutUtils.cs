using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Verse;

namespace KCSG
{
    public class LayoutUtils
    {
        public static XElement CreateStructureDef(List<IntVec3> cellExport, Map map, string defNamePrefix, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area)
        {
            cellExport.Sort((x, y) => x.z.CompareTo(y.z));
            XElement StructureLayoutDef = new XElement("KCSG.StructureLayoutDef", null);

            XElement layouts = new XElement("layouts", null);
            // Add pawns layout
            XElement pawnsL = Createpawnlayout(cellExport, defNamePrefix, area, out bool add, pairsSymbolLabel, pairsCellThingList);
            if (add) layouts.Add(pawnsL);
            // Add items layout
            XElement itemsL = CreateItemlayout(cellExport, defNamePrefix, area, out bool add2, pairsSymbolLabel, pairsCellThingList);
            if (add2) layouts.Add(itemsL);
            // Add terrain layout
            XElement terrainL = CreateTerrainlayout(cellExport, defNamePrefix, area, map, pairsSymbolLabel, out bool add3);
            if (add3) layouts.Add(terrainL);
            // Add things layouts
            int numOfLayout = RectUtils.GetMaxThingOnOneCell(cellExport, pairsCellThingList);
            for (int i = 0; i < numOfLayout; i++)
            {
                layouts.Add(CreateThinglayout(cellExport, defNamePrefix, i, area, pairsSymbolLabel, pairsCellThingList));
            }

            StructureLayoutDef.Add(layouts);

            // Add roofGrid
            XElement roofGrid = CreateRoofGrid(cellExport, map, out bool add4, area);
            if (add4) StructureLayoutDef.Add(roofGrid);

            return StructureLayoutDef;
        }

        public static XElement CreateThinglayout(List<IntVec3> cellExport, string defNamePrefix, int index, Area area, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            XElement liMain = new XElement("li", null);
            RectUtils.EdgeFromList(cellExport, out int height, out int width);
            List<Thing> aAdded = new List<Thing>();

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    List<Thing> things = pairsCellThingList.TryGetValue(first).FindAll(t => t.def.category != ThingCategory.Pawn || t.def.category != ThingCategory.Item || t.def.category != ThingCategory.Filth || t.def.defName != "PowerConduit");
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
                            if (thing.Stuff != null && thing.def.rotatable) symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == thing.def && s.stuffDef == thing.Stuff && s.rotation == thing.Rotation);
                            else if (thing.Stuff != null && !thing.def.rotatable) symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == thing.def && s.stuffDef == thing.Stuff);
                            else symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == thing.def);

                            if (symbolDef == null)
                            {
                                string symbolString = defNamePrefix + "_" + thing.def.defName;
                                if (thing.Stuff != null) symbolString += "_" + thing.Stuff.defName;
                                if (thing.def.rotatable && thing.def.category != ThingCategory.Plant) symbolString += "_" + thing.Rotation.ToStringHuman();

                                if (i2 + 1 == width) temp += symbolString;
                                else temp += symbolString + ",";
                            }
                            else
                            {
                                if (i2 + 1 == width) temp += symbolDef.symbol;
                                else temp += symbolDef.symbol + ",";
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

        public static XElement CreateTerrainlayout(List<IntVec3> cellExport, string defNamePrefix, Area area, Map map, Dictionary<string, SymbolDef> pairsSymbolLabel, out bool add)
        {
            XElement liMain = new XElement("li", null);
            RectUtils.EdgeFromList(cellExport, out int height, out int width);
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
                    else if (!map.terrainGrid.TerrainAt(first).BuildableByPlayer)
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        // Find corresponding symbol
                        TerrainDef terrainD = map.terrainGrid.TerrainAt(first);
                        if (pairsSymbolLabel != null && pairsSymbolLabel.Values.ToList().Find(s => s.isTerrain && s.terrainDef.defName == terrainD.defName) is SymbolDef symbolDef && symbolDef != null)
                        {
                            add = true;
                            if (i2 + 1 == width) temp += symbolDef.symbol;
                            else temp += symbolDef.symbol + ",";
                        }
                        else if (terrainD != null)
                        {
                            add = true;
                            if (i2 + 1 == width) temp += defNamePrefix + "_" + terrainD.defName;
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

        public static XElement CreateRoofGrid(List<IntVec3> cellExport, Map map, out bool add, Area area)
        {
            XElement roofGrid = new XElement("roofGrid", null);
            RectUtils.EdgeFromList(cellExport, out int height, out int width);
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
                    else if (first.Roofed(map))
                    {
                        add = true;
                        if (i2 + 1 == width) temp += "1";
                        else temp += "1,";
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

        public static XElement Createpawnlayout(List<IntVec3> cellExport, string defNamePrefix, Area area, out bool add, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            XElement liMain = new XElement("li", null);
            RectUtils.EdgeFromList(cellExport, out int height, out int width);
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
                            SymbolDef symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.pawnKindDefNS == pawn.kindDef);
                            if (symbolDef == null)
                            {
                                string symbolString = defNamePrefix + "_" + pawn.kindDef.defName;

                                if (i2 + 1 == width) temp += symbolString;
                                else temp += symbolString + ",";
                            }
                            else
                            {
                                if (i2 + 1 == width) temp += symbolDef.symbol;
                                else temp += symbolDef.symbol + ",";
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

        public static XElement CreateItemlayout(List<IntVec3> cellExport, string defNamePrefix, Area area, out bool add, Dictionary<string, SymbolDef> pairsSymbolLabel, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            add = false;
            XElement liMain = new XElement("li", null);
            RectUtils.EdgeFromList(cellExport, out int height, out int width);

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
                        SymbolDef symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == things.First().def && s.isItem);
                        if (symbolDef == null)
                        {
                            string symbolString = defNamePrefix + "_Item_" + things.First().def.defName;

                            if (i2 + 1 == width) temp += symbolString;
                            else temp += symbolString + ",";
                        }
                        else
                        {
                            if (i2 + 1 == width) temp += symbolDef.symbol;
                            else temp += symbolDef.symbol + ",";
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

        public static void FillCellThingsList(List<IntVec3> cellExport, Map map, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            pairsCellThingList.Clear();
            foreach (IntVec3 intVec in cellExport)
            {
                pairsCellThingList.Add(intVec, intVec.GetThingList(map).ToList());
            }
        }

        public static Dictionary<string, SymbolDef> FillpairsSymbolLabel()
        {
            Dictionary<string, SymbolDef> pairsSymbolLabel = new Dictionary<string, SymbolDef>();
            List<SymbolDef> symbolDefs = DefDatabase<SymbolDef>.AllDefsListForReading;
            foreach (SymbolDef s in symbolDefs)
            {
                pairsSymbolLabel.Add(s.symbol, s);
            }
            return pairsSymbolLabel;
        }
    }
}