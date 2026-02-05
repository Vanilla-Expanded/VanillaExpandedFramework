using HarmonyLib;
using RimWorld;
using Verse;
using VEF.OptionalFeatures;
using VEF.Maps;
using RimWorld.Planet;
using System.Collections.Generic;
using System;
using VEF.AnimalBehaviours;

namespace VEF.Weapons
{
    
    public static class OptionalFeatures_WeaponTraitDefFeatures
    {
        public static void ApplyFeature(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn), "GetInspectString"),
              postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_Pawn_GetInspectString_Patch), "AddInspectString"));

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

            harm.Patch(typeof(CompEquippable).PropertyGetter(nameof(CompEquippable.VerbProperties)),
               prefix: new HarmonyMethod(VanillaExpandedFramework_CompEquippable_VerbProperties_Patch.UseVerbTraitsIfPresent));

            harm.Patch(AccessTools.Method(typeof(VerbProperties), "AdjustedCooldown", new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing) }),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_VerbProperties_AdjustedCooldown_Patch), "RandomizeCooldown"));
            
            harm.Patch(typeof(Verb).PropertyGetter(nameof(Verb.BurstShotCount)),
               transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_BurstShotCount_Patch), "RandomizeBurstCount"));

            harm.Patch(AccessTools.Method(typeof(IncidentWorker_TraderCaravanArrival), "TryExecuteWorker"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_IncidentWorker_TraderCaravanArrival_TryExecuteWorker_Patch), "DetectEmpireContraband"));

            harm.Patch(AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAndApparelExtras"),
              prefix: new HarmonyMethod(typeof(VanillaExpandedFramework_PawnRenderUtility_DrawEquipmentAiming_Patch), "GrabPawn"));
            harm.Patch(AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAiming"),
               prefix: new HarmonyMethod(typeof(VanillaExpandedFramework_PawnRenderUtility_DrawEquipmentAiming_Patch), "DrawDuplicate"));
            harm.Patch(AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAiming"),
               finalizer: new HarmonyMethod(typeof(VanillaExpandedFramework_PawnRenderUtility_DrawEquipmentAiming_Patch), "DrawDuplicateCleanup"));
        }
    }
}
