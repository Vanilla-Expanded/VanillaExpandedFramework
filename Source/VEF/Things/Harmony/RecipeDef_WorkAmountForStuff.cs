using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.WorkAmountForStuff))]
[HarmonyPatchCategory(VEF_HarmonyCategories.UseStoneChunksAsStuffInRecipes)]
public static class VanillaExpandedFramework_RecipeDef_WorkAmountForStuff_Patch
{
    private static bool Prefix(ThingDef stuff, RecipeDef __instance, ref float __result)
    {
        if (__instance.GetModExtension<RecipeExtension>()?.chunksAsStuff != true)
            return true;

        // The work amount is specified by recipe rather the output
        if (__instance.workAmount >= 0f)
        {
            __result = __instance.workAmount;
            return false;
        }

        // As opposed to picking a stone chunk for the final product (random weighted by output count),
        // here we pick the one with the highest count and use that so the recipe is always deterministic.
        var replacement = VanillaExpandedFramework_GenRecipe_MakeRecipeProducts_Patch.GetStoneChunks(stuff).MaxByWithFallback(x => x.count);

        __result = __instance.products[0].thingDef.GetStatValueAbstract(StatDefOf.WorkToMake, replacement?.thingDef ?? stuff);

        return false;
    }
}