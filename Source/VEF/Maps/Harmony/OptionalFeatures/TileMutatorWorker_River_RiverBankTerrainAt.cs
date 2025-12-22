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


    public static class VanillaExpandedFramework_TileMutatorWorker_River_RiverBankTerrainAt_Patch
    {

        public static IEnumerable<CodeInstruction> MultiplyRiverBankSize(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();


            var lerpedCall = AccessTools.Method(typeof(IntRange), "Lerped");
            var multiplyBankSize = AccessTools.Method(typeof(VanillaExpandedFramework_TileMutatorWorker_River_RiverBankTerrainAt_Patch), "MultiplyBankSize");
            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Call && codes[i].Calls(lerpedCall))
                {

                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, multiplyBankSize);
                    yield return new CodeInstruction(OpCodes.Mul);

                }

                else yield return codes[i];
            }


        }


        public static int MultiplyBankSize(Map map)
        {

            int multiplier = 1;
            foreach (TileMutatorDef mutator in map.Tile.Tile.Mutators)
            {
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();

                if (extension != null && extension.riverbankSizeMultiplier != 1)
                {
                    multiplier *= extension.riverbankSizeMultiplier;
                }

            }
            return multiplier;
        }

    }
}