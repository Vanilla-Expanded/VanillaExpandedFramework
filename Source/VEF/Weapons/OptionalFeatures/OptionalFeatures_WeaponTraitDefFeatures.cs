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

            harm.Patch(AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"),
               transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_Verb_TryCastNextBurstShot_Patch), "ChangeSoundProduced"));

         
        }
    }
}
