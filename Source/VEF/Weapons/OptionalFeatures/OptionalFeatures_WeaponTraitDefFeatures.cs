using HarmonyLib;
using RimWorld;
using Verse;
using VEF.OptionalFeatures;

namespace VEF.Weapons
{
    
    public static class OptionalFeatures_WeaponTraitDefFeatures
    {
        public static void ApplyFeature(Harmony harm)
        {

            harm.Patch(AccessTools.Property(typeof(Verb_LaunchProjectile), "Projectile").GetMethod, 
                postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_LaunchProjectile_Projectile_Patch), "ChangeProjectile"));

            harm.Patch(AccessTools.Method(typeof(CompUniqueWeapon), "CanAddTrait"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_CompUniqueWeapon_CanAddTrait_Patch), "DetectOnlyAllowedWeapon"));

            harm.Patch(AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"),
               transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_TryCastNextBurstShot_Patch), "ChangeSoundProduced"));

            harm.Patch(AccessTools.Method(typeof(WeaponTraitWorker), "Notify_EquipmentLost"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_WeaponTraitWorker_Notify_EquipmentLost_Patch), "RemoveAbilities"));

            harm.Patch(AccessTools.Method(typeof(WeaponTraitWorker), "Notify_Equipped"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_WeaponTraitWorker_Notify_Equipped_Patch), "AddAbilities"));
        }
    }
}
