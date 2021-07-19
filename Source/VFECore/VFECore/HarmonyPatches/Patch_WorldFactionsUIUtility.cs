using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    using System.Reflection.Emit;

    [StaticConstructorOnStartup]
    public static class UIUtilityData
    {
        public static Dictionary<FactionDef, int> factionCounts = new Dictionary<FactionDef, int>();
    }

    [HarmonyPatch(typeof(WorldFactionsUIUtility), "DoWindowContents")]
    public static class Patch_WorldFactionsUIUtility
    {
        [HarmonyPostfix]
        public static void Postfix(ref Dictionary<FactionDef, int> factionCounts)
        {
            foreach (var item in factionCounts)
            {
                UIUtilityData.factionCounts.SetOrAdd(item.Key, item.Value);
            }
        }
    }

    [HarmonyPatch(typeof(WorldFactionsUIUtility), nameof(WorldFactionsUIUtility.DoRow))]
    public static class Patch_WorldFactionsUIUtility_DoRow
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_2)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                }
                
                yield return instruction;
            }
        }
    }
}