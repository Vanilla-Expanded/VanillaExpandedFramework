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

            Type type = traitDef.workerClass;
            if (typeof(WeaponTraitWorker_Extended).IsAssignableFrom(type))
            {
                WeaponTraitWorker_Extended workerExtended =
                    (WeaponTraitWorker_Extended)Activator.CreateInstance(type);

                workerExtended.Notify_Added(__instance.parent);
            }
        }
    }


}
