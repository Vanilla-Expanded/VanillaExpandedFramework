using System;
using System.Collections.Generic;
using System.ComponentModel;
using RimWorld;
using UnityEngine;
using Verse;

// Copyright Sarg - Alpha Biomes 2020 & Taranchuck

namespace VFECore
{
    public class ObjectSpawnsDef : Def
    {
        public ThingDef thingDef;
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
            foreach (var error in base.ConfigErrors())
            {
                yield return error;
            }
            if (thingDef is null)
            {
                yield return "[VEF] ObjectSpawnsDef " + this.defName + " contain null thing def. It will not work.";
            }
        }
    }
}
