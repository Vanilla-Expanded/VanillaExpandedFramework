using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse.AI.Group;

namespace NocturnalAnimals
{
    public static class JobGiver_GetRest_Patch
    {

        [HarmonyPatch(typeof(JobGiver_GetRest))]
        [HarmonyPatch(nameof(JobGiver_GetRest.GetPriority))]
        public static class VanillaExpandedFramework_JobGiver_GetRest_GetPriority_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                bool found = false;
                var label = ilg.DefineLabel();
                var curLevelGetter = AccessTools.PropertyGetter(typeof(Need), "CurLevel");
                var shouldOverride = AccessTools.Method(typeof(VanillaExpandedFramework_JobGiver_GetRest_GetPriority_Patch), "ShouldOverride");
                var sleepHourFor = AccessTools.Method(typeof(VanillaExpandedFramework_JobGiver_GetRest_GetPriority_Patch), "TimeAssignmentFor");
                for (int i = 0; i < instructionList.Count; i++)
                {
                    CodeInstruction instruction = instructionList[i];
                    if (!found && instruction.Calls(curLevelGetter))
                    {
                        found = true;
                        instructionList[i].labels.Add(label);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, shouldOverride);
                        yield return new CodeInstruction(OpCodes.Brfalse, label);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, sleepHourFor);
                        yield return new CodeInstruction(OpCodes.Stloc_2);
                    }
                    yield return instruction;
                }
            }

            public static TimeAssignmentDef TimeAssignmentFor(Pawn pawn)
            {
                int hour = GenLocalDate.HourOfDay(pawn);
                ExtendedRaceProperties extendedRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>();
                if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Crepuscular)
                {
                    return hour > 3 && hour < 16 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                }
                else if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Nocturnal)
                {
                    return hour > 9 && hour < 19 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                }
                return ((hour >= 7 && hour <= 21) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep);
            }
            public static bool ShouldOverride(Pawn pawn)
            {
                return pawn.def.GetModExtension<ExtendedRaceProperties>() != null;
            }
        }
    }
}
