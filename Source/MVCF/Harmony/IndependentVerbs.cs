using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.Harmony
{
    public class IndependentVerbs
    {
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "SpawnSetup"),
                postfix: new HarmonyMethod(typeof(IndependentVerbs), nameof(Postfix_Pawn_SpawnSetup)));
            harm.Patch(AccessTools.Method(typeof(Pawn), "DeSpawn"),
                postfix: new HarmonyMethod(typeof(IndependentVerbs), nameof(Postfix_Pawn_DeSpawn)));
            harm.Patch(AccessTools.Method(typeof(Pawn_StanceTracker), "SetStance"),
                new HarmonyMethod(typeof(IndependentVerbs), nameof(Pawn_StanceTracker_SetStance)));
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

        public static bool Pawn_StanceTracker_SetStance(Stance newStance)
        {
            return !(newStance is Stance_Busy busy && busy.verb?.caster is IFakeCaster);
        }
    }
}