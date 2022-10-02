using RimWorld;
using System.Collections.Generic;
using Verse;

// Copyright Sarg - Alpha Biomes 2020 & Taranchuck

namespace VFECore
{
    public class ObjectSpawnsDef : Def
    {
        public ThingDef thingDef;
        public PawnKindDef pawnKindDef;
        public FactionDef factionDef;

        public bool allowOnWater;
        public bool allowOnChunks;
        public IntRange numberToSpawn;
        public List<string> allowedTerrains;
        public List<string> disallowedTerrainTags;
        public List<BiomeDef> allowedBiomes;
        public bool findCellsOutsideColony = false;
        public bool spawnOnlyInPlayerMaps = false;
        public bool randomRotation;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (thingDef is null && pawnKindDef is null)
            {
                yield return "[VEF] ObjectSpawnsDef " + defName + " contain null thing/pawnkind def. It will not work.";
            }
        }
    }
}
