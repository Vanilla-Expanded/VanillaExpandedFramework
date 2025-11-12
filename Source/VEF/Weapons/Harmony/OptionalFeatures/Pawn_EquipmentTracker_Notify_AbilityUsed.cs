using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_Pawn_EquipmentTracker_Notify_AbilityUsed_Patch
    {

        public static void NotifyAbilityUses(Ability ability, Pawn_EquipmentTracker __instance)
        {
            CompApplyWeaponTraits comp = __instance.Primary?.GetComp<CompApplyWeaponTraits>();
            if (comp != null && ability.def == comp.abilityWithCharges)
            {
                comp.Notify_UsedAbility();
            }
        }
    }


}
