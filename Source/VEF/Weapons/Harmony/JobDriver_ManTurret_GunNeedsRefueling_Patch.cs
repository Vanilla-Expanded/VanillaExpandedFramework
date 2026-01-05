using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons;

[HarmonyPatch(typeof(JobDriver_ManTurret), "GunNeedsRefueling")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class JobDriver_ManTurret_GunNeedsRefueling_Patch
{
    private static bool? patchingAllowed = null;

    internal static bool Prepare() => patchingAllowed ??= DefDatabase<ThingDef>.AllDefs.Any(x => x.HasModExtension<AutoRefuelMannedTurrets>());

    private static void Postfix(Building b, ref bool __result)
    {
        var extension = b?.def?.GetModExtension<AutoRefuelMannedTurrets>();
        if (extension == null)
            return;

        __result = extension.ShouldAutoReload(b, __result);
    }
}