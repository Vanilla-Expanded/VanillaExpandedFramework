using HarmonyLib;
using RimWorld;
using Verse;
using VEF.OptionalFeatures;
using VEF.Maps;

namespace VEF.Weapons
{
    
    public static class OptionalFeatures_WeaponTraitDefFeatures
    {
        public static void ApplyFeature(Harmony harm)
        {

            harm.Patch(AccessTools.Property(typeof(Verb_LaunchProjectile), "Projectile").GetMethod, 
                postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_LaunchProjectile_Projectile_Patch), "ChangeProjectile"));

            harm.Patch(AccessTools.Method(typeof(Verb_MeleeAttack), "SoundHitPawn"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_MeleeAttack_SoundHitPawn_Patch), "ChangeMeleeSound"));

            harm.Patch(AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"),
               transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_TryCastNextBurstShot_Patch), "ChangeSoundProduced"));

            harm.Patch(AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(Verb_MeleeAttackDamage), "DamageInfosToApply")),
               transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_MeleeAttackDamage_DamageInfosToApply_Patch), "ModifyMeleeDamage"));

            harm.Patch(AccessTools.Method(typeof(CompUniqueWeapon), "AddTrait"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_CompUniqueWeapon_AddTrait_Patch), "HandleExtendedWorker"));

            harm.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "Notify_AbilityUsed"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_Pawn_EquipmentTracker_Notify_AbilityUsed_Patch), "NotifyAbilityUses"));
        }
    }
}
