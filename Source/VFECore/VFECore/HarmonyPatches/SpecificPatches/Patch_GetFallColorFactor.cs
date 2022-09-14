using HarmonyLib;
using RimWorld;

namespace VFECore
{
    [HarmonyPatch(typeof(PlantFallColors), "GetFallColorFactor")]
    public static class Patch_GetFallColorFactor
    {
        public static float fallColorFactor = 0f;
        public static void Postfix(ref float __result)
        {
            fallColorFactor = __result;
        }
    }
}
