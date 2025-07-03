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

            if (__result == __instance.verbProps.defaultProjectile)
            {
                CompUniqueWeapon comp = __instance.EquipmentSource?.GetComp<CompUniqueWeapon>();
                if (comp != null)
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                        if (extension?.randomprojectiles == true) {
                            __result = StaticCollectionsClass.projectilesInGame.RandomElement();
                        }              
                        else if (extension?.projectileOverride != null)
                        {
                            __result = extension.projectileOverride;
                        }
                    }
                }
            }
        }
    }


}
