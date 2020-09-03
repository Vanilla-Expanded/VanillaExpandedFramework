using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse;
using UnityEngine;

using System.IO;
using System.Threading;
using HarmonyLib;
using Verse.AI.Group;

namespace KCSG
{
    class MapComponent_KSG : MapComponent
    {
        public MapComponent_KSG(Map map) : base(map)
        {
        }

        public static Dictionary<string, SymbolDef> pairsSymbolLabel = new Dictionary<string, SymbolDef>();
        private List<IntVec3> cellExport = new List<IntVec3>();
        public static bool done = false;
        public static bool exportNaturalTerrain = false;

        public override void MapComponentTick()
        {
            // At save load
            if (!done)
            {
                List<SymbolDef> symbolDefs = DefDatabase<SymbolDef>.AllDefsListForReading;
                foreach (SymbolDef s in symbolDefs)
                {
                    if (!pairsSymbolLabel.ContainsKey(s.symbol)) pairsSymbolLabel.Add(s.symbol, s);
                }
                done = true;
            }
        }

        private bool AlreadyExist(Thing thing, TerrainDef terrain)
        {
            foreach (SymbolDef s in DefDatabase<SymbolDef>.AllDefsListForReading)
            {
                if (thing?.def.altitudeLayer == AltitudeLayer.Filth) return true;
                else if (s.isItem && s.thingDef.defName == thing?.def.defName) return true;
                else if (thing is Pawn p && s.pawnKindDefNS == p.kindDef) return true;
                else if (thing is Plant && s.thingDef == thing.def) return true;
                else if (thing != null)
                {
                    if (s.thingDef == thing.def && s.stuffDef == thing.Stuff)
                    {
                        if (!thing.def.rotatable) return true;
                        else if (thing.def.rotatable && s.rotation == thing.Rotation) return true;
                    }
                }
                else if (s.isTerrain && s.terrainDef == terrain) return true;
            }
            return false;
        }

        private void CreateSymbolFromThing(Thing thingT)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            string defNameString = /*"KCSG_" + */thingT.def.defName;
            if (thingT.Stuff != null) defNameString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant) defNameString += "_" + thingT.Rotation.ToStringHuman();

