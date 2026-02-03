using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class GenRecipe_PostProcessProduct_Patch
{
    private static bool Prepare(MethodBase method)
    {
        // Don't bother checking on repeat passes.
        if (method != null)
            return true;

        // Only patch if we've got any def using this feature
        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            if (def.GetModExtension<ThingDefExtension>() is { playerCraftedStyleChance: > 0f } extension && !extension.playerCraftedStyles.NullOrEmpty())
                return true;
        }

        return false;
    }

    private static void Postfix(Thing product)
    {
        var extension = product.def?.GetModExtension<ThingDefExtension>();
        if (extension == null || extension.playerCraftedStyles.NullOrEmpty() || extension.playerCraftedStyleChance <= 0f)
            return;

        var styleable = (product as ThingWithComps)?.compStyleable;
        // Can't change style if the thing has no styleable comp
        if (styleable == null)
        {
            Log.WarningOnce($"[VEF] {product} has {nameof(ThingDefExtension)} with {nameof(ThingDefExtension.playerCraftedStyles)}, but it has no {nameof(CompProperties_Styleable)}", Gen.HashCombineInt(product.thingIDNumber, -2118693939));
            return;
        }

        // Can't replace existing styles
        if (!extension.playerCraftedStylesOverrideOtherStyles && (styleable.styleDef != null || styleable.SourcePrecept != null))
            return;

        // Must pass the RNG check
        if (!Rand.Chance(extension.playerCraftedStyleChance))
            return;

        styleable.styleDef = extension.playerCraftedStyles.RandomElementByWeight(x => x.Chance).StyleDef;
        styleable.cachedStyleCategoryDef = null;
        product.overrideGraphicIndex = null;
    }
}