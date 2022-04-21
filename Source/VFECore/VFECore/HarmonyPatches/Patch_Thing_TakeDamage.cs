using HarmonyLib;
using Verse;
using Verse.Sound;

namespace VFECore
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class Patch_Thing_TakeDamage
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
