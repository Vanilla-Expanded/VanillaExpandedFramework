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


    public static class VanillaExpandedFramework_CompDeepScanner_DoFind_Patch
    {
      
        public static IEnumerable<CodeInstruction> ModifyDeepResourceNumbers(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

            var field = AccessTools.Field(typeof(ThingDef), "deepCountPerCell");
            var deepresourcemultiplier = AccessTools.Method(typeof(VanillaExpandedFramework_CompDeepScanner_DoFind_Patch), "MultiplyDeepResourceNumbers");

            for (var i = 0; i < codes.Count; i++)
            {

                if (i>0 && codes[i-1].opcode == OpCodes.Ldloc_2 && codes[i].LoadsField(field))
                {
                    
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, deepresourcemultiplier);
                   
                }

                else yield return codes[i];
            }
        }


        public static int MultiplyDeepResourceNumbers(int deepCountPerCell, CompDeepScanner comp)
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
            return (int)(deepCountPerCell * multiplier);
        }

    }
}