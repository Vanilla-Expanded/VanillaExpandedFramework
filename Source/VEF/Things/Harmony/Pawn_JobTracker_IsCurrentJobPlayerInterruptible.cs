using HarmonyLib;
using Verse;
using Verse.AI;

namespace VEF.Things;

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.IsCurrentJobPlayerInterruptible))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public class Pawn_JobTracker_IsCurrentJobPlayerInterruptible
{
    private static bool Prepare() => CustomFireDef.AnyFireCausesPanic;

    private static void Postfix(ref bool __result, Pawn ___pawn)
    {
        // Already can't interrupt, skip
        if (!__result)
            return;

        var attachComp = ___pawn.GetComp<CompAttachBase>();
        // No attach comp or attachments still null, skip
        if (attachComp?.attachments == null)
            return;

        // Check if any fire causes panic
        for (var i = 0; i < attachComp.attachments.Count; i++)
        {
            if (attachComp.attachments[i] is CustomFire fire && fire.Def.causesPanic)
            {
                __result = false;
                return;
            }
        }
    }
}