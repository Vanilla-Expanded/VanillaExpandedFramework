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


    public static class VanillaExpandedFramework_CompDeepDrill_TryProducePortion_Patch
    {
      
        public static IEnumerable<CodeInstruction> ModifyDeepDrillOutput(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

            var mathmax = AccessTools.Method(typeof(Mathf), "Max", new Type[] { typeof(int), typeof(int) });
            var deepdrillmultiplier = AccessTools.Method(typeof(VanillaExpandedFramework_CompDeepDrill_TryProducePortion_Patch), "MultiplyDrillOutput");



            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 1 && codes[i - 2].Calls(mathmax) && codes[i - 1].opcode == OpCodes.Stloc_S && codes[i - 1].operand is LocalBuilder lb && lb.LocalIndex == 5)
                {

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, deepdrillmultiplier);
                    yield return new CodeInstruction(OpCodes.Stloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                }

                else yield return codes[i];
            }
        }


        public static int MultiplyDrillOutput(int stackcount, CompDeepDrill comp)
        {
            Map map = comp.parent.Map;
            float multiplier = 1;
            foreach (TileMutatorDef mutator in map.Tile.Tile.Mutators)
            {
                TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();

                if (extension != null && extension.deepOresMultiplier != 1)
                {
                    multiplier *= extension.deepOresMultiplier;
                }

            }
            return (int)(stackcount * multiplier);
        }

    }
}