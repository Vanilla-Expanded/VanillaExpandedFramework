using HarmonyLib;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(Gene), "PostAdd")]
    public static class VanillaGenesExpanded_Gene_PostAdd_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(Gene __instance)
        {
            if (__instance.Active)
            {
                GeneExtension extension = __instance.def.GetModExtension<GeneExtension>();
                if (extension != null)
                {
                    if (extension.forceFemale == true)
                    {
                        __instance.pawn.gender = Gender.Female;
                    }
                    if (extension.forceMale == true)
                    {
                        __instance.pawn.gender = Gender.Male;
                    }

                    if (extension.forcedBodyType != null)
                    {
                        __instance.pawn.story.bodyType = extension.forcedBodyType;
                        __instance.pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                    }
                }
            }
        }
    }
}