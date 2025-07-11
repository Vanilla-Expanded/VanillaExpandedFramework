using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_Verb_MeleeAttack_SoundHitPawn_Patch
    {

        public static void ChangeMeleeSound(ref SoundDef __result, Verb_MeleeAttack __instance)
        {

            
                CompUniqueWeapon comp = __instance.EquipmentSource?.GetComp<CompUniqueWeapon>();
                if (comp != null)
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                        if (extension?.meleeSoundOverride !=null)
                        {
                            __result = extension.meleeSoundOverride;
                        }
                        
                    }
                }
            
        }
    }


}
