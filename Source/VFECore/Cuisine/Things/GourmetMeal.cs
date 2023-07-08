using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;

using Verse;
using static HarmonyLib.Code;

namespace VanillaCookingExpanded
{
    public class GourmetMeal : ThingWithComps
    {
        public override bool CanStackWith(Thing other)
        {
            
            if (other.def == this.def && other.TryGetComp<CompIngredients>() != null && this.TryGetComp<CompIngredients>() != null && other as GourmetMeal != null)
            {
                List<ThingDef> listingredients1 = other.TryGetComp<CompIngredients>().ingredients;
                List<ThingDef> listingredients2 = this.TryGetComp<CompIngredients>().ingredients;

                string flagIngredientFoundOnSource = "";
                string flagIngredientFoundOnTarget = "";


                foreach (ThingDef ingredient in listingredients1)
                {
                    if (ingredient.ingredient != null)
                    {
                        foreach (string tag in ingredient.ingredient.mergeCompatibilityTags)
                        {
                            if (tag == "Condiments")
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
                            if (tag2 == "Condiments")
                            {
                                flagIngredientFoundOnTarget = ingredient2.defName;
                            }
                        }
                    }
                }

                if ((flagIngredientFoundOnSource == flagIngredientFoundOnTarget) || flagIngredientFoundOnSource == "" || flagIngredientFoundOnTarget == "")
                {
                    return true;
                }

                else return false;




            }
            return base.CanStackWith(other);
        }

    }
}
