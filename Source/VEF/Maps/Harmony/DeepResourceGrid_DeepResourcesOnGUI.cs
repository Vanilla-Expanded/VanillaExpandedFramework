using HarmonyLib;
using Verse;
using VEF.Things;

namespace VEF.Maps
{
    [HarmonyPatch(typeof(DeepResourceGrid))]
    [HarmonyPatch("DeepResourcesOnGUI", MethodType.Normal)]
    public static class VanillaExpandedFramework_DeepResourceGrid_DeepResourcesOnGUI
    {
        public static void Postfix(DeepResourceGrid __instance, CellBoolDrawer ___drawer, Map ___map)
        {
            // DeepResourceGrid has a map check, so we should probably as well just in case.
            if (___map != Find.CurrentMap)
                return;

            Thing thing = Find.Selector.SingleSelectedThing;
            if (thing != null && thing.Map == ___map && thing.def.GetModExtension<ThingDefExtension>() is ThingDefExtension ext)
            {
                if (ext.deepResourcesOnGUI && (!ext.deepResourcesOnGUIRequireScanner || __instance.AnyActiveDeepScannersOnMap()))
                {
                    ___drawer.MarkForDraw();
                    NonPublicMethods.RenderMouseAttachments(__instance);
                }
            }
        }
    }
}
