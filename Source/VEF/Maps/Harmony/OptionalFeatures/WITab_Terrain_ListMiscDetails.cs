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


    public static class VanillaExpandedFramework_WITab_Terrain_ListMiscDetails_Patch
    {

        public static IEnumerable<CodeInstruction> CorrectlyOutputBiomeDiseaseMTB(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();


            var field = AccessTools.Field(typeof(BiomeDef), "diseaseMtbDays");
            var diseasemultiplier = AccessTools.Method(typeof(VanillaExpandedFramework_WITab_Terrain_ListMiscDetails_Patch), "MultiplyDiseaseMTB");
            var getPrimaryBiome = AccessTools.PropertyGetter(typeof(Tile), "PrimaryBiome");


            int position = -1;
            for (var i = 0; i < codes.Count; i++)
            {

                if ( codes[i].opcode == OpCodes.Ldarg_1)
                {
                    position = i;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, getPrimaryBiome);
                    yield return new CodeInstruction(OpCodes.Ldfld, field);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, diseasemultiplier);

                }
                else if(position!=-1&&position + 1== i)
                {
                    yield return new CodeInstruction(OpCodes.Nop);
                }
                else if (position != -1 && position +2 == i)
                {
                    yield return new CodeInstruction(OpCodes.Nop);
                }

                else yield return codes[i];
            }
        }


        public static float MultiplyDiseaseMTB(float diseaseMtbDays, Tile ws)
        {

            float multiplier = 1;
            foreach (TileMutatorDef mutator in ws.Mutators)
            {
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();

                if (extension != null && extension.diseaseMTBMultiplier != 1)
                {
                    multiplier *= extension.diseaseMTBMultiplier;
                }

            }
            return diseaseMtbDays * multiplier;
        }

    }
}