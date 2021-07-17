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

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                
                for (int i = 0; i < instructionList.Count; i++)
                {
                    CodeInstruction instruction = instructionList[i];
                    LocalBuilder locVar = ilg.DeclareLocal(typeof(bool));
                    if (instruction.Calls(AccessTools.Method(typeof(GenLocalDate), nameof(GenLocalDate.HourOfDay), new Type[] { typeof(Thing) })))
                    {
                        while (instruction.opcode != OpCodes.Ldloc_S)
						{
                            yield return instruction;
                            instruction = instructionList[++i];
						}
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_GetPriority), nameof(SleepHourFor)));
						int j = 1;
						while (true)
						{
							if (instructionList[i + j].opcode == OpCodes.Blt_S)
							{
								instruction = new CodeInstruction(OpCodes.Brfalse, instructionList[i + j].operand);
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
                ExtendedRaceProperties extendedRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>();

                if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Crepuscular)
                {
                    return hour > 3 && hour < 16;
                }
                else if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Nocturnal)
                {
                    return hour > 9 && hour < 19;
                }
                return hour < 7 && hour > 21;
            }
        }
    }

}
