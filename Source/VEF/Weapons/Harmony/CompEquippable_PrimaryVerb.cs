using System.Linq;
using HarmonyLib;
using Verse;

namespace VEF.Weapons;

[HarmonyPatch(typeof(CompEquippable), nameof(CompEquippable.PrimaryVerb), MethodType.Getter)]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_CompEquippable_PrimaryVerb_Patch
{
    private static bool? isActive = null;
    internal static bool IsActive => isActive ??= isActive ??= DefDatabase<ThingDef>.AllDefs.Any(x => x.HasComp<CompMultiVerbWeapon>());

    private static bool Prepare() => IsActive;

    private static bool Prefix(CompEquippable __instance, ref Verb __result)
    {
        var comp = __instance.parent.GetComp<CompMultiVerbWeapon>();
        if (comp == null)
            return true;

        if (__instance.VerbTracker.AllVerbs == null)
            __instance.VerbTracker.InitVerbsFromZero();

        __result = comp.ActiveVerb ??= __instance.VerbTracker.AllVerbs.FirstOrDefault(x => x.verbProps.Ranged);
        return false;
    }
}