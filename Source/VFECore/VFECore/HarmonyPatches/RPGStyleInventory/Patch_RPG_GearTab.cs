using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{
    public static class Patch_RPG_GearTab
    {
        public static Type DetailedRPGGearTab;
        public static Type DetailedRPGGearTabRevamped;

        public static IEnumerable<CodeInstruction> TryDrawOverallArmor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            var getApparel = AccessTools.Method(typeof(List<Apparel>), "get_Item", new[] {typeof(int)});
            var getBodyPart = AccessTools.Method(typeof(List<BodyPartRecord>), "get_Item", new[] {typeof(int)});
            var fromApparel = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromApparel");
            var fromEquipment = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromEquipment");
            var getPawn = AccessTools.PropertyGetter(DetailedRPGGearTab, "SelPawnForGear");

            var idx0 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 6);
            var idx1 = list.FindIndex(idx0 + 1, ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 6);
            var labels1 = list[idx1 + 2].ExtractLabels();

            list.InsertRange(idx1 + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldloca_S, 6).WithLabels(labels1),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_S, 8),
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


        public static IEnumerable<CodeInstruction> TryDrawOverallArmor1_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            var getApparel = AccessTools.Method(typeof(List<Apparel>), "get_Item", new[] {typeof(int)});
            var getBodyPart = AccessTools.Method(typeof(List<BodyPartRecord>), "get_Item", new[] {typeof(int)});
            var fromApparel = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromApparel");
            var fromEquipment = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromEquipment");
            var getPawn = AccessTools.PropertyGetter(DetailedRPGGearTab, "SelPawnForGear");

            var idx0 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 7);
            var idx1 = list.FindIndex(idx0 + 1, ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 7);
            var labels1 = list[idx1 + 1].ExtractLabels();

            list.InsertRange(idx1 + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldloca_S, 6).WithLabels(labels1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Callvirt, getApparel),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_S, 6),
                new CodeInstruction(OpCodes.Callvirt, getBodyPart),
                new CodeInstruction(OpCodes.Call, fromApparel)
            });

            var idx2 = list.FindIndex(ins => ins.opcode == OpCodes.Ldloc_0);
            var labels2 = list[idx2].ExtractLabels();

            list.InsertRange(idx2, new[]
            {
                new CodeInstruction(OpCodes.Ldloca_S, 6).WithLabels(labels2),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_S, 6),
                new CodeInstruction(OpCodes.Callvirt, getBodyPart),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, getPawn),
                new CodeInstruction(OpCodes.Call, fromEquipment)
            });

            return list;
        }

        public static IEnumerable<CodeInstruction> TryDrawOverallArmor_Revamped_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            var getApparel = AccessTools.Method(typeof(List<Apparel>), "get_Item", new[] {typeof(int)});
            var getBodyPart = AccessTools.Method(typeof(List<BodyPartRecord>), "get_Item", new[] {typeof(int)});
            var fromApparel = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromApparel");
            var fromEquipment = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromEquipment");
            var getPawn = AccessTools.PropertyGetter(DetailedRPGGearTabRevamped, "SelPawnForGear");

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


        public static IEnumerable<CodeInstruction> TryDrawOverallArmor1_Revamped_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            var getApparel = AccessTools.Method(typeof(List<Apparel>), "get_Item", new[] {typeof(int)});
            var getBodyPart = AccessTools.Method(typeof(List<BodyPartRecord>), "get_Item", new[] {typeof(int)});
            var fromApparel = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromApparel");
            var fromEquipment = AccessTools.Method(typeof(Patch_ITab_Pawn_Gear.TryDrawOverallArmor), "ShieldFromEquipment");
            var getPawn = AccessTools.PropertyGetter(DetailedRPGGearTabRevamped, "SelPawnForGear");
            var stat = list.Find(ins => ins.opcode == OpCodes.Ldarg_S).operand;

            var idx0 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 6);
            var idx1 = list.FindIndex(idx0 + 1, ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalBuilder lb && lb.LocalIndex == 6);
            var labels1 = list[idx1 + 1].ExtractLabels();

            list.InsertRange(idx1 + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldloca_S, 6).WithLabels(labels1),
                new CodeInstruction(OpCodes.Ldarg_S, stat),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldloc_S, 8),
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
                new CodeInstruction(OpCodes.Ldarg_S, stat),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                new CodeInstruction(OpCodes.Callvirt, getBodyPart),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, getPawn),
                new CodeInstruction(OpCodes.Call, fromEquipment)
            });

            return list;
        }
    }
}