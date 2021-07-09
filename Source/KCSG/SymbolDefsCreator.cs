using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace KCSG
{
    [StaticConstructorOnStartup]
    class SymbolDefsCreator
    {
        static List<ThingDef> stuffs = new List<ThingDef>();
        static int sCreated;
        static int modCount;

        static SymbolDefsCreator()
        {
            stuffs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.IsStuff);
            sCreated = 0;
            DateTime before = DateTime.Now;

            Log.Message($"<color=orange>[CSG]</color> Symbols Creator Started.");

            CreateSymbolsFor("ludeon.rimworld");
            CreateSymbolsFor("ludeon.rimworld.royalty");
            CreateSymbolsFor("ludeon.rimworld.ideology");

            foreach (var item in ModsConfig.ActiveModsInLoadOrder)
            {
                if (DefDatabase<SymbolAutoCreation>.AllDefsListForReading.Find(d => d.modContentPack?.PackageId == item.PackageId && d.autoSymbolsCreation) != null)
                {
                    CreateSymbolsFor(item.PackageId.ToLower());
                }
            }

            Log.Message($"<color=orange>[CSG]</color> Created {sCreated} symbolDefs for {modCount} mods. Took {(DateTime.Now - before).TotalSeconds.ToString("00.00")}s.");
        }

        static void CreateSymbolsFor(string modId)
        {
            if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message($"Creating symbols for {modId}...");
            
            List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.modContentPack?.PackageId == modId);
            foreach (ThingDef thingDef in thingDefs)
            {
                CreateAllSymbolsForDef(thingDef);
            }

            List<TerrainDef> terrainDefs = DefDatabase<TerrainDef>.AllDefsListForReading.FindAll(t => t.modContentPack?.PackageId == modId);
            foreach (TerrainDef terrainDef in terrainDefs)
            {
                AddDef(CreateSymbolDef(terrainDef));
            }

            List<PawnKindDef> pawnKindDefs = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(t => t.modContentPack?.PackageId == modId);
            foreach (PawnKindDef pawnKindDef in pawnKindDefs)
            {
                AddDef(CreateSymbolDef(pawnKindDef));
            }
            modCount++;
        }
                
        static void CreateAllSymbolsForDef(ThingDef thing)
        {
            if (thing.category == ThingCategory.Item || thing.IsFilth)
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

        static SymbolDef CreatePlantSymbolDef(ThingDef thing)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}",
                thingDef = thing,
            };

            sCreated++;
            return symbolDef;
        }

        static SymbolDef CreateSymbolDef(TerrainDef terrain)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{terrain.defName}",
                isTerrain = true,
                terrainDef = terrain,
            };
            sCreated++;
            return symbolDef;
        }

        static SymbolDef CreateSymbolDef(PawnKindDef pawnKindDef)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{pawnKindDef.defName}",
                pawnKindDefNS = pawnKindDef,
            };
            sCreated++;
            return symbolDef;
        }

        static SymbolDef CreateSymbolDef(ThingDef thing, ThingDef stuff, Rot4 rot)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}_{stuff.defName}_{rot.ToStringHuman()}",
                thingDef = thing,
                stuffDef = stuff,
                rotation = rot,
            };
            sCreated++;
            return symbolDef;
        }

        static SymbolDef CreateSymbolDef(ThingDef thing, ThingDef stuff)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}_{stuff.defName}",
                thingDef = thing,
                stuffDef = stuff,
            };
            sCreated++;
            return symbolDef;
        }

        static SymbolDef CreateSymbolDef(ThingDef thing, Rot4 rot)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}_{rot.ToStringHuman()}",
                thingDef = thing,
                rotation = rot,
            };
            sCreated++;
            return symbolDef;
        }

        static SymbolDef CreateSymbolDef(ThingDef thing)
        {
            SymbolDef symbolDef = new SymbolDef
            {
                defName = $"{thing.defName}",
                thingDef = thing,
            };

            sCreated++;
            return symbolDef;
        }

        static void AddDef(SymbolDef def)
        {
            if (DefDatabase<SymbolDef>.GetNamedSilentFail(def.defName) == null)
                DefDatabase<SymbolDef>.Add(def);
        }
    }
}
