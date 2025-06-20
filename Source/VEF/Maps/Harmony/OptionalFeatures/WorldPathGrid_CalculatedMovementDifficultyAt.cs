using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using RimWorld.Planet;
using System;


namespace VEF.Maps
{

    // This Harmony patch will only be patched if TileMutatorMechanics is added via XML to a mod using OptionalFeatures


    public static class VanillaExpandedFramework_WorldPathGrid_CalculatedMovementDifficultyAt_Patch
    {

        public static IEnumerable<CodeInstruction> TweakMovementDifficulty(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

            var getPrimaryBiome = AccessTools.PropertyGetter(typeof(Tile), "PrimaryBiome");
            var movementOffset = AccessTools.Method(typeof(VanillaExpandedFramework_WorldPathGrid_CalculatedMovementDifficultyAt_Patch), "OffsetMovementDifficulty");
            var field = AccessTools.Field(typeof(BiomeDef), "movementDifficulty");

            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 1 && codes[i - 2].opcode == OpCodes.Ldloc_0 && codes[i-1].Calls(getPrimaryBiome) && codes[i].LoadsField(field))
                {

                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, movementOffset);

                }

                else yield return codes[i];
            }
        }


        public static float OffsetMovementDifficulty(float movementDifficulty, Tile tile)
        {
           
            float offset = 0;
            foreach (TileMutatorDef mutator in tile.Mutators)
            {
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();

                if (extension != null && extension.movementDifficultyOffset != 0)
                {
                    offset += extension.movementDifficultyOffset;
                }

            }
            return movementDifficulty + offset > 0 ?  movementDifficulty +offset : 0.1f;
        }

    }
}