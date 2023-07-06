using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    internal class StartupActions
    {
        public static bool defsCreated = false;
        public static List<ThingDef> stuffs = new List<ThingDef>();
        public static Dictionary<string, Dictionary<string, int>> missingSymbols;

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
            missingSymbols = new Dictionary<string, Dictionary<string, int>>();

            var layouts = DefDatabase<StructureLayoutDef>.AllDefsListForReading;
            for (int i = 0; i < layouts.Count; i++)
                layouts[i].ResolveLayouts();

            // Output list of missing symbols
            foreach (var missing in missingSymbols)
            {
                Log.Warning($"[KCSG] {missing.Key} contains {missing.Value.Count} missing symbols.");
                foreach (var m in missing.Value)
                {
                    Debug.Message($"Missing symbol: {m.Key} (needed {m.Value} times)");
                }
            }
            // Cache things
            SettlementGenUtils.BuildingPlacement.CacheTags();
            // Resolve tiles
            var tiledStruct = DefDatabase<TiledStructureDef>.AllDefsListForReading;
            for (int i = 0; i < tiledStruct.Count; i++)
                tiledStruct[i].Resolve();
            // Make new map generators, used with preventBridgeable
            CreateMapGeneratorDefs();
        }

        /// <summary>
        /// Add missing symbol to dic, for verbose
        /// </summary>
        public static void AddToMissing(string modName, string symbol)
        {
            if (modName == null)
            {
                Debug.Message($"Null modName for {symbol}");
                return;
            }

            if (missingSymbols.ContainsKey(modName))
            {
                if (missingSymbols[modName].ContainsKey(symbol))
                {
                    missingSymbols[modName][symbol]++;
                }
                else if (!symbol.Contains("VFEPD"))
                {
                    missingSymbols[modName].Add(symbol, 1);
                }
            }
            else if (!symbol.Contains("VFEPD"))
            {
                missingSymbols.Add(modName, new Dictionary<string, int>());
                missingSymbols[modName].Add(symbol, 1);
            }
        }

        /// <summary>
        /// Create MapGeneratorDef that use KCSG_TerrainNoPatches instead of Terrain
        /// Allow to skip terrain patches maker
        /// </summary>
        private static void CreateMapGeneratorDefs()
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

        /// <summary>
        /// Create all symbols for rimworld/royalty/ideology and vfepropsanddecor if possible
        /// </summary>
        public static void CreateSymbols()
        {
            if (defsCreated)
                return;

            createdSymbolAmount = 0;
            var thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            var pawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading;

            foreach (string id in ModContentPack.ProductPackageIDs)
                CreateSymbolsFor(thingDefs, pawnKindDefs, id);

            if (ModLister.GetActiveModWithIdentifier("vanillaexpanded.vfepropsanddecor") != null)
                CreateSymbolsFor(thingDefs, pawnKindDefs, "vanillaexpanded.vfepropsanddecor");

            Debug.Message($"Created {createdSymbolAmount} symbolDefs for vanilla and DLCs");
            defsCreated = true;
        }

        /// <summary>
        /// Add def with a max amount of ushort.MaxValue SymbolDefs in database
        /// </summary>
        private static void AddDef(SymbolDef def)
        {
            if (!defsCreated && DefDatabase<SymbolDef>.DefCount < ushort.MaxValue)
            {
                if (DefDatabase<SymbolDef>.GetNamedSilentFail(def.defName) == null)
                    DefDatabase<SymbolDef>.Add(def);
            }
            else
            {
                Debug.Error("Cannot add more symbolDef. Maximum amount reached.");
                defsCreated = true;
            }
        }

        /// <summary>
        /// Create a thing SymbolDef
        /// </summary>
        private static void CreateAllSymbolsForDef(ThingDef thing)
        {
            if (thing.IsCorpse)
            {
                return;
            }
            else if (thing.category == ThingCategory.Item || thing.IsFilth)
            {
                AddDef(CreateSymbolDef(thing));
            }
            else if (thing.stuffCategories != null)
            {
                foreach (StuffCategoryDef stuffCat in thing.stuffCategories)
                {
                    foreach (ThingDef stuffDef in stuffs.FindAll(t => t.stuffProps.categories.Contains(stuffCat)))
                    {
                        if (thing.rotatable)
                        {
                            AddDef(CreateSymbolDef(thing, stuffDef, Rot4.North));
                            AddDef(CreateSymbolDef(thing, stuffDef, Rot4.South));
                            AddDef(CreateSymbolDef(thing, stuffDef, Rot4.East));
                            AddDef(CreateSymbolDef(thing, stuffDef, Rot4.West));
                        }
                        else
                        {
                            AddDef(CreateSymbolDef(thing, stuffDef));
                        }
                    }
                }
            }
            else if (thing.plant != null)
            {
                AddDef(CreatePlantSymbolDef(thing));
            }
            else if (thing.rotatable)
            {
                AddDef(CreateSymbolDef(thing, Rot4.North));
                AddDef(CreateSymbolDef(thing, Rot4.South));
                AddDef(CreateSymbolDef(thing, Rot4.East));
                AddDef(CreateSymbolDef(thing, Rot4.West));
            }
            else
            {
                AddDef(CreateSymbolDef(thing));
            }
        }

        /// <summary>
        /// Create a plant SymbolDef
        /// </summary>
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

        /// <summary>
        /// Create a SymbolDef from a PawnKindDef
        /// </summary>
        private static SymbolDef CreateSymbolDef(PawnKindDef pawnKindDef)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{pawnKindDef.defName}",
                pawnKindDefNS = pawnKindDef,
                isSlave = pawnKindDef.defName == "Slave"
            };
            createdSymbolAmount++;
            return symbolDef;
        }


        /// <summary>
        /// Create a SymbolDef from a corpse
        /// </summary>
        private static SymbolDef CreateCorpseSymbolDef(PawnKindDef pawnKindDef)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"Corpse_{pawnKindDef.defName}",
                pawnKindDefNS = pawnKindDef,
                spawnDead = true
            };
            createdSymbolAmount++;
            return symbolDef;
        }

        /// <summary>
        /// Create a SymbolDef from a thing/stuff/rot
        /// </summary>
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

        /// <summary>
        /// Create a SymbolDef from a thing/stuff
        /// </summary>
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

        /// <summary>
        /// Create a SymbolDef from a thing/rot
        /// </summary>
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

        /// <summary>
        /// Create a SymbolDef from a thing
        /// </summary>
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

        /// <summary>
        /// Call CreateAllSymbolsForDef & CreateSymbolDef
        /// </summary>
        private static void CreateSymbolsFor(List<ThingDef> thingDefs, List<PawnKindDef> pawnKindDefs, string modId)
        {
            for (int i = 0; i < thingDefs.Count && !defsCreated; i++)
            {
                var thing = thingDefs[i];
                if (thing.modContentPack?.PackageId == modId)
                    CreateAllSymbolsForDef(thing);
            }

            for (int i = 0; i < pawnKindDefs.Count && !defsCreated; i++)
            {
                var kind = pawnKindDefs[i];
                if (kind.modContentPack?.PackageId == modId)
                {
                    AddDef(CreateSymbolDef(kind));
                    AddDef(CreateCorpseSymbolDef(kind));
                }
            }
        }

        /// <summary>
        /// Translate a rot to english, no matter the choosen game language
        /// </summary>
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