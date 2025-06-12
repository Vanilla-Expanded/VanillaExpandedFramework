using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Pawns
{
    [HarmonyPatch(typeof(HediffSet), "CalculatePain")]
    public static class VanillaExpandedFramework_HediffSet_CalculatePain_Patch
    {
        public static void Postfix(HediffSet __instance, ref float __result)
        {
            var traits = __instance.pawn.story?.traits?.allTraits ?? new List<Trait>();
            foreach (var trait in traits)
            {
                var extension = trait.def.GetModExtension<TraitExtension>();
                if (extension != null)
                {
                    if (extension.painFactor != 1f)
                    {
                        __result *= extension.painFactor;
                    }
                }
            }
        }
    }
}
