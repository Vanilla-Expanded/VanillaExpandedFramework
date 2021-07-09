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
                    LocalBuilder locVar = ilg.DeclareLocal(typeof(ValueTuple<int, int>));
                    if (instruction.Calls(AccessTools.Method(typeof(GenLocalDate), nameof(GenLocalDate.HourOfDay), new Type[] { typeof(Thing) })))
                    {
                        //Skip 3 instructions down to avoid mismatching local var with future builds
                        ///call | HourOfDay
                        ///stloc.s | V_4
                        ///ldloc.s | V_4
                        for (int j = 0; j < 3; j++)
                        {
                            yield return instruction;
                            instruction = instructionList[++i];
                        }
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_GetPriority), nameof(Patch_GetPriority.SleepHourFor)));
                        yield return new CodeInstruction(OpCodes.Stloc_S, locVar);
                    }

					if (instruction.opcode == OpCodes.Ldc_I4_7)
					{
                        yield return new CodeInstruction(OpCodes.Ldloc_S, locVar);
						yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ValueTuple<int, int>), nameof(ValueTuple<int, int>.Item1)));
                        instruction = instructionList[++i];
					}
					else if (instruction.opcode == OpCodes.Ldc_I4_S)
					{
                        yield return new CodeInstruction(OpCodes.Ldloc_S, locVar);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ValueTuple<int, int>), nameof(ValueTuple<int,int>.Item2)));
                        instruction = instructionList[++i];
                    }
                    yield return instruction;
                }
            }

            public static (int, int) SleepHourFor(Pawn pawn)
            {
                ExtendedRaceProperties extendedRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>();

                if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Crepuscular)
                {
                    return (3, 16);
                }
                else if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Nocturnal)
                {
                    return (9, 19);
                }
                return (7, 21);
            }
        }
    }

}
