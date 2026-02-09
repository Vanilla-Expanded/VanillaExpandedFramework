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
using UnityEngine;

namespace VEF.Buildings
{
    [HarmonyPatch(typeof(CompBreakdownable), nameof(CompBreakdownable.CheckForBreakdown))]
    public static class VanillaExpandedFramework_CompBreakdownable_CheckForBreakdown_Patch
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var check = AccessTools.Method(typeof(VanillaExpandedFramework_CompBreakdownable_CheckForBreakdown_Patch), "AdjustMTB");



            for (var i = 0; i < codes.Count; i++)
            {
               
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float f &&
    Mathf.Abs(f - 13680000f) < 0.01f)
                {

                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, check);




                }

                else yield return codes[i];
            }
        }



        public static float AdjustMTB(float baseline,CompBreakdownable comp)
        {
           
            return baseline / comp.parent.GetStatValue(InternalDefOf.VEF_BuildingBreakdownFactor);

        }

    }
}