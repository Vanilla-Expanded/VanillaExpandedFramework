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
using UnityEngine.Tilemaps;

namespace VEF.Maps
{
    // This Harmony patch will only be patched if TileMutatorMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_Game_InitNewGame_Patch
    {

        public static IEnumerable<CodeInstruction> TweakMapSizes(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var check = AccessTools.Method(typeof(VanillaExpandedFramework_Game_InitNewGame_Patch), "AdjustMapSize");
           
            var field = AccessTools.Field(typeof(Game), "initData");

          
            for (var i = 0; i < codes.Count; i++)
            {
                if (i == codes.Count)
                {
                    yield return codes[i];
                }
                else
                if (codes[i].opcode == OpCodes.Ldloc_1 && codes[i + 1].opcode == OpCodes.Ldloc_2)
                {
                   
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, field);
                    yield return new CodeInstruction(OpCodes.Call, check);
                   



                }
               
                else yield return codes[i];
            }
        }


      
        public static IntVec3 AdjustMapSize(GameInitData initData)
        {
            float multiplier = 1;
            float finalX = initData.mapSize;
            float finalZ = initData.mapSize;
            foreach (TileMutatorDef mutator in initData.startingTile.Tile.Mutators)
            {
             
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();
              
                if (extension != null)
                {
                    multiplier *= extension.mapSizeMultiplier;

                    if (extension.mapSizeOverrideX != -1)
                    {
                        finalX = extension.mapSizeOverrideX;
                    }
                    if (extension.mapSizeOverrideZ != -1)
                    {
                        finalZ = extension.mapSizeOverrideZ;
                    }
                }
 

            }
            return new IntVec3((int)(finalX*multiplier), 1, (int)(finalZ * multiplier));

        }
       
    }
}