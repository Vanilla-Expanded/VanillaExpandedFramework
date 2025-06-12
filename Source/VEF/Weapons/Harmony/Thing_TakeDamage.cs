using HarmonyLib;
using Verse;
using Verse.Sound;

namespace VEF.Weapons
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class VanillaExpandedFramework_Thing_TakeDamage_Patch
    {
        public static void Postfix(Thing __instance, DamageInfo dinfo)
        {
            var extension = dinfo.Def?.GetModExtension<DamageExtension>();
            if (extension != null)
            {
                if (extension.soundOnDamage != null)
                {
                    extension.soundOnDamage.PlayOneShot(__instance);
                }
            }
        }
    }
}
