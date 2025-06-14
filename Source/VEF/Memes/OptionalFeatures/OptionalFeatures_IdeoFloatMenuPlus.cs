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
            harm.Patch(AccessTools.Method(typeof(IdeoUIUtility), "AddPrecept"), transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_IdeoUIUtility_AddPrecept_Patch), nameof(VanillaExpandedFramework_IdeoUIUtility_AddPrecept_Patch.Transpiler)));
        }
    }
}
