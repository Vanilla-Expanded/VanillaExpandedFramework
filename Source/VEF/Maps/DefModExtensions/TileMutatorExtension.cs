using Verse;
using RimWorld;
using System.Collections.Generic;

namespace VEF.Maps
{

    public class TileMutatorExtension : DefModExtension
    {


        public List<PawnKindDefAndChance> forcedPawnKindDefs;

        public ThingDef thingToSpawn;
        public IntRange thingToSpawnAmount;

        public List<PrefabDef> prefabsToSpawn;
        public IntRange prefabsToSpawnAmount = new IntRange(1, 1);
        public int minSeparationBetweenPrefabs = 10;

        public List<KCSG.StructureLayoutDef> KCSGStructuresToSpawn;
        public IntRange KCSGStructuresToSpawnAmount = new IntRange(1, 1);
        public int minSeparationBetweenKCSGStructures = 10;

        public TerrainDef terrainToSwap;
        public TerrainDef terrainToSwapTo;

        public int mapSizeOverrideX = -1;
        public int mapSizeOverrideZ = -1;
        public float mapSizeMultiplier = 1;

        public float deepOresMultiplier = 1;

    }

    public class PawnKindDefAndChance
    {
        public PawnKindDef forcedPawnKindDef;
        public float forcedPawnKindDefChance;

    }

}
