using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_Stats : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.Method(typeof(StatWorker), nameof(StatWorker.GetBaseValueFor)),
            AccessTools.Method(GetType(), nameof(GetBaseValue_Prefix)));
        yield return Patch.Transpiler(
            AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new[] { typeof(Tool), typeof(Pawn), typeof(Thing) }),
            AccessTools.Method(GetType(), nameof(CooldownTranspiler1)));
        yield return Patch.Transpiler(
            AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown),
                new[] { typeof(Tool), typeof(Pawn), typeof(ThingDef), typeof(ThingDef) }),
            AccessTools.Method(GetType(), nameof(CooldownTranspiler2)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(VerbProperties), "AdjustedAccuracy"),
            AccessTools.Method(GetType(), nameof(AccuracyTranspiler)));
    }

    public static bool GetBaseValue_Prefix(ref float __result)
    {
        if (VerbStatsUtility.ForceBaseValue is not { } val) return true;
        __result = val;
        return false;
    }

    public static IEnumerable<CodeInstruction> CooldownTranspiler1(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var info1 = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));
        var idx = codes.FindIndex(ins => ins.Calls(info1));
        codes[idx].operand = AccessTools.Method(typeof(VerbStatsUtility), nameof(VerbStatsUtility.GetStatValueWithBase));
        codes.RemoveAt(idx - 1);
        var info2 = AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.RangedWeapon_Cooldown));
        idx = codes.FindIndex(ins => ins.LoadsField(info2));
        codes.InsertRange(idx + 1, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldsfld, info2),
            new CodeInstruction(OpCodes.Ldarg_3),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VerbStatsUtility), nameof(VerbStatsUtility.GetBaseValue)))
        });
        return codes;
    }

    public static IEnumerable<CodeInstruction> CooldownTranspiler2(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var info = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValueAbstract),
            new[] { typeof(BuildableDef), typeof(StatDef), typeof(ThingDef) });
        var idx = codes.FindIndex(ins => ins.Calls(info));
        codes[idx].operand = AccessTools.Method(typeof(VerbStatsUtility), nameof(VerbStatsUtility.GetStatValueAbstractWithBase));
        codes.InsertRange(idx - 1, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.RangedWeapon_Cooldown))),
            new CodeInstruction(OpCodes.Ldarg_3),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VerbStatsUtility), nameof(VerbStatsUtility.GetBaseValue)))
        });
        return codes;
    }

    public static IEnumerable<CodeInstruction> AccuracyTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var info = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));
        var idx = codes.FindIndex(ins => ins.Calls(info));
        codes[idx].operand = AccessTools.Method(typeof(VerbStatsUtility), nameof(VerbStatsUtility.GetStatValueWithBase));
        codes.RemoveAt(idx - 1);
        codes.InsertRange(idx - 2, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Ldarg_2),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VerbStatsUtility), nameof(VerbStatsUtility.GetBaseValue)))
        });
        return codes;
    }
}
