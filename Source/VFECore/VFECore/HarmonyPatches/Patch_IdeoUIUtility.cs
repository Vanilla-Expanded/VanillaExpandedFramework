using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore
{
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;

    [HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.FactionForRandomization))]
    public static class Patch_IdeoUIUtility
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            bool done = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!done && instruction.opcode == OpCodes.Ldloc_2)
                {
                    done = true;
                    yield return new CodeInstruction(instruction);
                    yield return CodeInstruction.LoadField(typeof(Faction), nameof(Faction.def));
                    yield return CodeInstruction.LoadField(typeof(FactionDef), nameof(FactionDef.humanlikeFaction));
                    yield return new CodeInstruction(instructionList[i + 4]);
                }

                yield return instruction;
            }
        }
    }
}
