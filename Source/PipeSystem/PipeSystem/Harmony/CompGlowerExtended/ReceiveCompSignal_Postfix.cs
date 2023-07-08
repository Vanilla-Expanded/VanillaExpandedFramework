using HarmonyLib;
using VanillaFurnitureExpanded;
using Verse;

namespace PipeSystem.GlowerExtended
{
    /// <summary>
    /// Make sure CompGlowerExtended update the light.
    /// </summary>
    [HarmonyPatch(typeof(CompGlowerExtended))]
    [HarmonyPatch("ReceiveCompSignal", MethodType.Normal)]
    public static class ReceiveCompSignal_Postfix
    {
        public static void Postfix(string signal, ThingWithComps ___parent, CompGlowerExtended __instance)
        {
            if (CachedSignals.IsResourceSignal(signal))
                __instance.UpdateLit();
        }
    }
}