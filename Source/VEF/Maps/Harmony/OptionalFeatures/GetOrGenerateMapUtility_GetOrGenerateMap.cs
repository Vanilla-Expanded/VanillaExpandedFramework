using HarmonyLib;
using Mono.Cecil.Cil;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld.Planet;
using Verse;
using UnityEngine.UIElements;
using Verse.Noise;

namespace VEF.Maps
{

    // This Harmony patch will only be patched if TileMutatorMechanics is added via XML to a mod using OptionalFeatures


    public static class VanillaExpandedFramework_GetOrGenerateMapUtility_GetOrGenerateMap_Patch
    {


        public static void TweakMapSizes(PlanetTile tile, ref IntVec3 size, WorldObjectDef suggestedMapParentDef, IEnumerable<GenStepWithParams> extraGenStepDefs = null, bool stepDebugger = false)
        {
            foreach (TileMutatorDef mutator in tile.Tile.Mutators)
            {
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();
                if(extension!=null)
                {
                    float multiplier = extension.mapSizeMultiplier;
                    int x = (int)(size.x*multiplier);
                    int z = (int)(size.z*multiplier);

                    if (extension.mapSizeOverrideX != -1)
                    {
                        x = (int)(extension.mapSizeOverrideX*multiplier);
                    }
                    if (extension.mapSizeOverrideZ != -1)
                    {
                        z = (int)(extension.mapSizeOverrideZ*multiplier);
                    }
                    size = new IntVec3(x, 1, z);
                }

            }
            

        }

    }

}