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
using Harmony;

namespace VFECore
{

    public static class Patch_DualWield_Harmony_FloatMenuMakerMap_AddHumanlikeOrders
    {

        public static class manual_Postfix
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var getItemInfo = AccessTools.Method(typeof(List<Thing>), "get_Item");
                var eligibleForDualWieldOptionInfo = AccessTools.Method(typeof(manual_Postfix), nameof(EligibleForDualWieldOption));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder lb && lb.LocalIndex == 12)
                    {
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
