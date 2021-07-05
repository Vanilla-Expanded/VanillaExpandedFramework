using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NocturnalAnimals
{

    public static class Patch_JobGiver_GetRest
    {

        [HarmonyPatch(typeof(JobGiver_GetRest))]
        [HarmonyPatch(nameof(JobGiver_GetRest.GetPriority))]
        public static class Patch_GetPriority
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> instructionList = instructions.ToList();

                MethodInfo hourOfDayInfo = AccessTools.Method(typeof(GenLocalDate), nameof(GenLocalDate.HourOfDay), new []{typeof(Thing)});

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Effectively turn 'if (num < 7 || num > 21)' into 'if (SleepHourFor(num, pawn))'
                    if (instruction.opcode == OpCodes.Stloc_S && instructionList[i-1].Calls(hourOfDayInfo))
                    {
                        yield return instruction;                                                                                            // int num = GenLocalDate.HourOfDay(pawn)
                        yield return new CodeInstruction(OpCodes.Ldloc_S, instruction.operand);                                              // num
                        yield return new CodeInstruction(OpCodes.Ldarg_1);                                                                   // pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_GetPriority), nameof(SleepHourFor))); // SleepHourFor(num, pawn)

                        int j = 1;
                        while (true)
                        {
                            if (instructionList[i + j].opcode == OpCodes.Blt_S)
                            {
                                instruction            = new CodeInstruction(OpCodes.Brfalse, instructionList[i + j].operand);
                                instructionList[i + j] = new CodeInstruction(OpCodes.Nop);
                                break;
                            }

                            instructionList[i + j] = new CodeInstruction(OpCodes.Nop);
                            j++;
                        }
                    }

                    yield return instruction;

                }


            }

            public static bool SleepHourFor(int hour, Pawn pawn)
            {

                var extendedRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>();


                if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Crepuscular)
                {
                    return hour > 3 && hour < 16;
                }
                else if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Nocturnal)
                {
                    return hour > 9 && hour < 19;
                }
                else
                    return hour < 7 || hour > 21;
            }

        }

    }

}
