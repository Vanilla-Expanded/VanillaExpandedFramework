using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_VerbProperties_AdjustedCooldown_Patch
    {

        public static void RandomizeCooldown(ref float __result, Thing equipment)
        {

            if (equipment!=null && StaticCollectionsClass.uniqueWeaponsInGame.Contains(equipment.def))
            {
                CompUniqueWeapon comp = equipment.TryGetComp<CompUniqueWeapon>();
                if (comp != null)
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                        if (extension!=null && extension.coolDownRange != FloatRange.Zero)
                        {
                            __result *= extension.coolDownRange.RandomInRange;
                        }

                    }
                }

            }


        }
    }


}
