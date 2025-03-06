using System.Collections.Generic;
using System.Linq;

namespace VanillaCookingExpanded
{
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestIngredientsInSet_NoMixHelper")]
    public static class WorkGiver_DoBill_TryFindBestIngredientsInSet_NoMixHelper_Patch
    {
        private static bool adjust;

        public static void Prefix(Bill bill)
        {
            Recipe_Extension extension = bill?.recipe?.GetModExtension<Recipe_Extension>();
            adjust = extension?.individualIngredients ?? false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo addToListInfo = AccessTools.Method(typeof(ThingCountUtility), nameof(ThingCountUtility.AddToList));

            Label skip = ilg.DefineLabel();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (instruction.Calls(addToListInfo))
                {
                    yield return CodeInstruction.LoadField(typeof(WorkGiver_DoBill_TryFindBestIngredientsInSet_NoMixHelper_Patch), nameof(adjust));
                    yield return new CodeInstruction(OpCodes.Brfalse, skip);
                    yield return CodeInstruction.LoadField(typeof(WorkGiver_DoBill), "availableCounts");
                    yield return CodeInstruction.LoadLocal(5);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    yield return CodeInstruction.Call(AccessTools.Inner(typeof(WorkGiver_DoBill), "DefCountList"), "SetCount");

                    yield return new CodeInstruction(OpCodes.Ldstr, "hey");
                    yield return new CodeInstruction(OpCodes.Call,  AccessTools.Method(typeof(Log), "Warning"));
                    yield return new CodeInstruction(OpCodes.Nop) { labels = [skip] };
                }
            }
        }
    }
}
