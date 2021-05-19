using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolUtils
    {
        public static bool AlreadyExist(Thing thing, TerrainDef terrain)
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

        public static void CreateItemSymbolFromThing(Thing thingT, string defnamePrefix, List<string> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            symbolDef.Add(new XElement("defName", defnamePrefix + "_Item_" + thingT.def.defName)); // defName
            symbolDef.Add(new XElement("thing", thingT.def.defName)); // thing defName
            symbolDef.Add(new XElement("isItem", "true")); // isItem?
            symbolDef.Add(new XElement("stackCount", (new IntRange(1, Math.Min(75, thingT.def.stackLimit)).ToString()))); // stackCount

            XElement symbol = new XElement("symbol", defnamePrefix + "_Item_" + thingT.def.defName); // symbol
            symbolDef.Add(symbol);

            if (!alreadyCreated.Contains(symbol.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbol.Value);
        }

        public static void CreateSymbolFromPawn(Pawn pawn, string defnamePrefix, List<string> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            symbolDef.Add(new XElement("defName", defnamePrefix + "_" + pawn.kindDef.defName)); // defName
            symbolDef.Add(new XElement("isPawn", "true")); // isPawn
            symbolDef.Add(new XElement("pawnKindDef", pawn.kindDef.defName)); // pawnKindDef

            XElement symbol = new XElement("symbol", defnamePrefix + "_" + pawn.kindDef.defName); // symbol
            symbolDef.Add(symbol);

            if (!alreadyCreated.Contains(symbol.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbol.Value);
        }

        public static void CreateSymbolFromTerrain(TerrainDef terrainD, string defnamePrefix, List<string> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            symbolDef.Add(new XElement("defName", defnamePrefix + "_" + terrainD.defName)); // defName
            symbolDef.Add(new XElement("isTerrain", "true")); // isTerrain
            symbolDef.Add(new XElement("terrain", terrainD.defName)); // terrain defName

            XElement symbol = new XElement("symbol", defnamePrefix + "_" + terrainD.defName); // symbol
            symbolDef.Add(symbol);

            if (!alreadyCreated.Contains(symbol.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbol.Value);
        }

        public static void CreateSymbolFromThing(Thing thingT, string defnamePrefix, List<string> alreadyCreated, List<XElement> symbols)
        {
            XElement symbolDef = new XElement("KCSG.SymbolDef", null);
            // Generate defName
            string defNameString = defnamePrefix + "_" + thingT.def.defName;
            if (thingT.Stuff != null) defNameString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant) defNameString += "_" + thingT.Rotation.ToStringHuman();

            symbolDef.Add(new XElement("defName", defNameString)); // defName
            symbolDef.Add(new XElement("thing", thingT.def.defName)); // thing defName
            if (thingT.Stuff != null)
                symbolDef.Add(new XElement("stuff", thingT.Stuff.defName)); // Add stuff
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant)
                symbolDef.Add(new XElement("rotation", thingT.Rotation.ToStringHuman())); // Add rotation
            if (thingT is Plant plant)
                symbolDef.Add(new XElement("plantGrowth", plant.Growth.ToString())); // Plant growth

            string symbolString = defnamePrefix + "_" + thingT.def.defName;
            if (thingT.Stuff != null) symbolString += "_" + thingT.Stuff.defName;
            if (thingT.def.rotatable && thingT.def.category != ThingCategory.Plant) symbolString += "_" + thingT.Rotation.ToStringHuman();

            XElement symbol = new XElement("symbol", symbolString); // symbol
            symbolDef.Add(symbol);

            if (!alreadyCreated.Contains(symbolDef.Value)) symbols.Add(symbolDef); alreadyCreated.Add(symbolDef.Value);
        }

        public static List<XElement> CreateSymbolIfNeeded(List<IntVec3> cellExport, Map map, string defnamePrefix, Dictionary<IntVec3, List<Thing>> pairsCellThingList, Area area = null)
        {
            List<string> justCreated = new List<string>();
            List<XElement> symbols = new List<XElement>();

            foreach (IntVec3 c in cellExport)
            {
                if (area != null && !area.ActiveCells.Contains(c)) { }
                else
                {
                    TerrainDef terrainDef = c.GetTerrain(map);
                    if (terrainDef != null && !AlreadyExist(null, terrainDef) && terrainDef.BuildableByPlayer) CreateSymbolFromTerrain(terrainDef, defnamePrefix, justCreated, symbols);

                    List<Thing> things = pairsCellThingList.TryGetValue(c);
                    foreach (Thing t in things)
                    {
                        if (t != null && !AlreadyExist(t, null))
                        {
                            if (t.def.category == ThingCategory.Item) CreateItemSymbolFromThing(t, defnamePrefix, justCreated, symbols);
                            if (t.def.category == ThingCategory.Pawn) CreateSymbolFromPawn(t as Pawn, defnamePrefix, justCreated, symbols);
                            if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Plant)
                            {
                                CreateSymbolFromThing(t, defnamePrefix, justCreated, symbols);
                            }
                        }
                    }
                }
            }

            return symbols;
        }
    }
}