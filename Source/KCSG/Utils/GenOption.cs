using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KCSG
{
    public class GenOption
    {
        public static StuffableOptions StuffableOptions => sld.stuffableOptions;
        public static RoadOptions RoadOptions => sld.roadOptions;


        public static CustomGenOption ext;

        public static SettlementLayoutDef sld;
        public static ThingDef generalWallStuff;

        public static StructureLayoutDef structureLayoutDef;

        /*------ Falling structure ------*/
        public static FallingStructure fallingStructure;
        public static StructureLayoutDef fallingStructureChoosen;

        public static void ClearFalling()
        {
            fallingStructure = null;
            fallingStructureChoosen = null;
        }

        public static ThingDef RandomWallStuffByWeight(ThingDef thingDef)
        {
            if (StuffableOptions.generalWallStuff && generalWallStuff != null)
                return generalWallStuff;

            if (StuffableOptions.allowedWallStuff.Count > 0)
            {
                return RandomStuffFromFor(StuffableOptions.allowedWallStuff, thingDef);
            }

            if (StuffableOptions.disallowedWallStuff.Count > 0)
            {
                var from = SymbolDefsCreator.stuffs.FindAll(t => !StuffableOptions.disallowedWallStuff.Contains(t));
                return RandomStuffFromFor(from, thingDef);
            }

            return RandomStuffFromFor(SymbolDefsCreator.stuffs, thingDef);
        }

        public static ThingDef RandomFurnitureStuffByWeight(SymbolDef symbol)
        {
            if (symbol.thingDef.costStuffCount <= 0)
                return null;

            if (StuffableOptions.randomizeFurniture && !StuffableOptions.excludedFunitureDefs.Contains(symbol.thingDef))
            {
                if (StuffableOptions.allowedFurnitureStuff.Count > 0)
                {
                    return RandomStuffFromFor(StuffableOptions.allowedFurnitureStuff, symbol.thingDef);
                }

                if (StuffableOptions.disallowedFurnitureStuff.Count > 0)
                {
                    var from = SymbolDefsCreator.stuffs.FindAll(t => !StuffableOptions.disallowedFurnitureStuff.Contains(t));
                    return RandomStuffFromFor(from, symbol.thingDef);
                }

                return RandomStuffFromFor(SymbolDefsCreator.stuffs, symbol.thingDef);
            }

            return symbol.stuffDef ?? symbol.thingDef.defaultStuff ?? ThingDefOf.WoodLog;
        }

        public static ThingDef RandomStuffFromFor(List<ThingDef> from, ThingDef thingDef) => from.FindAll(t => thingDef.stuffCategories.Find(c => t.stuffProps.categories.Contains(c)) != null).RandomElementByWeight(t => t.stuffProps.commonality);
    }
}