using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.Features;

public class Feature_VerbComps : Feature_Humanoid
{
    public override string Name => "VerbComps";

    public override IEnumerable<Patch> GetPatches()
    {
        foreach (var patch in base.GetPatches()) yield return patch;

        var info1 = AccessTools.Method(GetType(), nameof(Available_Postfix));
        yield return Patch.Postfix(AccessTools.Method(typeof(Verb), nameof(Verb.Available)), info1);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var type in typeof(Verb).AllSubclasses())
        {
            var method = AccessTools.Method(type, nameof(Verb.Available));
            if (method is not null && method.IsDeclaredMember()) yield return Patch.Postfix(method, info1);
        }

        var info2 = AccessTools.Method(GetType(), nameof(Projectile_Postfix));
        yield return Patch.Postfix(AccessTools.PropertyGetter(typeof(Verb_LaunchProjectile), nameof(Verb_LaunchProjectile.Projectile)), info2);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var type in typeof(Verb_LaunchProjectile).AllSubclasses())
        {
            var method = AccessTools.PropertyGetter(type, nameof(Verb_LaunchProjectile.Projectile));
            if (method is not null && method.IsDeclaredMember()) yield return Patch.Postfix(method, info2);
        }

        yield return Patch.Transpiler(AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"),
            AccessTools.Method(GetType(), nameof(TryCastNextBurstShot_Transpiler)));
    }

    public static void Available_Postfix(Verb __instance, ref bool __result)
    {
        if (__result && __instance.Managed(false) is { } mv) __result = mv.Available();
    }

    public static void Projectile_Postfix(Verb_LaunchProjectile __instance, ref ThingDef __result)
    {
        if (__instance.Managed(false) is { } mv) mv.ModifyProjectile(ref __result);
    }

    public static IEnumerable<CodeInstruction> TryCastNextBurstShot_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var list = instructions.ToList();
        var info = AccessTools.Method(typeof(Verb), nameof(Verb.Available));
        var label3 = (Label)list[list.FindIndex(ins => ins.Calls(info)) + 1].operand;
        var idx4 = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_0);
        var label4 = generator.DefineLabel();
        var label5 = generator.DefineLabel();
        list.InsertRange(idx4 + 1, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldc_I4_0),
            CodeInstruction.Call(typeof(ManagedVerbUtility), nameof(ManagedVerbUtility.Managed)),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Brfalse, label4),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ManagedVerb), nameof(ManagedVerb.PreCastShot))),
            new CodeInstruction(OpCodes.Brfalse, label3),
            new CodeInstruction(OpCodes.Br, label5),
            new CodeInstruction(OpCodes.Pop).WithLabels(label4),
            new CodeInstruction(OpCodes.Nop).WithLabels(label5)
        });
        var info2 = AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.CasterIsPawn));
        var idx = list.FindIndex(ins => ins.Calls(info2)) - 1;
        var label = generator.DefineLabel();
        list.InsertRange(idx, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(list[idx].ExtractLabels()),
            new CodeInstruction(OpCodes.Ldc_I4_0),
            CodeInstruction.Call(typeof(ManagedVerbUtility), nameof(ManagedVerbUtility.Managed)),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Brfalse, label),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ManagedVerb), nameof(ManagedVerb.Notify_ProjectileFired))),
            new CodeInstruction(OpCodes.Pop).WithLabels(label)
        });
        return list;
    }
}
