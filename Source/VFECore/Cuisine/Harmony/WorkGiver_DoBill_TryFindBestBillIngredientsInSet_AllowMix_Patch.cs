using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace VanillaCookingExpanded
{
    using System.Reflection;
    using System.Reflection.Emit;
    using Verse;

    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet_AllowMix")]
    public static class WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_Patch
    {
        private static bool adjust;

        public static void Prefix(Bill bill)
        {
            Recipe_Extension extension = bill?.recipe?.GetModExtension<Recipe_Extension>();
            adjust = extension?.individualIngredients ?? false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo addToListInfo = AccessTools.Method(typeof(ThingCountUtility), nameof(ThingCountUtility.AddToList));


            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (instruction.Calls(addToListInfo))
                {
                    yield return CodeInstruction.LoadArgument(0);
                    yield return CodeInstruction.LoadLocal(5);
                    yield return CodeInstruction.Call(typeof(WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_Patch), nameof(MixHelper));
                }
            }
        }

        public static void MixHelper(List<Thing> available, Thing th)
        {
            if(adjust)
                available.RemoveAll(t => t.def == th.def);
        }
    }
}
