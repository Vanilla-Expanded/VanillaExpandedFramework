using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Utils;
using MVCF.Utilities;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_RunAndGun : PatchSet
{
    public static Func<ThingWithComps, bool> IsOffHand;

    public override IEnumerable<Patch> GetPatches()
    {
        var type = AccessTools.TypeByName("DualWield.Ext_ThingWithComps");
        if (type is not null) IsOffHand ??= AccessTools.Method(type, "IsOffHand")?.CreateDelegate<Func<ThingWithComps, bool>>();
        yield return Patch.Transpiler(AccessTools.Method(AccessTools.TypeByName("RunAndGun.Harmony.Verb_TryCastNextBurstShot"), "SetStanceRunAndGun"),
            AccessTools.Method(GetType(), nameof(RunAndGunSetStance)));
        yield return Patch.Postfix(AccessTools.TypeByName("RunAndGun.Extensions")?.GetMethod("HasRangedWeapon", AccessTools.all),
            AccessTools.Method(GetType(), nameof(RunAndGunHasRangedWeapon)));
    }

    public static IEnumerable<CodeInstruction> RunAndGunSetStance(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();
        var idx1 = list.FindIndex(ins => ins.IsLdarg(0));
        var idx2 = list.FindIndex(ins => ins.opcode == OpCodes.Ldfld && (FieldInfo)ins.operand ==
            AccessTools.Field(typeof(Pawn_StanceTracker), "curStance"));
        var label = list.Find(ins => ins.opcode == OpCodes.Br).operand;
        list.RemoveRange(idx1, idx2 - idx1 - 2);
        var idx3 = list.FindIndex(ins => ins.IsLdarg(0));
        var list2 = new List<CodeInstruction>
        {
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_StanceTracker), "pawn")),
            new(OpCodes.Ldarg_1),
            new(OpCodes.Ldfld, AccessTools.Field(typeof(Stance_Busy), "verb")),
            new(OpCodes.Call, AccessTools.Method(typeof(PatchSet_RunAndGun), nameof(CanRunAndGun))),
            new(OpCodes.Brfalse_S, label)
        };
        list.InsertRange(idx3 - 1, list2);
        return list;
    }

    public static bool CanRunAndGun(Pawn pawn, Verb verb)
    {
        if (verb.EquipmentSource == null) return true;
        if (IsOffHand == null) return true;
        return !IsOffHand(verb.EquipmentSource);
    }

    // ReSharper disable once InconsistentNaming
    public static void RunAndGunHasRangedWeapon(Pawn instance, ref bool __result)
    {
        if (!__result) __result = instance.Manager().CurrentlyUseableRangedVerbs.Any();
    }
}
