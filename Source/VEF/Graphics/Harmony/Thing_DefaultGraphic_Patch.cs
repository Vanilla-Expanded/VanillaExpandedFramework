using HarmonyLib;
using VEF.Graphics;
using Verse;

namespace VEF.Graphics
{
    [HarmonyPatch(typeof(Thing), "DefaultGraphic", MethodType.Getter)]
    public static class VanillaExpandedFramework_Thing_DefaultGraphic_Patch
    {
        public static bool Prefix(Thing __instance, ref Graphic __result)
        {
            if (ReflectionCache.itemGraphic(__instance) is null && __instance is not Mote)
            {
                var comp = __instance.TryGetComp<CompGraphicCustomization>();
                if (comp != null)
                {
                    __result = ReflectionCache.itemGraphic(__instance) = comp.Graphic;
                    return false;
                }
            }
            return true;
        }
    }
}
