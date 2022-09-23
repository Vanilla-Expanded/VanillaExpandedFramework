using System.Collections.Generic;
using Verse;

namespace RecipeInheritance
{
    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {
        static StaticConstructorClass()
        {
            var defs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < defs.Count; i++)
            {
                var self = defs[i];
                if (self.IsWorkTable && self.GetModExtension<ThingDefExtension>() is ThingDefExtension ext && ext.inheritRecipesFrom != null)
                {
                    var list = new List<RecipeDef>(self.AllRecipes);
                    NonPublicFields.ThingDef_allRecipesCached.SetValue(self, null);

                    for (int j = 0; j < ext.inheritRecipesFrom.Count; j++)
                    {
                        var worktable = ext.inheritRecipesFrom[j];
                        var recipeDefs = worktable.AllRecipes ?? new List<RecipeDef>();

                        for (int k = 0; k < recipeDefs.Count; k++)
                        {
                            var recipeDef = worktable.AllRecipes[k];
                            if (ext.Allows(recipeDef))
                            {
                                if (self.recipes == null)
                                {
                                    self.recipes = new List<RecipeDef>();
                                }
                                if (!list.Contains(recipeDef))
                                {
                                    self.recipes.Add(recipeDef);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}