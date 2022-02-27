using System.Collections.Generic;
using HarmonyLib;
using MVCF.HarmonyPatches;
using MVCF.Utilities;
using Verse;

namespace MVCF.Features.PatchSets
{
    public class PatchSet_IndependentVerbs : PatchSet
    {
        public override IEnumerable<Patch> GetPatches()
        {
            yield return Patch.Postfix(AccessTools.Method(typeof(Pawn), nameof(Pawn.SpawnSetup)),
                AccessTools.Method(GetType(), nameof(Postfix_Pawn_SpawnSetup)));
            yield return Patch.Prefix(AccessTools.Method(typeof(Pawn), nameof(Pawn.DeSpawn)),
                AccessTools.Method(GetType(), nameof(Postfix_Pawn_DeSpawn)));
            yield return Patch.Prefix(AccessTools.Method(typeof(Pawn_StanceTracker), "SetStance"),
                AccessTools.Method(GetType(), nameof(Pawn_StanceTracker_SetStance)));
            yield return Patch.Postfix(AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.Caster)),
                AccessTools.Method(GetType(), nameof(Postfix_get_Caster)));
            yield return Patch.Postfix(AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.CasterPawn)),
                AccessTools.Method(GetType(), nameof(Postfix_get_CasterPawn)));
            yield return Patch.Postfix(AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.CasterIsPawn)),
                AccessTools.Method(GetType(), nameof(Postfix_get_CasterIsPawn)));
        }

        public static void Postfix_Pawn_SpawnSetup(Pawn __instance)
        {
            var man = __instance.Manager();
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.GetComp().TickManagers.Add(new System.WeakReference<VerbManager>(man));
            man.Notify_Spawned();
        }

        public static void Postfix_Pawn_DeSpawn(Pawn __instance)
        {
            var man = __instance.Manager(false);
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.GetComp().TickManagers.RemoveAll(wr =>
                {
                    if (!wr.TryGetTarget(out var vm)) return true;
                    return vm == man;
                });
            man.Notify_Despawned();
        }

        public static bool Pawn_StanceTracker_SetStance(Stance newStance) => !(newStance is Stance_Busy busy && busy.verb?.caster is IFakeCaster);

        public static void Postfix_get_Caster(ref Thing __result)
        {
            if (__result is IFakeCaster caster) __result = caster.RealCaster();
        }

        public static void Postfix_get_CasterPawn(ref Pawn __result, Verb __instance)
        {
            if (__instance.caster is IFakeCaster caster) __result = caster.RealCaster() as Pawn;
        }

        public static void Postfix_get_CasterIsPawn(ref bool __result, Verb __instance)
        {
            if (__instance.caster is IFakeCaster caster) __result = caster.RealCaster() is Pawn;
        }
    }
}