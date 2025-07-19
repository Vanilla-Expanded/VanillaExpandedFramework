using RimWorld;
using System.Collections.Generic;
using Verse;

// Copyright Sarg - Alpha Biomes 2020 & Taranchuck
// You know we were serious with this one, as it has COPYRIGHT

namespace VEF.Maps
{
    public class ObjectSpawnsDef : Def
    {
        public ThingDef thingDef;
        public List<ThingOption> thingDefs;
        public ThingCategoryDef category;

        public PawnKindDef pawnKindDef;
        public FactionDef factionDef;

        public bool allowOnWater;
        public bool allowOnChunks;
        public IntRange numberToSpawn;
        public List<string> allowedTerrains;
        public List<string> disallowedTerrainTags;
        public List<BiomeDef> allowedBiomes;
        public List<RoadDef> allowedRoads;
        public bool findCellsOutsideColony = false;
        public bool spawnOnlyInPlayerMaps = false;
        public bool randomRotation;
        public bool allowSpawningOnPocketMaps = false;

        public List<PlanetLayerDef> allowedPlanetLayers;
    }
}
