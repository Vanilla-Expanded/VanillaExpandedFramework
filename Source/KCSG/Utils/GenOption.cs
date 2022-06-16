using RimWorld;
using Verse;

namespace KCSG
{
    public class GenOption
    {
        public static StuffableOptions StuffableOptions => sld.stuffableOptions;

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

        public static ThingDef RandomWallStuffByWeight()
        {
            if (StuffableOptions.generalWallStuff && generalWallStuff != null)
                return generalWallStuff;

            if (StuffableOptions.allowedWallStuff.Count > 0)
            {
                return StuffableOptions.allowedWallStuff.RandomElementByWeight(t => t.stuffProps.commonality);
            }

            if (StuffableOptions.disallowedWallStuff.Count > 0)
            {
                var from = SymbolDefsCreator.stuffs.FindAll(t => !StuffableOptions.disallowedWallStuff.Contains(t));
                return from.RandomElementByWeight(t => t.stuffProps.commonality);
            }

            Debug.Error("Using RandomWallStuffByWeight with empty allowedWallStuff/disallowedWallStuff");
            return null;
        }

        public static ThingDef RandomFurnitureStuffByWeight(SymbolDef symbol)
        {
            if (symbol.thingDef.costStuffCount <= 0)
                return null;

            if (StuffableOptions.randomizeFurniture && !StuffableOptions.excludedFunitureDefs.Contains(symbol.thingDef))
            {
                if (StuffableOptions.allowedFurnitureStuff.Count > 0)
                {
                    return StuffableOptions.allowedFurnitureStuff.FindAll(t => symbol.thingDef.stuffCategories.Find(c => t.stuffProps.categories.Contains(c)) != null).RandomElementByWeight(t => t.stuffProps.commonality);
                }

                if (StuffableOptions.disallowedFurnitureStuff.Count > 0)
                {
                    var from = SymbolDefsCreator.stuffs.FindAll(t => !StuffableOptions.disallowedFurnitureStuff.Contains(t));
                    return from.FindAll(t => symbol.thingDef.stuffCategories.Find(c => t.stuffProps.categories.Contains(c)) != null).RandomElementByWeight(t => t.stuffProps.commonality);
                }
            }

            return symbol.stuffDef ?? symbol.thingDef.defaultStuff ?? ThingDefOf.WoodLog;
        }
    }
}