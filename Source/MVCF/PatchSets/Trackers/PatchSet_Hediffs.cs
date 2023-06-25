using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MVCF.Utilities;
using Verse;
using Verse.AI;

namespace MVCF.PatchSets.Trackers;

public class PatchSet_Hediffs : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.GetForceMissFactorFor)),
            AccessTools.Method(GetType(), nameof(GetForceMissFactorFor_Prefix)));
        yield return Patch.Postfix(
            AccessTools.Method(typeof(Pawn_HealthTracker), "AddHediff",
                new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult) }),
            AccessTools.Method(GetType(), nameof(AddHediff_Postfix)));
        yield return Patch.Prefix(AccessTools.Method(typeof(Hediff), "PostRemoved"), AccessTools.Method(GetType(), nameof(PostRemoved_Prefix)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(Verb_ShootBeam), "ApplyDamage"), AccessTools.Method(GetType(), nameof(FixBeamVerb)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(Verb_CastTargetEffect), "TryCastShot"),
            AccessTools.Method(GetType(), nameof(FixTargetEffectVerb)));
    }

    public static bool GetForceMissFactorFor_Prefix(ref float __result, Thing equipment)
    {
        if (equipment is not null) return true;
        __result = 1f;
        return false;
    }

    public static void AddHediff_Postfix(Hediff hediff, Pawn_HealthTracker __instance)
    {
        __instance.hediffSet.pawn.Manager(false)?.AddVerbs(hediff);
    }

    public static void PostRemoved_Prefix(Hediff __instance)
    {
        if (MVCF.IsIgnoredMod(__instance.def?.modContentPack?.Name)) return;
        var comp = __instance.TryGetComp<HediffComp_VerbGiver>();
        if (comp?.VerbTracker?.AllVerbs == null) return;
        var manager = __instance.pawn.Manager(false);
        if (manager == null) return;
        foreach (var verb in comp.VerbTracker.AllVerbs.Concat(manager.ExtraVerbsFor(__instance))) manager.RemoveVerb(verb);
    }

    public static IEnumerable<CodeInstruction> FixBeamVerb(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codes = instructions.ToList();
        var info = AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.EquipmentSource));
        for (var i = 0; i < codes.Count; i++)
        {
            var instruction = codes[i];
            yield return instruction;
            if (instruction.Calls(info))
            {
                var label = generator.DefineLabel();
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Brfalse, label);
                yield return codes[++i];
                yield return new CodeInstruction(OpCodes.Castclass, typeof(ThingDef)).WithLabels(label);
            }
        }
    }

    public static IEnumerable<CodeInstruction> FixTargetEffectVerb(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codes = instructions.ToList();
        var info = AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.EquipmentSource));
        foreach (var instruction in codes)
        {
            yield return instruction;
            if (instruction.Calls(info))
            {
                var label = generator.DefineLabel();
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Brtrue, label);
                yield return new CodeInstruction(OpCodes.Pop);
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return new CodeInstruction(OpCodes.Nop).WithLabels(label);
            }
        }
    }
}
