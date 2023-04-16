using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System;
using UnityEngine;


/// Ideo ExposeData automatically adds any hidden preceptdef thats not part of the ideo already
/// it does so ignoring any foundation can add rules, or can generate as special rules.
/// This patch adds those checks to prevent precepts that shouldnt be added from being added.
namespace VanillaMemesExpanded
{
    [HarmonyPatch(typeof(Ideo))]
    [HarmonyPatch("ExposeData")]
    public static class VanillaMemesExpanded_Ideo_ExposeData_Patch
    {
        static MethodInfo AddPrecept = AccessTools.Method(typeof(Ideo), "AddPrecept");
        static MethodInfo DebugLog = AccessTools.Method(typeof(Log), "Warning", new Type[] { typeof(string) });
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            bool found = false;
            int debugCall = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                //Local builder 5 is where it stores the Ritual Pattern Def. 1.4 added an additional place AddPrecept is used so this became necesarry
                if (!found && codes[i].Calls(AddPrecept) && codes[i - 1].opcode == OpCodes.Ldloc_S && codes[i - 1].operand is LocalBuilder lb && lb.LocalIndex == 5)
                {
                    found = true;
                    codes[i].opcode = OpCodes.Nop;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VanillaMemesExpanded_Ideo_ExposeData_Patch), nameof(CheckIfCanAdd)));
                    //Noping the next set to remove debug log as long its where I expect
                    if (codes[i + 7].Calls(DebugLog))
                    {
                        debugCall = i + 7;
                    }
                }
                if (found && i <= debugCall)
                {
                    codes[i].opcode = OpCodes.Nop;
                }
            }
            if (!found)
            {
                Log.Warning("VanillaMemesExpanded Transpiler on Ideo:ExposeData could not find hook");
            }
        }
        public static void CheckIfCanAdd(Ideo ideo, Precept precept, bool init = false, FactionDef generatingFor = null, RitualPatternDef ritualPatternBase = null)
        {
            // ideo.foundation will be null if the user does not have Ideology
            if (ideo.foundation == null || ideo.foundation.CanAdd(precept.def))//Can Add filters things that shouldnt be added for memes and such
            {
                if (precept.def.canGenerateAsSpecialPrecept)
                {
                    ideo.AddPrecept(precept, true, null, ritualPatternBase);
                    Debug.LogWarning("A hidden ritual precept was missing, adding: " + precept.def.LabelCap);
                }
            }
        }
    }
}
