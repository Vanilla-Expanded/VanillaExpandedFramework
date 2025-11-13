using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using VEF.Weapons;
using System.Linq;


namespace VEF.Plants
{



    [HarmonyPatch(typeof(PlantUtility))]
    [HarmonyPatch("CanSowOnGrower")]
    public static class VanillaExpandedFramework_PlantUtility_CanSowOnGrower_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();

            var detectNewTags = AccessTools.Method(typeof(VanillaExpandedFramework_PlantUtility_CanSowOnGrower_Patch), "DetectTags");

            yield return codes[0];

            for (var i = 1; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Callvirt && codes[i-1].opcode != OpCodes.Ldstr
                    )
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);

                    yield return new CodeInstruction(OpCodes.Call, detectNewTags);

                }

                else yield return codes[i];
            }
        }


        public static bool DetectTags(List<string> sowTags, string sowTag, Thing sower)
        {
            SowerExtension extension = sower?.def.GetModExtension<SowerExtension>();
            if(extension != null)
            {
                if (sowTags.Contains(sowTag))
                {
                    return true;
                }
                else
                {
                    return sowTags.Intersect(extension.extraSowTags).Any();
 

                }

            }
            
            return sowTags.Contains(sowTag);
        }

    }


}











