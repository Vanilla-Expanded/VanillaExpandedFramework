using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Comps;
using RimWorld;
using Verse;

namespace MVCF.Harmony
{
    public class ExtraEquipment
    {
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(ThingDef), "get_IsRangedWeapon"),
                new HarmonyMethod(typeof(ExtraEquipment), nameof(Prefix_IsRangedWeapon)));
            harm.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddDraftedOrders"),
                transpiler: new HarmonyMethod(typeof(ExtraEquipment), nameof(CheckForMelee)));
        }

        public static IEnumerable<CodeInstruction> CheckForMelee(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var idx = list.FindIndex(ins => ins.opcode == OpCodes.Brtrue);
            var label = list[idx].operand;
            list.InsertRange(idx + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), "equipment")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_EquipmentTracker), "get_Primary")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VerbManager), "PreferMelee")),
                new CodeInstruction(OpCodes.Brtrue, label)
            });
            return list;
        }

        public static bool Prefix_IsRangedWeapon(ref bool __result, ThingDef __instance)
        {
            if (__instance.IsWeapon &&
                __instance.GetCompProperties<CompProperties_VerbProps>() is CompProperties_VerbProps props &&
                props.ConsiderMelee)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}