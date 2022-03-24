using HarmonyLib;
using RimWorld;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Patch Designator_Install SelectedUpdate. If the selected thing will ever transmit resource, draw the net overlay.
    /// </summary>
    [HarmonyPatch(typeof(Designator_Install))]
    [HarmonyPatch("SelectedUpdate", MethodType.Normal)]
    public static class Designator_Install_Patch
    {
        public static void Postfix(Designator_Install __instance)
        {
            if (__instance.PlacingDef is ThingDef thingDef && CachedResourceThings.resourceCompsOf.ContainsKey(thingDef))
            {
                SectionLayer_Resource.UpdateAndDrawFor(CachedResourceThings.resourceCompsOf[thingDef][0].pipeNet);
            }
        }
    }
}