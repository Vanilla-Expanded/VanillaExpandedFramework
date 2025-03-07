using System.Collections.Generic;
using System.Linq;

namespace VanillaCookingExpanded
{
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyDebug]
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestIngredientsInSet_NoMixHelper")]
    public static class WorkGiver_DoBill_TryFindBestIngredientsInSet_NoMixHelper_Patch
    {
        private static          bool              adjust;
        private static readonly HashSet<ThingDef> alreadyUsed = [];

        public static void Prefix(Bill bill)
        {
            Recipe_Extension extension = bill?.recipe?.GetModExtension<Recipe_Extension>();
            adjust = extension?.individualIngredients ?? false;
            alreadyUsed.Clear();
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
                    yield return CodeInstruction.LoadField(typeof(WorkGiver_DoBill_TryFindBestIngredientsInSet_NoMixHelper_Patch), nameof(alreadyUsed));
                    yield return CodeInstruction.LoadArgument(0);
                    yield return CodeInstruction.LoadLocal(7);
                    yield return CodeInstruction.Call(typeof(List<Thing>), "get_Item");
                    yield return CodeInstruction.LoadField(typeof(Thing), nameof(Thing.def));
                    yield return CodeInstruction.Call(typeof(HashSet<ThingDef>), nameof(HashSet<ThingDef>.Add));
                    yield return new CodeInstruction(OpCodes.Pop);

                }
                if(instruction.opcode == OpCodes.Bne_Un)
                {
                    yield return CodeInstruction.LoadField(typeof(WorkGiver_DoBill_TryFindBestIngredientsInSet_NoMixHelper_Patch), nameof(adjust));
                    yield return new CodeInstruction(OpCodes.Brfalse, skip);

                    yield return CodeInstruction.LoadField(typeof(WorkGiver_DoBill_TryFindBestIngredientsInSet_NoMixHelper_Patch), nameof(alreadyUsed));
                    yield return CodeInstruction.LoadArgument(0);
                    yield return CodeInstruction.LoadLocal(7);
                    yield return CodeInstruction.Call(typeof(List<Thing>), "get_Item");
                    
                    yield return CodeInstruction.LoadField(typeof(Thing), nameof(Thing.def));
                    yield return CodeInstruction.Call(typeof(HashSet<ThingDef>), nameof(HashSet<ThingDef>.Contains));
                    yield return new CodeInstruction(OpCodes.Brtrue, instruction.operand);
                    yield return new CodeInstruction(OpCodes.Nop) { labels = [skip] };
                }
            }
        }
    }
}
