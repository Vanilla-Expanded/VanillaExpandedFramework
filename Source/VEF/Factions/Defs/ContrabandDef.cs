using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Factions;

public class ContrabandDef: Def
{
        public List<ThingDef> things;
        public List<ChemicalDef> chemicals;
        public HistoryEventDef giftedHistoryEvent;
        public HistoryEventDef soldHistoryEvent;
        public List<FactionDef> factions;

        public string giftWarning = "Gifting {ILLEGALTHING_label} is against {FACTION_name} law. This may or may not affect your relations with them in the future";
        public string sellWarning = "Selling {ILLEGALTHING_label} is against {FACTION_name} law. This may or may not affect your relations with them in the future";

        public string letterLabel = "{FACTION_name} angered";
        public string letterDescGift = "{FACTION_name} caught wind that you have been gifting {ILLEGALTHING_label} behind their back. They consider it to be against the law, and your relations have degraded";
        public string letterDescSold = "{FACTION_name} caught wind that you have been selling {ILLEGALTHING_label} behind their back. They consider it to be against the law, and your relations have degraded";

        public string relationInfo = "Goodwill with {FACTION_name} is now {1}. (Reduced by {2})";

        public float chanceToGetCaught = 0.5f;


        /// <summary>
        /// Checks if a ThingDef is considered contraband. Checks the thing itself, and drug comps, and ingredients/costs.
        /// </summary>
        /// <param name="thingDef">The ThingDef to check</param>
        /// <param name="count">number of contraband items found (1 if the thing is contraband, otherwise it's the number of items required to make the thing)</param>
        /// <param name="contrabandThingDef">The specific contraband ThingDef that matched if found</param>
        /// <param name="contrabandChemicalDef">The specific contraband ChemicalDef that matched if found</param>
        /// <returns>True if the ThingDef is contraband false otherwise</returns>
        public bool IsThingDefContraband(ThingDef thingDef, out int count, out ThingDef contrabandThingDef, out ChemicalDef contrabandChemicalDef)
        {
            count = 0;
            contrabandThingDef = null;
            contrabandChemicalDef = null;

            // check the thing def directly
            if (IsThingDefDirectlyContraband(thingDef))
            {
                contrabandThingDef = thingDef;
                count += 1;
                return true;
            }
            if (IsThingDefChemicalDirectlyContraband(thingDef, out contrabandChemicalDef))
            {
                count += 1;
                return true;
            }

            // Check each item in the cost list for thingDef to see if it's contraband
            if (thingDef.costList != null)
            {
                foreach (var thingDefCountClass in thingDef.costList)
                {
                    if (contrabandThingDef != null && thingDefCountClass.thingDef == contrabandThingDef)
                    {
                        count += thingDefCountClass.count;
                    }else if (contrabandChemicalDef != null &&
                              IsThingDefChemicalDirectlyContraband(thingDefCountClass.thingDef,
                                  out ChemicalDef chemicalDef) && chemicalDef == contrabandChemicalDef)
                    {
                        count += thingDefCountClass.count;
                    }else if (IsThingDefContraband(thingDefCountClass.thingDef, out int _, out contrabandThingDef, out contrabandChemicalDef))
                    {
                        count += thingDefCountClass.count;
                        
                    }
                }
                
                // better matches existing behaviour in psycasts to return here 
                return count > 0;
            }
            

            // Check if any ingredients for the thing def are contraband
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs.Where(r=>r.ProducedThingDef == thingDef))
            {
                foreach (IngredientCount ingredientCount in recipe.ingredients.Where(x=>x.IsFixedIngredient))
                {
                    if (IsThingDefDirectlyContraband(ingredientCount.FixedIngredient))
                    {
                        contrabandThingDef = ingredientCount.FixedIngredient;
                        count += Mathf.CeilToInt(ingredientCount.GetBaseCount());
                    }
                }
            }

            return count > 0;
        }
        
        public bool IsThingDefDirectlyContraband(ThingDef thingDef)
        {
            return !things.NullOrEmpty() && things.Contains(thingDef);
        }

        /// <summary>
        /// Checks if a ThingDef contains an contraband chemical component
        /// </summary>
        /// <param name="thingDef">The ThingDef to check</param>
        /// <param name="contrabandChemicalDef">the matching contraband ChemicalDef if found</param>
        /// <returns>True if the ThingDef contains an contraband chemical, false otherwise</returns>
        public bool IsThingDefChemicalDirectlyContraband(ThingDef thingDef,
            out ChemicalDef contrabandChemicalDef)
        {
            contrabandChemicalDef = null;
            if (!chemicals.NullOrEmpty() && thingDef.HasComp<CompDrug>())
            {
                foreach (CompProperties_Drug compProperties in thingDef.comps.OfType<CompProperties_Drug>())
                {
                    if (chemicals.Contains(compProperties.chemical))
                    {
                        contrabandChemicalDef = compProperties.chemical;
                        return true;
                    }
                }
            }

            return false;
        }
}