using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using UnityEngine;
using System;


namespace VEF.Maps
{

    // This Harmony patch will only be patched if TileMutatorMechanics is added via XML to a mod using OptionalFeatures


    public static class VanillaExpandedFramework_StorytellerComp_Disease_MakeIntervalIncidents_Patch
    {

        public static IEnumerable<CodeInstruction> ModifyBiomeDiseaseMTB(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();


            var field = AccessTools.Field(typeof(BiomeDef), "diseaseMtbDays");
            var diseasemultiplier = AccessTools.Method(typeof(VanillaExpandedFramework_StorytellerComp_Disease_MakeIntervalIncidents_Patch), "MultiplyDiseaseMTB");



            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 1 && codes[i - 2].LoadsField(field) && codes[i - 1].opcode == OpCodes.Stloc_S && codes[i - 1].operand is LocalBuilder lb && lb.LocalIndex == 4)
                {

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, diseasemultiplier);
                    yield return new CodeInstruction(OpCodes.Stloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                }

                else yield return codes[i];
            }
        }


        public static float MultiplyDiseaseMTB(float diseaseMtbDays, Map map)
        {
           
            float multiplier = 1;
            if (map?.Tile.Tile?.Mutators != null)
            {
                foreach (TileMutatorDef mutator in map.Tile.Tile.Mutators)
                {
                    TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();

                    if (extension != null && extension.diseaseMTBMultiplier != 1)
                    {
                        multiplier *= extension.diseaseMTBMultiplier;
                    }

                }
            }
            
            return diseaseMtbDays * multiplier;
        }

    }
}