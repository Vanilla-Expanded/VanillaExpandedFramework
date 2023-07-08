using HarmonyLib;
using VanillaFurnitureExpanded;
using Verse;

namespace PipeSystem.GlowerExtended
{
    /// <summary>
    /// Make sure CompGlowerExtended is off if any of the required resources are off.
    /// </summary>
    [HarmonyPatch(typeof(CompGlowerExtended))]
    [HarmonyPatch("ShouldBeLitNow", MethodType.Getter)]
    public static class ShouldBeLitNow_Postfix
    {
        public static void Postfix(ThingWithComps ___parent, ref bool __result)
        {
            foreach (var comp in ___parent.GetComps<CompResourceTrader>())
            {
                if (comp != null && !comp.ResourceOn)
                {
                    __result = false;
                    return;
                }
            }
        }
    }
}