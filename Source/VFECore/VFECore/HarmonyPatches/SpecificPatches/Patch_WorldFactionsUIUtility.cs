using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore
{
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld.Planet;

    [HarmonyPatch]
    public static class Patch_WorldFactionsUIUtility
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod() => 
            typeof(WorldFactionsUIUtility).GetNestedTypes(AccessTools.all).SelectMany(nestedType => 
                                                                                          AccessTools.GetDeclaredMethods(nestedType)).FirstOrDefault(declaredMethod => declaredMethod.Name.Contains("CanAddFaction"));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if(instruction.opcode == OpCodes.Ldc_I4_S && instruction.OperandIs(12))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 99);
                } else
                    yield return instruction;
            }
        }
    }
}
