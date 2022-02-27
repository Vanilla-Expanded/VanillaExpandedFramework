using System.Collections.Generic;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Features
{
    public class Feature_ApparelVerbs : Feature_Humanoid
    {
        public override string Name => "ApparelVerbs";

        public override IEnumerable<Patch> GetPatches()
        {
            var type = typeof(Pawn_ApparelTracker);
            yield return Patch.Postfix(AccessTools.Method(type, "Notify_ApparelAdded"),
                AccessTools.Method(GetType(), nameof(ApparelAdded_Postfix)));
            yield return Patch.Postfix(AccessTools.Method(type, "Notify_ApparelRemoved"),
                AccessTools.Method(GetType(), nameof(ApparelRemoved_Postfix)));
        }

        public static void ApparelAdded_Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            __instance.pawn.Manager().AddVerbs(apparel);
        }

        public static void ApparelRemoved_Postfix(Apparel apparel, Pawn_ApparelTracker __instance)
        {
            if (Base.IsIgnoredMod(apparel?.def?.modContentPack?.Name)) return;
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            comp.Notify_Unworn();
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }
    }
}