            XElement defName = new XElement("defName", defNameString);
            symbolDef.Add(defName);
            // Add thing
            XElement thing = new XElement("thing", thingT.def.defName);
            symbolDef.Add(thing);
            // Add stuff
            if (thingT.Stuff != null)
            {
                XElement stuff = new XElement("stuff", thingT.Stuff.defName);
                symbolDef.Add(stuff);
            }
            // Add rotation
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant)
            {
                XElement rotation = new XElement("rotation", thingT.Rotation.ToStringHuman());
                symbolDef.Add(rotation);
            }
            // Plant growth
            if (thingT is Plant plant)
            {
                XElement plantGrowth = new XElement("plantGrowth", plant.Growth.ToString());
                symbolDef.Add(plantGrowth);
            }

            string symbolString = thingT.def.defName;
            if (thingT.Stuff != null) symbolString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant) symbolString += "_" + thingT.Rotation.ToStringHuman();
            XElement symbol = new XElement("symbol", symbolString);
            symbolDef.Add(symbol);

            if (!justCreated.Contains(symbol.Value)) Log.Message(symbolDef.ToString()); justCreated.Add(symbol.Value);
        }

        private void CreateSymbolFromTerrain(TerrainDef terrainD)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            XElement defName = new XElement("defName", terrainD.defName);
            symbolDef.Add(defName);
            // Add isTerrain
            XElement isTerrain = new XElement("isTerrain", "true");
            symbolDef.Add(isTerrain);
            // Add terrain
            XElement terrain = new XElement("terrain", terrainD.defName);
            symbolDef.Add(terrain);
            // Add symbol
            XElement symbol = new XElement("symbol", terrainD.defName);
            symbolDef.Add(symbol);

            if (!justCreated.Contains(symbol.Value)) Log.Message(symbolDef.ToString()); justCreated.Add(symbol.Value);
        }

        private void CreateSymbolFromPawn(Pawn pawn)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            string defNameString = /*"KCSG_" + */pawn.kindDef.defName;
            // defNameString += "_" + pawn.GetLord().GetType().Name;

            XElement defName = new XElement("defName", defNameString);
            symbolDef.Add(defName);
            // Add isPawn
            XElement isPawn = new XElement("isPawn", "true");
            symbolDef.Add(isPawn);
            // Add pawnKindDef
            XElement pawnKindDef = new XElement("pawnKindDef", pawn.kindDef.defName);
            symbolDef.Add(pawnKindDef);
            // Add lordJob
            // XElement lordJob = new XElement("lordJob", null);
            // symbolDef.Add(lordJob);

            string symbolString = pawn.kindDef.defName;
            // symbolString += "_" + pawn.GetLord().GetType().Name;
            XElement symbol = new XElement("symbol", symbolString);
            symbolDef.Add(symbol);

            if (!justCreated.Contains(symbol.Value)) Log.Message(symbolDef.ToString()); justCreated.Add(symbol.Value);
        }

        private void CreateItemSymbolFromThing(Thing thingT)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            string defNameString = "Item_" + thingT.def.defName;

            XElement defName = new XElement("defName", defNameString);
            symbolDef.Add(defName);
            // Add thing
            XElement thing = new XElement("thing", thingT.def.defName);
            symbolDef.Add(thing);
            // Add stuff
            /*if (thingT.Stuff != null)
            {
                XElement stuff = new XElement("stuff", thingT.Stuff.defName);
                symbolDef.Add(stuff);
            }*/
            // Add
            XElement isItem = new XElement("isItem", "true");
            symbolDef.Add(isItem);
            // Add
            IntRange intRange = new IntRange(1, thingT.def.stackLimit);
            XElement stackCount = new XElement("stackCount", intRange.ToString());
            symbolDef.Add(stackCount);
            // Add
            /* if (thingT.TryGetComp<CompQuality>() != null)
            {
                XElement quality = new XElement("quality", thingT.TryGetComp<CompQuality>().Quality.ToString());
                symbolDef.Add(quality);
            }*/

            string symbolString = "Item_" + thingT.def.defName;
            XElement symbol = new XElement("symbol", symbolString);
            symbolDef.Add(symbol);

            if (!justCreated.Contains(symbol.Value)) justCreated.Add(symbol.Value);
        }

        private List<string> justCreated = new List<string>(); // Don't print 2 same symbol

        private void CreateSymbolIfNeeded(List<IntVec3> cellExport, Area area = null)
        {
            justCreated.Clear();
            IntVec3 last;
            foreach (IntVec3 c in cellExport)
            {
                if (area != null && !area.ActiveCells.Contains(c)) { }
                else
                {
                    TerrainDef terrainDef = c.GetTerrain(map);
                    if (!MapComponent_KSG.exportNaturalTerrain && !terrainDef.BuildableByPlayer) { }
                    else if (!this.AlreadyExist(null, terrainDef)) this.CreateSymbolFromTerrain(terrainDef);

                    List<Thing> things = c.GetThingList(this.map);
                    foreach (Thing t in things)
                    {
                        if (!this.AlreadyExist(t, null))
                        {
                            if (t.def.category == ThingCategory.Item) this.CreateItemSymbolFromThing(t);
                            if (t.def.category == ThingCategory.Pawn) this.CreateSymbolFromPawn(t as Pawn);
                            if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant) this.CreateSymbolFromThing(t);
                        }
                    }
                    last = c;
                }
            }
        }

        private void CreateStructureDef(List<IntVec3> cellExport, Area area = null)
        {
            cellExport.Sort((x, y) => x.z.CompareTo(y.z));
            XElement StructureLayoutDef = new XElement("KCSG.StructureLayoutDef", null);
            // Generate defName
            Room room = cellExport[cellExport.Count / 2].GetRoom(map);
            string defNameString = "PlaceHolder"; Log.Warning("Please remember to change the defName of the following room");
            XElement defName = new XElement("defName", defNameString);
            StructureLayoutDef.Add(defName);
            // Roof?
            bool roofOverBool = false;
            if (map.roofGrid.Roofed(cellExport.FindAll(c => c.Walkable(map)).First())) roofOverBool = true;
            XElement roofOver = new XElement("roofOver", roofOverBool.ToString());
            StructureLayoutDef.Add(roofOver);
            // Stockpile?
            bool isStockpileBool = false;
            if (map.zoneManager.ZoneAt(cellExport.FindAll(c => c.Walkable(map)).First()) is Zone_Stockpile) isStockpileBool = true;
            XElement isStockpile = new XElement("isStockpile", isStockpileBool.ToString());
            StructureLayoutDef.Add(isStockpile);
            // allowedThingsInStockpile
            XElement allowedThingsInStockpile = new XElement("allowedThingsInStockpile", null);
            if (isStockpileBool)
            {
                foreach (Thing item in map.zoneManager.ZoneAt(cellExport.FindAll(c => c.Walkable(map)).First()).AllContainedThings)
                {
                    XElement li = new XElement("li", item.def.defName);
                    if (item is Pawn || item is Filth || item.def.building != null) { }
                    else if (allowedThingsInStockpile.Elements().ToList().FindAll(x => x.Value == li.Value).Count() == 0) allowedThingsInStockpile.Add(li);
                }
            }
            StructureLayoutDef.Add(allowedThingsInStockpile);
            XElement layouts = new XElement("layouts", null);
            // Add pawns layout
            bool add = false;
            XElement pawnsL = this.Createpawnlayout(cellExport, area, out add);
            if (add) layouts.Add(pawnsL);
            // Add items layout
            bool add2 = false;
            XElement itemsL = this.CreateItemlayout(cellExport, area, out add2);
            if (add2) layouts.Add(itemsL);
            // Add terrain layout
            StructureLayoutDef.Add(this.CreateTerrainlayout(cellExport, area));
            // Add things layouts
            int numOfLayout = this.GetMaxThingOnOneCell(cellExport);
            for (int i = 0; i < numOfLayout; i++)
            {
                layouts.Add(this.CreateThinglayout(cellExport, i, area));
            }
            
            StructureLayoutDef.Add(layouts);

            // Add roofGrid
            if (area != null) StructureLayoutDef.Add(this.CreateRoofGrid(cellExport, area));
            Log.Message(StructureLayoutDef.ToString());
        }

        private XElement CreateThinglayout(List<IntVec3> cellExport, int index, Area area)
        {
            XElement liMain = new XElement("li", null);
            int height, width;
            this.EdgeFromList(cellExport, out height, out width);
            List<Thing> aAdded = new List<Thing>();

            IntVec3 first = cellExport.First();
            for (int i = 0; i < height; i++)
            {
                XElement li = new XElement("li", null);
                string temp = "";
                for (int i2 = 0; i2 < width; i2++)
                {
                    List<Thing> things = first.GetThingList(map);
                    things.RemoveAll(t => t.def.category == ThingCategory.Pawn || t.def.category == ThingCategory.Item || t.def.category == ThingCategory.Filth || t.def.defName == "PowerConduit");
                    Thing thing;
                    if (things.Count < index + 1)
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else if (area != null && !area.ActiveCells.Contains(first))
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
                            else symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == thing.def && s.rotation == thing.Rotation);

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

        private XElement CreateTerrainlayout(List<IntVec3> cellExport, Area area)
        {
            XElement liMain = new XElement("terrainGrid", null);
            int height, width;
            this.EdgeFromList(cellExport, out height, out width);

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
                    else if (!map.terrainGrid.TerrainAt(first).BuildableByPlayer && !MapComponent_KSG.exportNaturalTerrain)
                    {
                        if (i2 + 1 == width) temp += ".";
                        else temp += ".,";
                    }
                    else
                    {
                        // Find corresponding symbol
                        TerrainDef terrainD = map.terrainGrid.TerrainAt(first);
                        SymbolDef symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.isTerrain && s.terrainDef.defName == terrainD.defName);
                        if (symbolDef == null)
                        {
                            if (i2 + 1 == width) temp += terrainD.defName;
                            else temp += terrainD.defName + ",";
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
                li.Add(temp);
                liMain.Add(li);
                first.z++;
            }
            return liMain;
        }

        private XElement CreateRoofGrid(List<IntVec3> cellExport, Area area)
        {
            XElement roofGrid = new XElement("roofGrid", null);
            int height, width;
            this.EdgeFromList(cellExport, out height, out width);

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
                    else
                    {
                        if (first.Roofed(area.Map))
                        {
                            if (i2 + 1 == width) temp += "1";
                            else temp += "1,";
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
                roofGrid.Add(li);
                first.z++;
            }
            return roofGrid;
        }

        private XElement Createpawnlayout(List<IntVec3> cellExport, Area area, out bool add)
        {
            XElement liMain = new XElement("li", null);
            int height, width;
            add = false;
            this.EdgeFromList(cellExport, out height, out width);

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
                    else
                    {
                        List<Thing> things = first.GetThingList(map).FindAll(t => t is Pawn);
                        if (things.Count == 0)
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                        else
                        {
                            add = true;
                            foreach (Pawn pawn in things)
                            {
                                SymbolDef symbolDef;
                                symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.pawnKindDefNS == pawn.kindDef/* && s.lordJob == pawn.GetLord().GetType()*/);
                                if (symbolDef == null)
                                {
                                    string symbolString = pawn.kindDef.defName;
                                    // symbolString += "_" + pawn.GetLord().GetType().Name;

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

        private XElement CreateItemlayout(List<IntVec3> cellExport, Area area, out bool add)
        {
            XElement liMain = new XElement("li", null);
            int height, width;
            add = false;
            this.EdgeFromList(cellExport, out height, out width);

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
                    else
                    {
                        List<Thing> things = first.GetThingList(map).FindAll(t => t.def.category == ThingCategory.Item && t.def.category != ThingCategory.Filth);
                        if (things.Count == 0)
                        {
                            if (i2 + 1 == width) temp += ".";
                            else temp += ".,";
                        }
                        else
                        {
                            add = true;
                            foreach (Thing item in things)
                            {
                                SymbolDef symbolDef;
                                symbolDef = pairsSymbolLabel.Values.ToList().Find(s => s.thingDef == item.def && s.isItem);
                                if (symbolDef == null)
                                {
                                    string symbolString = "Item_" + item.def.defName;

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

        private void EdgeFromList(List<IntVec3> cellExport, out int height, out int width)
        {
            height = 0;
            width = 0;
            IntVec3 first = cellExport.First();
            foreach (IntVec3 c in cellExport)
            {
                if (first.z == c.z) width++;
            }
            foreach (IntVec3 c in cellExport)
            {
                if (first.x == c.x) height++;
            }
#if DEBUG
            Log.Message("Export height: " + height.ToString() + " width: " + width.ToString());
#endif
        }

        private void EdgeFromArea(List<IntVec3> cellExport, out int height, out int width)
        {
            height = 0;
            width = 0;
            IntVec3 first = cellExport.First();
            foreach (IntVec3 f in cellExport)
            {
                int tempW = 0, tempH = 0;
                foreach (IntVec3 c in cellExport)
                {
                    if (f.z == c.z) tempW++;
                }
                foreach (IntVec3 c in cellExport)
                {
                    if (f.x == c.x) tempH++;
                }
                if (tempW > width) width = tempW;
                if (tempH > height) height = tempH;
            }
#if DEBUG
            Log.Message("Export area height: " + height.ToString() + " width: " + width.ToString());
#endif
        }

        private int GetMaxThingOnOneCell(List<IntVec3> cellExport)
        {
            int max = 1;
            foreach (var item in cellExport)
            {
                List<Thing> things = item.GetThingList(map).ToList();
                things.RemoveAll(t => t is Pawn || t.def.building == null || t.def.defName == "PowerConduit");
                if (things.Count > max) max = things.Count;
            }
            return max;
        }

        private List<IntVec3> AreaToSquare(Area a, int height, int widht)
        {
            List<IntVec3> list = a.ActiveCells.ToList();
            int zMin, zMax, xMin, xMax;
            KCSG_Utilities.MinMaxXZ(list, out zMin, out zMax, out xMin, out xMax);

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

        public void ProcessExport(List<IntVec3> cellExport)
        {
            Log.Clear();

            this.CreateSymbolIfNeeded(cellExport);
            this.CreateStructureDef(cellExport);

            Log.TryOpenLogWindow();
        }

        public void ProcessExportFromArea(Area area)
        {
            Log.Clear();
            
            int width, height;
            this.EdgeFromArea(area.ActiveCells.ToList(), out height, out width);
            List<IntVec3> cellExport = this.AreaToSquare(area, height, width);
            this.CreateSymbolIfNeeded(cellExport, area);
            this.CreateStructureDef(cellExport, area);

            Log.TryOpenLogWindow();
        }
    }
}
