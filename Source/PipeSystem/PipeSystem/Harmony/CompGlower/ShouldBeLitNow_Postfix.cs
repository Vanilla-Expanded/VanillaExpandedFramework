using HarmonyLib;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Make sure CompGlower is off if any of the required resources are off.
    /// </summary>
    [HarmonyPatch(typeof(CompGlower))]
    [HarmonyPatch("ShouldBeLitNow", MethodType.Getter)]
    public static class ShouldBeLitNow_Postfix
    {
        public static void Postfix(ThingWithComps ___parent, ref bool __result)
        {
            if (CachedCompResourceTrader.cachedCompResourceTrader.ContainsKey(___parent))
                __result = CachedCompResourceTrader.AllResourceOn(___parent);
        }
    }
}