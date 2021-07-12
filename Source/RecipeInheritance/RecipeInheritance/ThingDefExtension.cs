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

        private static readonly ThingDefExtension defaultValues = new ThingDefExtension();

        public static ThingDefExtension Get(Def def)
        {
            return def.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;
        }

        public bool Allows(RecipeDef recipe)
        {
            ThingDef producedThingDef = recipe.ProducedThingDef;
            return (producedThingDef == null || ((this.allowedProductFilter == null || this.allowedProductFilter.Allows(producedThingDef)) && (this.disallowedProductFilter == null || !this.disallowedProductFilter.Allows(producedThingDef)))) && (this.allowedRecipes == null || this.allowedRecipes.Contains(recipe)) && (this.disallowedRecipes == null || !this.disallowedRecipes.Contains(recipe));
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (this.inheritRecipesFrom == null)
            {
                yield return "inheritRecipesFrom is null.";
            }
            else
            {
                List<string> nonWorkbenchDefNames = new List<string>();
                List<string> recipelessThingDefNames = new List<string>();
                int num;
                for (int i = 0; i < this.inheritRecipesFrom.Count; i = num + 1)
                {
                    ThingDef thing = this.inheritRecipesFrom[i];
                    if (!thing.IsWorkTable)
                    {
                        nonWorkbenchDefNames.Add(thing.defName);
                    }
                    else
                    {
                        if (thing.AllRecipes.NullOrEmpty())
                        {
                            recipelessThingDefNames.Add(thing.defName);
                        }
                    }
                    num = i;
                }
                if (nonWorkbenchDefNames.Any())
                {
                    yield return "the following ThingDefs in inheritRecipesFrom are not workbenches: " + GenText.ToCommaList(nonWorkbenchDefNames, false);
                }
                if (recipelessThingDefNames.Any())
                {
                    yield return "the following workbenches in inheritRecipesFrom do not have any recipes: " + GenText.ToCommaList(recipelessThingDefNames, false);
                }
            }
            yield break;
        }
    }
}