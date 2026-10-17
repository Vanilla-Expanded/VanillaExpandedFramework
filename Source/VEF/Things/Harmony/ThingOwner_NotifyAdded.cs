using HarmonyLib;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(ThingOwner), "NotifyAdded")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public class VanillaExpandedFramework_ThingOwner_NotifyAdded_Patch
{
    private static void Prefix(Thing item, IThingHolder ___owner)
    {
        if (!ThingOwnerUtility.ShouldAutoExtinguishInnerThings(___owner))
            return;

        var attachComp = item.TryGetComp<CompAttachBase>();
        if (attachComp?.attachments == null)
            return;

        for (var i = attachComp.attachments.Count - 1; i >= 0; i--)
        {
            if (attachComp.attachments[i] is CustomFire fire)
                fire.Destroy();
        }
    }
}