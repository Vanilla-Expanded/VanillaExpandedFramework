using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace PipeSystem
{

    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
    public class PipeSystem_Thing_SpawnSetup_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool done = false;
            FieldInfo stackCountInfo = AccessTools.Field(typeof(Thing), nameof(Thing.stackCount));


            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!done && instruction.OperandIs(stackCountInfo))
                {
                    i += 3;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    instruction = CodeInstruction.Call(typeof(PipeSystem_Thing_SpawnSetup_Patch), nameof(SpawnSetupHelper));
                    instructionList[i + 1].opcode = OpCodes.Brtrue_S;
                    done = true;
                }

                yield return instruction;
            }
        }

        public static bool SpawnSetupHelper(Thing thing, Map map)
        {
            return true;
            //if (thing.stackCount > thing.def.stackLimit)
            //{
            //    Log.Message(string.Join(" | ", thing.Position.GetThingList(map).Select(t => t.def.label)));
            //    return false;
            //}
            //return true;
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.PostMapInit))]
    public class PipeSystem_Thing_PostMapInit_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Thing __instance)
        {
            if (__instance.stackCount > __instance.def.stackLimit && !__instance.Position.GetThingList(__instance.Map).Any(t => t.def.GetModExtension<FactoryHopperExtension>()?.isfactoryHopper==true))
            {
                Log.Error(string.Concat("Spawned ", __instance, " with stackCount ", __instance.stackCount, " but stackLimit is ", __instance.def.stackLimit, ". Truncating."));
                __instance.stackCount = __instance.def.stackLimit;
            }
        }
    }
}
