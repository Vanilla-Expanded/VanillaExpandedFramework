using HarmonyLib;
using Verse;

namespace GraphicCustomization
{
    [HarmonyPatch(typeof(Thing), "DefaultGraphic", MethodType.Getter)]
    public static class Thing_DefaultGraphic_Patch
    {
        public static bool Prefix(Thing __instance, ref Graphic __result)
        {
            if (__instance.graphicInt is null && __instance is not Mote)
            {
                var comp = __instance.TryGetComp<CompGraphicCustomization>();
                if (comp != null)
                {
                    __result = __instance.graphicInt = comp.Graphic;
                    return false;
                }
            }
            return true;
        }
    }
}
