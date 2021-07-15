using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    public static class Patch_ITab_Pawn_Gear
    {
        [HarmonyPatch(typeof(ITab_Pawn_Gear), "TryDrawOverallArmor")]
        public static class TryDrawOverallArmor
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();

                var getApparel = AccessTools.Method(typeof(List<Apparel>), "get_Item", new[] {typeof(int)});
                var getBodyPart = AccessTools.Method(typeof(List<BodyPartRecord>), "get_Item", new[] {typeof(int)});
                var fromApparel = AccessTools.Method(typeof(TryDrawOverallArmor), nameof(ShieldFromApparel));
                var fromEquipment = AccessTools.Method(typeof(TryDrawOverallArmor), nameof(ShieldFromEquipment));
                var getPawn = AccessTools.PropertyGetter(typeof(ITab_Pawn_Gear), "SelPawnForGear");

                var idx0 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 6);
                var idx1 = list.FindIndex(idx0 + 1, ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 6);
                var labels1 = list[idx1 + 1].ExtractLabels();

                list.InsertRange(idx1 + 1, new[]
                {
                    new CodeInstruction(OpCodes.Ldloca_S, 6).WithLabels(labels1),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldloc_S, 7),
                    new CodeInstruction(OpCodes.Callvirt, getApparel),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldloc_S, 5),
                    new CodeInstruction(OpCodes.Callvirt, getBodyPart),
                    new CodeInstruction(OpCodes.Call, fromApparel)
                });

                var idx2 = list.FindIndex(ins => ins.opcode == OpCodes.Ldloc_0);
                var labels2 = list[idx2].ExtractLabels();

                list.InsertRange(idx2, new[]
                {
                    new CodeInstruction(OpCodes.Ldloca_S, 6).WithLabels(labels2),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldloc_S, 5),
                    new CodeInstruction(OpCodes.Callvirt, getBodyPart),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, getPawn),
                    new CodeInstruction(OpCodes.Call, fromEquipment)
                });

                return list;
            }

            public static void ShieldFromApparel(ref float armourImportance, StatDef stat, Apparel apparel, BodyPartRecord part)
            {
                if (apparel.IsShield(out var shieldComp) && shieldComp.UsableNow &&
                    shieldComp.CoversBodyPart(part))
                {
                    var shieldRating = Mathf.Clamp01(apparel.GetStatValue(stat) / 2);
                    armourImportance *= 1 - shieldRating;
                }
            }

            public static void ShieldFromEquipment(ref float armourImportance, StatDef stat, BodyPartRecord part, Pawn pawn)
            {
                if (pawn.equipment == null) return;
                foreach (var eq in pawn.equipment.AllEquipmentListForReading)
                    if (eq.IsShield(out var shieldComp) && shieldComp.UsableNow &&
                        shieldComp.CoversBodyPart(part))
                    {
                        var shieldRating = Mathf.Clamp01(eq.GetStatValue(stat) / 2);
                        armourImportance *= 1 - shieldRating;
                    }
            }
        }
    }
}