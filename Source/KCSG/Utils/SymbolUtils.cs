using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolUtils
    {
        public static void CreateItemSymbolFromThing(Thing thingT, List<XElement> symbols)
        {
            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.thingDef == thingT.def).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", thingT.def.defName));
                symbolDef.Add(new XElement("thing", thingT.def.defName));

                if (!symbols.Contains(symbolDef))
                {
                    symbols.Add(symbolDef);
                }
            }
        }

        public static void CreateSymbolFromPawn(Pawn pawn, List<XElement> symbols)
        {
            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.pawnKindDefNS == pawn.kindDef).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", pawn.kindDef.defName));
                symbolDef.Add(new XElement("pawnKindDef", pawn.kindDef.defName));

                if (!symbols.Contains(symbolDef))
                {
                    symbols.Add(symbolDef);
                }
            }
        }

        public static void CreateSymbolFromTerrain(TerrainDef terrainD, List<XElement> symbols)
        {
            if (!DefDatabase<SymbolDef>.AllDefsListForReading.FindAll(s => s.terrainDef == terrainD).Any())
            {
                XElement symbolDef = new XElement("KCSG.SymbolDef", null);
                symbolDef.Add(new XElement("defName", terrainD.defName));
                symbolDef.Add(new XElement("isTerrain", "true"));
                symbolDef.Add(new XElement("terrain", terrainD.defName));

                if (!symbols.Contains(symbolDef))
                {
                    symbols.Add(symbolDef);
                }
            }
        }

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

                if (!symbols.Contains(symbolDef))
                {
                    symbols.Add(symbolDef);
                }
            }
        }

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
    }
}