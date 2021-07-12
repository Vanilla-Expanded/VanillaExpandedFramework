using System.Collections.Generic;
using Verse;

namespace RecipeInheritance
{
    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {
        static StaticConstructorClass()
        {
            for (int i = 0; i < DefDatabase<ThingDef>.AllDefsListForReading.Count; i++)
            {
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefsListForReading[i];
                if (thingDef.IsWorkTable)
                {
                    ThingDefExtension thingDefExtension = ThingDefExtension.Get(thingDef);
                    if (thingDefExtension.inheritRecipesFrom != null)
                    {
                        List<RecipeDef> list = new List<RecipeDef>(thingDef.AllRecipes);
                        NonPublicFields.ThingDef_allRecipesCached.SetValue(thingDef, null);

                        for (int j = 0; j < thingDefExtension.inheritRecipesFrom.Count; j++)
                        {
                            ThingDef thingDef2 = thingDefExtension.inheritRecipesFrom[j];
                            for (int k = 0; k < thingDef2.AllRecipes.Count; k++)
                            {
                                RecipeDef recipeDef = thingDef2.AllRecipes[k];
                                if (thingDefExtension.Allows(recipeDef))
                                {
                                    if (thingDef.recipes == null)
                                    {
                                        thingDef.recipes = new List<RecipeDef>();
                                    }
                                    if (!list.Contains(recipeDef))
                                    {
                                        thingDef.recipes.Add(recipeDef);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}