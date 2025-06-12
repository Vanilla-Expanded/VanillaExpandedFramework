using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace VEF.Cooking
{
    using System.Reflection;
    using System.Reflection.Emit;
    using Verse;

    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredientsInSet_AllowMix")]
    public static class VanillaExpandedFramework_WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_Patch
    {
        private static bool adjust;
        private static readonly HashSet<ThingDef> alreadyUsed = [];

        public static void Prefix(Bill bill, List<Thing> availableThings)
        {
            Recipe_Extension extension = bill?.recipe?.GetModExtension<Recipe_Extension>();
            adjust = extension?.individualIngredients ?? false;
            alreadyUsed.Clear();
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo addToListInfo = AccessTools.Method(typeof(ThingCountUtility), nameof(ThingCountUtility.AddToList));
            MethodInfo isFixedInfo = AccessTools.PropertyGetter(typeof(IngredientCount), nameof(IngredientCount.IsFixedIngredient));

            Label skip = ilg.DefineLabel();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if(instruction.Calls(isFixedInfo))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return CodeInstruction.LoadField(typeof(VanillaExpandedFramework_WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_Patch), nameof(adjust));
                    yield return new CodeInstruction(OpCodes.Brfalse, skip);
                    yield return CodeInstruction.LoadField(typeof(VanillaExpandedFramework_WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_Patch), nameof(alreadyUsed));
                    yield return CodeInstruction.LoadLocal(5);
                    yield return CodeInstruction.LoadField(typeof(Thing), nameof(Thing.def));
                    yield return CodeInstruction.Call(typeof(HashSet<ThingDef>), nameof(HashSet<ThingDef>.Contains));
                    yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i-2].operand);
                    yield return new CodeInstruction(instructionList[i - 1]).WithLabels(skip);
                }

                yield return instruction;
                if (instruction.Calls(addToListInfo))
                {
                    yield return CodeInstruction.LoadField(typeof(VanillaExpandedFramework_WorkGiver_DoBill_TryFindBestBillIngredientsInSet_AllowMix_Patch), nameof(alreadyUsed));
                    yield return CodeInstruction.LoadLocal(5);
                    yield return CodeInstruction.LoadField(typeof(Thing), nameof(Thing.def));
                    yield return CodeInstruction.Call(typeof(HashSet<ThingDef>), nameof(HashSet<ThingDef>.Add));
                    yield return new CodeInstruction(OpCodes.Pop);
                }
            }
        }
    }
}
