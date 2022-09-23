using System.Collections.Generic;
using Verse;

namespace RecipeInheritance
{
    public class ThingDefExtension : DefModExtension
    {
        public ThingFilter allowedProductFilter;

        public List<RecipeDef> allowedRecipes;

        public ThingFilter disallowedProductFilter;

        public List<RecipeDef> disallowedRecipes;

        public List<ThingDef> inheritRecipesFrom;

        public bool Allows(RecipeDef recipe)
        {
            var producedThingDef = recipe.ProducedThingDef;
            return (producedThingDef == null || ((allowedProductFilter == null || allowedProductFilter.Allows(producedThingDef)) && (disallowedProductFilter == null || !disallowedProductFilter.Allows(producedThingDef))))
                   && (allowedRecipes == null || allowedRecipes.Contains(recipe))
                   && (disallowedRecipes == null || !disallowedRecipes.Contains(recipe));
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (inheritRecipesFrom == null)
            {
                yield return "inheritRecipesFrom is null.";
            }
            else
            {
                var nonWorkbenchDefNames = new List<string>();

                for (int i = 0; i < inheritRecipesFrom.Count; i++)
                {
                    var thing = inheritRecipesFrom[i];
                    if (!thing.IsWorkTable)
                    {
                        nonWorkbenchDefNames.Add(thing.defName);
                    }
                }

                if (nonWorkbenchDefNames.Any())
                {
                    yield return "the following ThingDefs in inheritRecipesFrom are not workbenches: " + GenText.ToCommaList(nonWorkbenchDefNames, false);
                }
            }
            yield break;
        }
    }
}