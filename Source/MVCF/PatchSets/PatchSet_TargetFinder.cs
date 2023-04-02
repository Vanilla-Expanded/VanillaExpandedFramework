using System.Collections.Generic;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace MVCF.PatchSets;

public class PatchSet_TargetFinder : PatchSet
{
    public static bool CurrentEffectiveVerb_Prefix(ref Verb __result, Pawn __instance)
    {
        if (TargetFinder.SearchVerb is not null)
        {
            MVCF.LogFormat($"Giving SearchVerb {TargetFinder.SearchVerb} from CurrentEffectiveVerb", LogLevel.Tick);
            __result = TargetFinder.SearchVerb;
            return false;
        }

        if (__instance.MannedThing() is Building_Turret) return true;

        if (__instance.stances?.curStance is Stance_Busy { verb: { } verb })
        {
            MVCF.LogFormat($"Giving stance verb {verb} from CurrentEffectiveVerb", LogLevel.Tick);
            __result = verb;
            return false;
        }

        var man = __instance.Manager();
        if (man.HasVerbs && man.SearchVerb is not null && man.SearchVerb.Available())
        {
            MVCF.LogFormat($"Giving SearchVerb {man.SearchVerb} from CurrentEffectiveVerb", LogLevel.Tick);
            __result = man.SearchVerb;
            return false;
        }

        return true;
    }

    public static IEnumerable<CodeInstruction> AttackTargetTranspiler(IEnumerable<CodeInstruction> instructions) =>
        instructions.MethodReplacer(
            AccessTools.Method(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget)),
            AccessTools.Method(typeof(TargetFinder), nameof(TargetFinder.BestAttackTarget_Replacement)));

    public static IEnumerable<CodeInstruction> BestTargetTranspiler(IEnumerable<CodeInstruction> instructions) =>
        instructions.MethodReplacer(
            AccessTools.Method(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestShootTargetFromCurrentPosition)),
            AccessTools.Method(typeof(TargetFinder), nameof(TargetFinder.BestShootTargetFromCurrentPosition_Replacement)));

    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.CurrentEffectiveVerb)),
            AccessTools.Method(GetType(), nameof(CurrentEffectiveVerb_Prefix)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(JobGiver_AIFightEnemy), "FindAttackTarget"),
            AccessTools.Method(GetType(), nameof(AttackTargetTranspiler)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob"),
            AccessTools.Method(GetType(), nameof(AttackTargetTranspiler)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"),
            AccessTools.Method(GetType(), nameof(BestTargetTranspiler)));
    }
}
