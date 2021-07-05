using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VanillaCookingExpanded
{
    public class CompStackByCondiments : ThingComp
    {

        //A comp class to make items only stack if their ingredients lists are the same

        //Used for example in Vanilla Cooking Expanded gourmet meals to avoid condiment abuse

        public CompProperties_StackByCondiments Props
        {
            get
            {
                return (CompProperties_StackByCondiments)this.props;
            }
        }


        public override bool AllowStackWith(Thing other)
        {

            if (other.TryGetComp<CompIngredients>() != null)
            {
                List<ThingDef> listingredients1 = other.TryGetComp<CompIngredients>().ingredients;
                List<ThingDef> listingredients2 = this.parent.TryGetComp<CompIngredients>().ingredients;

                string flagIngredientFoundOnSource = "";
                string flagIngredientFoundOnTarget = "";


                foreach (ThingDef ingredient in listingredients1)
                {
                    if (ingredient.ingredient != null)
                    {
                        foreach (string tag in ingredient.ingredient.mergeCompatibilityTags)
                        {
                            if (tag == Props.condimentTagToCheck)
                            {
                                flagIngredientFoundOnSource = ingredient.defName;
                            }
                        }
                    }
                }
                foreach (ThingDef ingredient2 in listingredients2)
                {
                    if (ingredient2.ingredient != null)
                    {
                        foreach (string tag2 in ingredient2.ingredient.mergeCompatibilityTags)
                        {
                            if (tag2 == Props.condimentTagToCheck)
                            {
                                flagIngredientFoundOnTarget = ingredient2.defName;
                            }
                        }
                    }
                }

                if ((flagIngredientFoundOnSource == flagIngredientFoundOnTarget)|| flagIngredientFoundOnSource=="" || flagIngredientFoundOnTarget == "")
                {
                    return true;
                }
                
                else return false;


               

            }



            return base.AllowStackWith(other);
        }

        


    }
}
