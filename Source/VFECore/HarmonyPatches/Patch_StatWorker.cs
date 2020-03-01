using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_StatWorker
    {

        [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetExplanationUnfinalized))]
        public static class GetExplanationUnfinalized
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("StatWorker.GetExplanationUnfinalized transpiler start (1 match todo)");
                #endif


                var instructionList = instructions.ToList();

                var statInfo = AccessTools.Field(typeof(StatWorker), "stat");
                var equipmentInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.equipment));
                var getExplanationFromShieldInfo = AccessTools.Method(typeof(GetExplanationUnfinalized), nameof(GetExplanationFromShield));

                bool equipmentReferenced = false;
                bool done = false;

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (!done)
                    {
                        // Look for references to pawn.equipment
                        if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(equipmentInfo))
                            equipmentReferenced = true;

                        // Once that has been found, look for the next instruction that references stringBuilder that immediately succeeds a 'pop' instruction; this is where we modify stringBuilder
                        if (equipmentReferenced && instruction.opcode == OpCodes.Ldloc_0)
                        {
                            var prevInstruction = instructionList[i - 1];
                            if (prevInstruction.opcode == OpCodes.Pop)
                            {
                                #if DEBUG
                                    Log.Message("StatWorker.GetExplanationUnfinalized match 1 of 1");
                                #endif

                                yield return instruction; // stringBuilder
                                yield return new CodeInstruction(OpCodes.Ldloc_2); // pawn
                                yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                                yield return new CodeInstruction(OpCodes.Ldfld, statInfo); // this.stat
                                yield return new CodeInstruction(OpCodes.Call, getExplanationFromShieldInfo); // GetExplanationFromShield(stringBuilder, pawn, this.stat)
                                yield return new CodeInstruction(OpCodes.Stloc_0); // stringBuilder = GetExplanationFromShield(stringBuilder, pawn, this.stat)
                                instruction = instruction.Clone(); // stringBuilder
                                done = true;
                            }
                        }
                    }

                    yield return instruction;
                }
            }

            private static StringBuilder GetExplanationFromShield(StringBuilder explanationBuilder, Pawn pawn, StatDef stat)
            {
                if (pawn.equipment is Pawn_EquipmentTracker eq && eq.OffHandShield() is ThingWithComps shield && shield.GetComp<CompShield>().UsableNow)
                {
                    explanationBuilder.AppendLine(NonPublicMethods.StatWorker_InfoTextLineFromGear(shield, stat));
                }
                return explanationBuilder;
            }

        }

        [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
        public static class GetValueUnfinalized
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                #if DEBUG
                    Log.Message("StatWorker.GetValueUnfinalized transpiler start (1 match todo)");
                #endif


                var instructionList = instructions.ToList();

                var statInfo = AccessTools.Field(typeof(StatWorker), "stat");
                var equipmentInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.equipment));
                var storyInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.story));
                var applyStatOffsetFromShieldInfo = AccessTools.Method(typeof(GetValueUnfinalized), nameof(ApplyStatOffsetFromShield));

                bool equipmentReferenced = false;
                bool done = false;

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (!done)
                    {
                        // Look for references to pawn.equipment
                        if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(equipmentInfo))
                            equipmentReferenced = true;

                        // Once that has been found, look for the next instruction that loads 'pawn' and tries to reference pawn.story; this is where we modify num
                        if (equipmentReferenced && instruction.opcode == OpCodes.Ldloc_1)
                        {
                            var nextInstruction = instructionList[i + 1];
                            if (nextInstruction.opcode == OpCodes.Ldfld && nextInstruction.OperandIs(storyInfo))
                            {
                                #if DEBUG
                                    Log.Message("StatWorker.GetValueUnfinalized match 1 of 1");
                                #endif

                                yield return instruction; // pawn
                                yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                                yield return new CodeInstruction(OpCodes.Ldfld, statInfo); // this.stat
                                yield return new CodeInstruction(OpCodes.Ldloca_S, 0); // ref num
                                yield return new CodeInstruction(OpCodes.Call, applyStatOffsetFromShieldInfo); // ApplyStatOffsetFromShield(pawn, this.stat, ref num)
                                instruction = instruction.Clone(); // pawn
                                done = true;
                            }
                        }
                    }

                    yield return instruction;
                }
            }

            private static void ApplyStatOffsetFromShield(Pawn pawn, StatDef stat, ref float value)
            {
                if (pawn.equipment is Pawn_EquipmentTracker eq && eq.OffHandShield() is ThingWithComps shield && shield.GetComp<CompShield>().UsableNow)
                {
                    value += NonPublicMethods.StatWorker_StatOffsetFromGear(shield, stat);
                }
            }

        }

        [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.ShouldShowFor))]
        public static class ShouldShowFor
        {

            public static void Postfix(StatWorker __instance, StatDef ___stat, StatRequest req, ref bool __result)
            {
                // Stats with Apparel category can also show on shields
                if (!__result && !___stat.alwaysHide && req.Def is ThingDef tDef && (___stat.showIfUndefined || tDef.statBases.StatListContains(___stat))  && ___stat.category == StatCategoryDefOf.Apparel)
                    __result = tDef.IsShield();
            }

        }

    }

}
