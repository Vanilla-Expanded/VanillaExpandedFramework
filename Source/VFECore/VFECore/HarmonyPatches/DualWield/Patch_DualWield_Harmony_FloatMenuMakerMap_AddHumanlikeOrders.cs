using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_DualWield_Harmony_FloatMenuMakerMap_AddHumanlikeOrders
    {

        public static class manual_Postfix
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders.manual_Postfix transpiler start (1 match todo)");
                #endif


                var instructionList = instructions.ToList();

                var getItemInfo = AccessTools.Method(typeof(List<Thing>), "get_Item");
                var eligibleForDualWieldOptionInfo = AccessTools.Method(typeof(manual_Postfix), nameof(EligibleForDualWieldOption));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder lb && lb.LocalIndex == 12)
                    {
                        #if DEBUG
                            Log.Message("DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders.manual_Postfix match 1 of 1");
                        #endif


                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 10); // thingList
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 11); // i
                        yield return new CodeInstruction(OpCodes.Callvirt, getItemInfo); // thingList[i]
                        instruction = new CodeInstruction(OpCodes.Call, eligibleForDualWieldOptionInfo); // EligibleForDualWieldOption(flag4, thingList[i])
                    }

                    yield return instruction;
                }
            }

            private static bool EligibleForDualWieldOption(bool result, Thing thing)
            {
                if (result && thing.def.IsShield())
                    return false;
                return result;
            }

        }

    }

}
