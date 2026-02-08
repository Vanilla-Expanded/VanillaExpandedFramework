using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts), MethodType.Enumerator)]
[HarmonyPatchCategory(VEF_HarmonyCategories.UseStoneChunksAsStuffInRecipes)]
public static class VanillaExpandedFramework_GenRecipe_MakeRecipeProducts_Patch
{
    private static ThingDef StuffDefWrapper(ThingDef def, RecipeDef recipe)
    {
        if (recipe.GetModExtension<RecipeExtension>()?.chunksAsStuff != true)
            return def;

        // If the chunk has multiple blocks it can make, pick one at random (weighted based on output count).
        var replacement = GetStoneChunks(def).RandomElementByWeightWithFallback(x => x.count);
        return replacement?.thingDef ?? def;
    }

    private static Color DrawColorWrapper(Color color, Thing thing, RecipeDef recipe)
    {
        if (thing.Stuff == null || recipe.GetModExtension<RecipeExtension>()?.chunksAsStuff != true)
            return color;

        return thing.Stuff.graphicData?.color ?? Color.white;
    }

    public static IEnumerable<ThingDefCountClass> GetStoneChunks(ThingDef def)
    {
        if (def?.butcherProducts == null)
            yield break;

        foreach (var product in def.butcherProducts)
        {
            if (product.thingDef is { IsStuff: true, MadeFromStuff: false, thingCategories: not null } && product.thingDef.thingCategories.Contains(ThingCategoryDefOf.StoneBlocks))
                yield return product;
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        // We're in a compiler-generated class, so we need to grab the dominantIngredient and recipeDef fields.
        var dominantIngredientField = baseMethod.DeclaringType.Field("dominantIngredient");
        var recipeDefField = baseMethod.DeclaringType.Field("recipeDef");
        var thingDefField = typeof(Thing).Field(nameof(Thing.def));

        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            // Loads the first argument (this)
            CodeMatch.IsLdarg(0),
            // Loads the dominantIngredient
            CodeMatch.LoadsField(dominantIngredientField),
            // Loads the Thing.def field
            CodeMatch.LoadsField(thingDefField),
            // Sets from stack to local
            CodeMatch.IsStloc()
        );

        // Insert before "Stloc" call
        matcher.Insert(
            // Loads "this"
            CodeInstruction.LoadArgument(0),
            // Load the recipeDef field
            new CodeInstruction(OpCodes.Ldfld, recipeDefField),
            // Call our wrapper method
            CodeInstruction.Call(() => StuffDefWrapper)
        );

        matcher.Reset();

        matcher.MatchEndForward(
            // Loads "this"
            CodeMatch.IsLdarg(0),
            // Loads the dominantIngredient field
            CodeMatch.LoadsField(dominantIngredientField),
            // Calls the DrawColor method
            CodeMatch.Calls((MethodInfo)null)
        );

        matcher.InsertAfter(
            // Loads "this"
            CodeInstruction.LoadArgument(0),
            // Load the dominantIngredient field
            new CodeInstruction(OpCodes.Ldfld, dominantIngredientField),
            // Loads "this"
            CodeInstruction.LoadArgument(0),
            // Load the recipeDef field
            new CodeInstruction(OpCodes.Ldfld, recipeDefField),
            // Call our wrapper method
            CodeInstruction.Call(() => DrawColorWrapper)
        );

        return matcher.Instructions();
    }
}