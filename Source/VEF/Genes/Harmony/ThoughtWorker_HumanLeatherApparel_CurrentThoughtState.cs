using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Genes;

[HarmonyPatch(typeof(ThoughtWorker_HumanLeatherApparel), nameof(ThoughtWorker_HumanLeatherApparel.CurrentThoughtState))]
public class VanillaExpandedFramework_ThoughtWorker_HumanLeatherApparel_CurrentThoughtState
{
    public static void Postfix(Pawn p, ref ThoughtState __result)
    {
        // Already maxed out
        if (__result.StageIndex >= 4)
            return;

        // Only do anything if the pawn has custom leather defined
        if (!StaticCollectionsClass.defs_treated_as_human_leather.TryGetValue(p, out var leathers))
            return;

        var stage = __result.StageIndex;
        // If inactive the stage is -99999
        if (stage < 0)
            stage = 0;
        var reason = __result.Reason;

        foreach (var apparel in p.apparel.WornApparel)
        {
            if (apparel.Stuff != null && leathers.Contains(apparel.Stuff))
            {
                reason ??= apparel.def.label;
                stage++;
            }
        }

        __result = stage switch
        {
            0    => ThoughtState.Inactive,
            >= 5 => ThoughtState.ActiveAtStage(4, reason),
            _    => ThoughtState.ActiveAtStage(stage - 1, reason)
        };
    }
}