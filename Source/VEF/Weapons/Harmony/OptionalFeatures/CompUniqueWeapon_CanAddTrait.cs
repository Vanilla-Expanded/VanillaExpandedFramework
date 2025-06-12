using HarmonyLib;
using RimWorld;

namespace VEF.Weapons
{
   
    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_CompUniqueWeapon_CanAddTrait_Patch
    {
    
        static void DetectOnlyAllowedWeapon(ref bool __result, CompUniqueWeapon __instance, WeaponTraitDef trait)
        {

            if (__result)
            {
                WeaponTraitDefExtension extension = trait.GetModExtension<WeaponTraitDefExtension>();
                if (extension?.onlyForThisWeapon != null && extension.onlyForThisWeapon != __instance.parent.def)
                {
                    __result = false;
                }
            }
        }
    }
}
