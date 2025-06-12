using HarmonyLib;
using VEF.Graphics;
using Verse;

namespace VEF.Graphics
{
    [HarmonyPatch(typeof(VerbTracker), "CreateVerbTargetCommand")]
    public static class VanillaExpandedFramework_VerbTracker_CreateVerbTargetCommand_Patch
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
