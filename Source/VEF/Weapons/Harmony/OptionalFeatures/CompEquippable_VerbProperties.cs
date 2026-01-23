using System.Collections.Generic;
using Verse;

namespace VEF.Weapons;

// This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

public static class VanillaExpandedFramework_CompEquippable_VerbProperties_Patch
{
    public static bool UseVerbTraitsIfPresent(CompEquippable __instance, ref List<VerbProperties> __result)
    {
        var uniqueWeapon = __instance.parent.GetComp<CompApplyWeaponTraits>();
        // Make sure we have a unique weapon
        if (uniqueWeapon == null)
            return true;

        // Check all traits for any verb-giving traits
        foreach (var extension in uniqueWeapon.GetDetails())
        {
            // Check if we have any verbs
            if (!extension.verbs.NullOrEmpty())
            {
                // Replace original verbs
                __result = extension.verbs;
                return false;
            }
        }

        return true;
    }
}