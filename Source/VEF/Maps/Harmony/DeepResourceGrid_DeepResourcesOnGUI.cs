using HarmonyLib;
using Verse;
using VEF.Things;

namespace VEF.Maps
{
    [HarmonyPatch(typeof(DeepResourceGrid))]
    [HarmonyPatch("DeepResourcesOnGUI", MethodType.Normal)]
    public static class VanillaExpandedFramework_DeepResourceGrid_DeepResourcesOnGUI
    {
        public static void Postfix(DeepResourceGrid __instance)
        {
            Thing thing = Find.Selector.SingleSelectedThing;
            if (thing != null && thing.def.GetModExtension<ThingDefExtension>() is ThingDefExtension ext)
            {
                Map map = thing.Map;
                if (ext.deepResourcesOnGUI
                    && ((ext.deepResourcesOnGUIRequireScanner && map.deepResourceGrid.AnyActiveDeepScannersOnMap()) || !ext.deepResourcesOnGUIRequireScanner))
                {
                    NonPublicMethods.RenderMouseAttachments(__instance);
                }
            }
        }
    }
}
