using HarmonyLib;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Make sure CompGlower update the light.
    /// </summary>
    [HarmonyPatch(typeof(CompGlower))]
    [HarmonyPatch("ReceiveCompSignal", MethodType.Normal)]
    public static class ReceiveCompSignal_Postfix
    {
        public static void Postfix(string signal, ThingWithComps ___parent, CompGlower __instance)
        {
            if (CachedCompResourceTrader.cachedCompResourceTrader.ContainsKey(___parent)
                && CachedCompResourceTrader.IsCompMessage(___parent, signal))
                __instance.UpdateLit(___parent.Map);
        }
    }
}