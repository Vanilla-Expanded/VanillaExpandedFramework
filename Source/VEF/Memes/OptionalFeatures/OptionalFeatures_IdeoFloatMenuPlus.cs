using HarmonyLib;
using RimWorld;
using Verse;
using VEF.OptionalFeatures;
using VEF.Memes;

namespace VEF.Memes
{
    
    public static class OptionalFeatures_IdeoFloatMenuPlus
    {
        public static void ApplyFeature(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(IdeoUIUtility), "DoPreceptsInt"), transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_IdeoUIUtility_DoPreceptsInt_Patch), nameof(VanillaExpandedFramework_IdeoUIUtility_DoPreceptsInt_Patch.Transpiler)));
        }
    }
}
