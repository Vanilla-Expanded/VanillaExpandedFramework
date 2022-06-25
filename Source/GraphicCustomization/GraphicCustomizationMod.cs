using HarmonyLib;
using System.Linq;
using UnityEngine;
using Verse;

namespace GraphicCustomization
{
    public class GraphicCustomizationMod : Mod
    {        
        public GraphicCustomizationMod(ModContentPack pack) : base(pack)
        {
            new Harmony("GraphicCustomization.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Thing), "DefaultGraphic", MethodType.Getter)]
    public static class Thing_DefaultGraphic_Patch
    {
        public static bool Prefix(Thing __instance, ref Graphic __result)
        {
            if (__instance.graphicInt is null)
            {
                var comp = __instance.TryGetComp<CompGraphicCustomization>();
                if (comp != null)
                {
                    __result = comp.Graphic;
                    __instance.graphicInt = __result;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(VerbTracker), "CreateVerbTargetCommand")]
    public static class VerbTracker_CreateVerbTargetCommand_Patch
    {
        public static void Postfix(ref Command_VerbTarget __result, Thing ownerThing, Verb verb)
        {
            var comp = ownerThing.TryGetComp<CompGraphicCustomization>();
            if (comp != null)
            {
                __result.icon = comp.Texture;
            }
        }
    }
}
