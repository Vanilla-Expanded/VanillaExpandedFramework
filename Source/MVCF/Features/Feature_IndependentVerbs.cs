using System.Collections.Generic;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.Features
{
    public class Feature_IndependentVerbs : Feature
    {
        public override string Name => "IndependentFire";

        public override IEnumerable<Patch> GetPatches()
        {
            foreach (var patch in base.GetPatches()) yield return patch;

            yield return Patch.Postfix(AccessTools.Method(typeof(Pawn), nameof(Pawn.SpawnSetup)),
                AccessTools.Method(GetType(), nameof(Postfix_Pawn_SpawnSetup)));
            yield return Patch.Prefix(AccessTools.Method(typeof(Pawn), nameof(Pawn.DeSpawn)),
                AccessTools.Method(GetType(), nameof(Postfix_Pawn_DeSpawn)));
            yield return Patch.Prefix(AccessTools.Method(typeof(Pawn_StanceTracker), "SetStance"),
                AccessTools.Method(GetType(), nameof(Pawn_StanceTracker_SetStance)));
        }

        public static void Postfix_Pawn_SpawnSetup(Pawn __instance)
        {
            var man = __instance.Manager();
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.Instance.TickManagers.Add(new System.WeakReference<VerbManager>(man));
            man.Notify_Spawned();
        }

        public static void Postfix_Pawn_DeSpawn(Pawn __instance)
        {
            var man = __instance.Manager(false);
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.Instance.TickManagers.RemoveAll(wr =>
                {
                    if (!wr.TryGetTarget(out var vm)) return true;
                    return vm == man;
                });
            man.Notify_Despawned();
        }

        public static bool Pawn_StanceTracker_SetStance(Stance newStance) => !(newStance is Stance_Busy busy && busy.verb.Managed().Independent);
    }
}