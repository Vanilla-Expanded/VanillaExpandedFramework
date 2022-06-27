using HarmonyLib;
using RimWorld;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Patch Designator_Build SelectedUpdate. If the selected thing will ever transmit resource, draw the net overlay.
    /// </summary>
    [HarmonyPatch(typeof(Designator_Build))]
    [HarmonyPatch("SelectedUpdate", MethodType.Normal)]
    public static class Designator_Build_Patch
    {
        public static void Postfix(BuildableDef ___entDef)
        {
            if (___entDef is ThingDef thingDef && CachedResourceThings.resourceCompsOf.ContainsKey(thingDef))
            {
                SectionLayer_Resource.UpdateAndDrawFor(CachedResourceThings.resourceCompsOf[thingDef][0].pipeNet);
            }
        }
    }
}