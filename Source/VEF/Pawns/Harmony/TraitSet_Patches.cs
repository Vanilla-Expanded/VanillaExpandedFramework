using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Pawns
{
    // Trait painOffset/painFactor (and TraitExtension.painFactor) are read in HediffSet.CalculatePain,
    // whose cached result vanilla does not reset when traits change
    [HarmonyPatch]
    public static class VanillaExpandedFramework_TraitSet_Patches
    {
        [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
        [HarmonyPostfix]
        public static void GainTrait_Postfix(Pawn ___pawn)
        {
            ___pawn?.health?.hediffSet?.DirtyPainCache();
        }

        [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.RemoveTrait))]
        [HarmonyPostfix]
        public static void RemoveTrait_Postfix(Pawn ___pawn)
        {
            ___pawn?.health?.hediffSet?.DirtyPainCache();
        }
    }
}
