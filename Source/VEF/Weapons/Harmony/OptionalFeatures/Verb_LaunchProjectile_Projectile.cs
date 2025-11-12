using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_Verb_LaunchProjectile_Projectile_Patch
    {

        public static void ChangeProjectile(ref ThingDef __result, Verb_LaunchProjectile __instance)
        {

            if (__result == __instance.verbProps.defaultProjectile && __instance.EquipmentSource != null && StaticCollectionsClass.uniqueWeaponsInGame.Contains(__instance.EquipmentSource.def))
            {
                CompUniqueWeapon comp = __instance.EquipmentSource?.GetComp<CompUniqueWeapon>();
                if (comp != null)
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                        if (extension != null)
                        {
                            if (extension.randomprojectiles)
                            {
                                __result = StaticCollectionsClass.projectilesInGame.RandomElement();
                                if (!extension.lowPreferenceProjectileOverride)
                                {
                                    break;
                                }
                            }
                            else if (!extension.projectileOverrides.NullOrEmpty() && extension.projectileOverrides.ContainsKey(__instance.EquipmentSource.def))
                            {
                                __result = extension.projectileOverrides[__instance.EquipmentSource.def];
                                if (!extension.lowPreferenceProjectileOverride)
                                {
                                    break;
                                }
                            }
                            else if (extension.projectileOverride != null)
                            {
                                __result = extension.projectileOverride;
                                if (!extension.lowPreferenceProjectileOverride)
                                {
                                    break;
                                }
                            }

                        }

                    }
                }
            }
        }
    }


}
