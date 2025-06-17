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
            var checkX = AccessTools.Method(typeof(VanillaExpandedFramework_Game_InitNewGame_Patch), "AdjustMapSizeX");
            var checkZ = AccessTools.Method(typeof(VanillaExpandedFramework_Game_InitNewGame_Patch), "AdjustMapSizeZ");

            var field = AccessTools.Field(typeof(Game), "initData");

            int position = 0;
            bool found = false;
            for (var i = 0; i < codes.Count; i++)
            {
                if (i == 0)
                {
                    yield return codes[i];
                }
                else
                if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i - 1].opcode == OpCodes.Ldloca_S)
                {
                    position = i;
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, field);
                    yield return new CodeInstruction(OpCodes.Call, checkX);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, field);
                    yield return new CodeInstruction(OpCodes.Call, checkZ);



                }
                else if (found && i > position && i < position + 7)
                {
                    yield return new CodeInstruction(OpCodes.Nop);
                }
                else yield return codes[i];
            }
        }


      
        public static int AdjustMapSizeX(GameInitData initData)
        {
            float multiplier = 1;
            foreach (TileMutatorDef mutator in initData.startingTile.Tile.Mutators)
            {
             
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();
               multiplier = extension is null ? 1 : extension.mapSizeMultiplier;
              
                if (extension != null && extension.mapSizeOverrideX != -1)
                {
                   
                    return (int)(extension.mapSizeOverrideX * multiplier);
                }
                



            }
            return (int)(initData.mapSize * multiplier);

        }
        public static int AdjustMapSizeZ(GameInitData initData)
        {
            float multiplier = 1;
            foreach (TileMutatorDef mutator in initData.startingTile.Tile.Mutators)
            {
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();
                multiplier = extension is null ? 1 : extension.mapSizeMultiplier;
                if (extension != null && extension.mapSizeOverrideZ != -1)
                {
                    return (int)(extension.mapSizeOverrideZ * multiplier);
                }
                

            }
            return (int)(initData.mapSize * multiplier);

        }
    }
}