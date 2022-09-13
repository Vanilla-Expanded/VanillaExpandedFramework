using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    internal class StartupActions
    {
        public static bool defCreated = false;
        public static List<ThingDef> stuffs = new List<ThingDef>();
        public static Dictionary<string, int> missingSymbols;

        private static int createdSymbolAmount;

        static StartupActions()
        {
            var debug = VFECore.VFEGlobal.settings.enableVerboseLogging;
            stuffs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.IsStuff);

            // Create vanilla + dlcs sybmols
            if (debug
                || DefDatabase<SettlementLayoutDef>.DefCount > 0
                || DefDatabase<StructureLayoutDef>.DefCount > 0
                || DefDatabase<SymbolDef>.DefCount > 0)
            {
                CreateSymbols();
            }

            // Resolve all layouts, for faster gen time
            missingSymbols = new Dictionary<string, int>();

            var layouts = DefDatabase<StructureLayoutDef>.AllDefsListForReading;
            for (int i = 0; i < layouts.Count; i++)
                layouts[i].ResolveLayouts();

            // Output list of missing symbols
            if (debug)
            {
                foreach (var m in missingSymbols)
                {
                    Debug.Message($"Missing symbol: {m.Key} ({m.Value})");
                }
            }
            // Cache layout per tag
            SettlementGenUtils.BuildingPlacement.CacheTags();
            // Make two new map generator, used with preventBridgeable
            CreateMapDefs();
        }
        public static void AddToMissing(string symbol)
        {
            if (missingSymbols.ContainsKey(symbol))
                missingSymbols[symbol]++;
            else
                missingSymbols.Add(symbol, 1);
        }

        private static void CreateMapDefs()
        {
            var baseA = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_Base_Faction");
            MapGeneratorDef mgdA = new MapGeneratorDef
            {
                defName = "KCSG_Base_Faction_NoBridge",
                genSteps = baseA.genSteps.ListFullCopy()
            };
            mgdA.genSteps.Replace(AllDefOf.Terrain, AllDefOf.KCSG_TerrainNoPatches);
            DefDatabase<MapGeneratorDef>.Add(mgdA);

            var baseB = DefDatabase<MapGeneratorDef>.GetNamed("KCSG_WorldObject");
            MapGeneratorDef mgdB = new MapGeneratorDef
            {
                defName = "KCSG_WorldObject_NoBridge",
                genSteps = baseB.genSteps.ListFullCopy()
            };
            mgdB.genSteps.Replace(AllDefOf.Terrain, AllDefOf.KCSG_TerrainNoPatches);
            DefDatabase<MapGeneratorDef>.Add(mgdB);

            MapGeneratorDef enc = new MapGeneratorDef
            {
                defName = "KCSG_Encounter_NoBridge",
                genSteps = MapGeneratorDefOf.Encounter.genSteps.ListFullCopy()
            };
            enc.genSteps.Replace(AllDefOf.Terrain, AllDefOf.KCSG_TerrainNoPatches);
            DefDatabase<MapGeneratorDef>.Add(enc);
        }

        public static void CreateSymbols()
        {
            if (defCreated)
                return;

            createdSymbolAmount = 0;

            CreateSymbolsFor("ludeon.rimworld");
            CreateSymbolsFor("ludeon.rimworld.royalty");
            CreateSymbolsFor("ludeon.rimworld.ideology");
            if (ModLister.GetActiveModWithIdentifier("vanillaexpanded.vfepropsanddecor") != null)
                CreateSymbolsFor("vanillaexpanded.vfepropsanddecor");

            Debug.Message($"Created {createdSymbolAmount} symbolDefs for vanilla and DLCs");
            defCreated = true;
        }

        private static void AddDef(SymbolDef def)
        {
            if (DefDatabase<SymbolDef>.DefCount < 65535)
            {
                if (DefDatabase<SymbolDef>.GetNamedSilentFail(def.defName) == null)
                {
                    DefDatabase<SymbolDef>.Add(def);
                }
            }
            else
            {
                Debug.Error("Cannot add more symbolDef. Maximum amount reached.");
                defCreated = true;
            }
        }

        private static void CreateAllSymbolsForDef(ThingDef thing)
        {
            if (thing.category == ThingCategory.Item || thing.IsFilth)
            {
                if (!defCreated) AddDef(CreateSymbolDef(thing));
            }
            else if (thing.stuffCategories != null)
            {
                foreach (StuffCategoryDef stuffCat in thing.stuffCategories)
                {
                    foreach (ThingDef stuffDef in stuffs.FindAll(t => t.stuffProps.categories.Contains(stuffCat)))
                    {
                        if (thing.rotatable)
                        {
                            if (!defCreated) AddDef(CreateSymbolDef(thing, stuffDef, Rot4.North));
                            if (!defCreated) AddDef(CreateSymbolDef(thing, stuffDef, Rot4.South));
                            if (!defCreated) AddDef(CreateSymbolDef(thing, stuffDef, Rot4.East));
                            if (!defCreated) AddDef(CreateSymbolDef(thing, stuffDef, Rot4.West));
                        }
                        else
                        {
                            if (!defCreated) AddDef(CreateSymbolDef(thing, stuffDef));
                        }
                    }
                }
            }
            else if (thing.plant != null)
            {
                if (!defCreated) AddDef(CreatePlantSymbolDef(thing));
            }
            else if (thing.rotatable)
            {
                if (!defCreated) AddDef(CreateSymbolDef(thing, Rot4.North));
                if (!defCreated) AddDef(CreateSymbolDef(thing, Rot4.South));
                if (!defCreated) AddDef(CreateSymbolDef(thing, Rot4.East));
                if (!defCreated) AddDef(CreateSymbolDef(thing, Rot4.West));
            }
            else
            {
                if (!defCreated) AddDef(CreateSymbolDef(thing));
            }
        }

        private static SymbolDef CreatePlantSymbolDef(ThingDef thing)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}",
                thingDef = thing,
            };

            createdSymbolAmount++;
            return symbolDef;
        }

        private static SymbolDef CreateSymbolDef(PawnKindDef pawnKindDef)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{pawnKindDef.defName}",
                pawnKindDefNS = pawnKindDef,
            };
            createdSymbolAmount++;
            return symbolDef;
        }

        private static SymbolDef CreateSymbolDef(ThingDef thing, ThingDef stuff, Rot4 rot)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}_{stuff.defName}_{Rot4ToStringEnglish(rot)}",
                thingDef = thing,
                stuffDef = stuff,
                rotation = rot,
            };
            createdSymbolAmount++;
            return symbolDef;
        }

        private static SymbolDef CreateSymbolDef(ThingDef thing, ThingDef stuff)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}_{stuff.defName}",
                thingDef = thing,
                stuffDef = stuff,
            };
            createdSymbolAmount++;
            return symbolDef;
        }

        private static SymbolDef CreateSymbolDef(ThingDef thing, Rot4 rot)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}_{Rot4ToStringEnglish(rot)}",
                thingDef = thing,
                rotation = rot,
            };
            createdSymbolAmount++;
            return symbolDef;
        }

        private static SymbolDef CreateSymbolDef(ThingDef thing)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}",
                thingDef = thing,
            };

            createdSymbolAmount++;
            return symbolDef;
        }

        private static void CreateSymbolsFor(string modId)
        {
            List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.modContentPack?.PackageId == modId);
            foreach (ThingDef thingDef in thingDefs)
            {
                if (!defCreated) CreateAllSymbolsForDef(thingDef);
            }

            List<PawnKindDef> pawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(t => t.modContentPack?.PackageId == modId);
            foreach (PawnKindDef pawnKindDef in pawnKindDefs)
            {
                if (!defCreated) AddDef(CreateSymbolDef(pawnKindDef));
            }
        }

        public static string Rot4ToStringEnglish(Rot4 rot4)
        {
            switch (rot4.AsInt)
            {
                case 0:
                    return "North";
                case 1:
                    return "East";
                case 2:
                    return "South";
                case 3:
                    return "West";
                default:
                    return "error";
            }
        }
    }
}