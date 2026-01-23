using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_CompUniqueWeapon_AddTrait_Patch
    {

        public static void HandleExtendedWorker(WeaponTraitDef traitDef, CompUniqueWeapon __instance)
        {
            if (traitDef.Worker is WeaponTraitWorker_Extended extendedWorker)
                extendedWorker.Notify_Added(__instance.parent);

            __instance.parent?.GetComp<CompApplyWeaponTraits>()?.DeleteCaches();
        }
    }


}
