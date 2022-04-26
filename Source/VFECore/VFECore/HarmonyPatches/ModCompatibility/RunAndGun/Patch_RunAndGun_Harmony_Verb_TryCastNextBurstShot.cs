using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_RunAndGun_Harmony_Verb_TryCastNextBurstShot
    {

        public static class manual_SetStanceRunAndGun
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("RunAndGun.Harmony.Verb_TryCastNextBurstShot.manual_SetStanceRunAndGun transpiler start (1 match todo)");
                #endif


                var instructionList = instructions.ToList();

                var shouldSetStanceInfo = AccessTools.Method(typeof(manual_SetStanceRunAndGun), nameof(ShouldSetStance));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Fix infinite shield bashing
                    if (instruction.opcode == OpCodes.Stloc_1)
                    {
                        #if DEBUG
                            Log.Message("RunAndGun.Harmony.Verb_TryCastNextBurstShot.manual_SetStanceRunAndGun match 1 of 1");
                        #endif


                        yield return instruction; // bool flag2 = stanceTracker.pawn.equipment.Primary == stance.verb.EquipmentSource || stance.verb.EquipmentSource == null;
                        yield return new CodeInstruction(OpCodes.Ldloc_1); // flag2
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // stanceTracker
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // stance
                        yield return new CodeInstruction(OpCodes.Call, shouldSetStanceInfo); // ShouldSetStance(flag2, stanceTracker, stance)
                        instruction = instruction.Clone(); // flag2 = ShouldSetStance(flag2, stanceTracker, stance)
                    }

                    yield return instruction;
                }
            }

            private static bool ShouldSetStance(bool original, Pawn_StanceTracker stanceTracker, Stance_Cooldown stance)
            {
                // Also factor in off-hand shield
                return original || stanceTracker.pawn.OffHandShield() == stance.verb.EquipmentSource;
            }

        }

    }

}
