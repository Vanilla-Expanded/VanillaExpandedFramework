using HarmonyLib;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Draw ui icon of ProcessDef
    /// </summary>
    [HarmonyPatch(typeof(Widgets))]
    [HarmonyPatch("DefIcon", MethodType.Normal)]
    public static class Widgets_DefIcon_Patch
    {
        public static bool Prefix(Rect rect, Def def, ThingDef stuffDef = null, float scale = 1f, ThingStyleDef thingStyleDef = null, bool drawPlaceholder = false, Color? color = null, Material material = null, int? graphicIndexOverride = null)
        {
            if (def is ProcessDef process)
            {
                Widgets.ThingIcon(rect, process.results[0].thing, stuffDef, thingStyleDef, scale, color, graphicIndexOverride);
                return false;
            }
            return true; // Continue
        }
    }
}
