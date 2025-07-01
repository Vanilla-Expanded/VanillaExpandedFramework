using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_WeaponTraitWorker_Notify_Equipped_Patch
    {
      
        static void AddAbilities(Pawn pawn, WeaponTraitWorker __instance)
        {
           
            WeaponTraitDefExtension extension = __instance.def.GetModExtension<WeaponTraitDefExtension>();
            if (extension?.abilityToAdd != null)
            {
                pawn.abilities?.GainAbility(extension.abilityToAdd);
            }

        }
    }


}
