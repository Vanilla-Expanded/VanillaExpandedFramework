namespace VFECore.Abilities
{
    using HarmonyLib;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    [HarmonyPatch(typeof(PawnFlyer), "MakeFlyer")]
    public static class PawnFlyer_MakeFlyer_Patch
    {
        static FieldInfo jobdef = AccessTools.Field(typeof(Job), nameof(Job.def));
        static FieldInfo castJump = AccessTools.Field(typeof(JobDefOf), nameof(JobDefOf.CastJump));
        static MethodInfo myMethod = AccessTools.Method(typeof(PawnFlyer_MakeFlyer_Patch), nameof(PawnFlyer_MakeFlyer_Patch.ShouldEndJob));
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(jobdef))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, myMethod);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i + 2].operand);

                }
                else if (codes[i].LoadsField(castJump) || codes[i].opcode == OpCodes.Bne_Un_S && codes[i - 1].LoadsField(castJump))
                {
                    //do nothing or nop
                }
                else if (codes[i].opcode == OpCodes.Stloc_0)
                {
                    // We check for Stloc_0, as the flyer is only assigned to it once in the code
                    yield return codes[i];
                    // Load the flyer
                    yield return CodeInstruction.LoadLocal(0);
                    // Load the pawn
                    yield return CodeInstruction.LoadArgument(1);
                    // Call our method
                    yield return CodeInstruction.Call(typeof(PawnFlyer_MakeFlyer_Patch), nameof(SetSelectOnSpawn));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
        public static bool ShouldEndJob(JobDef jobDef, ThingDef thingDef)
        {
            if (jobDef == JobDefOf.CastJump || typeof(AbilityPawnFlyer).IsAssignableFrom(thingDef.thingClass))
            {
                return true;
            }
            return false;
        }
        public static void SetSelectOnSpawn(PawnFlyer flyer, Pawn pawn)
        {
            if (flyer is AbilityPawnFlyer abilityFlyer && Find.Selector.IsSelected(pawn) && abilityFlyer.AutoSelectPawn(pawn))
                abilityFlyer.selectOnSpawn = true;
        }
    }
}
