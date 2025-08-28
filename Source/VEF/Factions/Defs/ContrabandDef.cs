using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Factions;

public class ContrabandDef: Def
{
        // ThingDefs that are illegal to sell
        public List<ThingDef> things;
        
        // Chemicals that are illegal to sell
        public List<ChemicalDef> chemicals;
        
        // Categories of things that are illegal to sell
        public List<ThingCategoryDef> thingCategories;
        
        // Historical event created when contraband is gifted
        public HistoryEventDef giftedHistoryEvent;
        
        // Historical event created with contraband is sold
        public HistoryEventDef soldHistoryEvent;
        
        // Factions considering the trade/exchange of these goods illegal
        public List<FactionDef> factions;
        
        // Factions considered illegal to trade with
        public List<FactionDef> illegalFactions;

        // Warning message when selecting something to be gifted that matches. Gets passed a def as `ILLEGALTHING`, and a faction as `FACTION` 
        public string giftWarningKey = "VEF.Factions.Contraband_Booze_GiftWarning";
    
        // Warning message when selecting something to be sold that matches. Gets passed a def as `ILLEGALTHING`, and a faction as `FACTION`
        public string sellWarningKey = "VEF.Factions.Contraband_Booze_SellWarning";

        
        // Warning message when selecting something to be gifted that matches when gifted to an illegal faction. Gets passed a def as `ILLEGALTHING`, a faction as `FACTION`, and an illegal faction as `ILLEGALFACTION`  
        public string giftIllegalFactionWarningKey = "VEF.Factions.Contraband_Booze_GiftIllegalWarning";
    
        // Warning message when selecting something to be sold that matches when sold to an illegal faction. Gets passed a def as `ILLEGALTHING`, a faction as `FACTION`, and an illegal faction as `ILLEGALFACTION`
        public string sellIllegalWarningKey = "VEF.Factions.Contraband_Booze_SellIllegalWarning";

        // the type of letter to send
        public LetterDef letterType = LetterDefOf.ThreatSmall;
        
        // Letter Label for the letter sent when goodwill is impacted. Gets passed the faction as `FACTION`
        public string letterLabelKey = "VEF.Factions.Contraband_Booze_LetterLabel";
        // Letter Description for the letter sent when goodwill is impacted through gifting. Gets passed a def as `ILLEGALTHING`, and a faction as `FACTION`
        public string letterDescGiftKey = "VEF.Factions.Contraband_Booze_LetterDescGift";
        // Letter Description for the letter sent when goodwill is impacted through trading. Gets passed a def as `ILLEGALTHING`, and a faction as `FACTION`
        public string letterDescSoldKey = "VEF.Factions.Contraband_Booze_LetterDescSold";
        
        // Letter Description for the letter sent when goodwill is impacted through gifting to an illegal faction. Gets passed a def as `ILLEGALTHING`, a faction as `FACTION`, and an illegal faction as `ILLEGALFACTION`
        public string letterDescGifIllegalFactionKey = "VEF.Factions.Contraband_Booze_LetterDescIllegalGift";
        // Letter Description for the letter sent when goodwill is impacted through trading to an illegal faction. Gets passed a def as `ILLEGALTHING`, a faction as `FACTION`, and an illegal faction as `ILLEGALFACTION`
        public string letterDescSoldIllegalFactionKey = "VEF.Factions.Contraband_Booze_LetterDescIllegalSold";
        
        // Extra info for the letter describing how relations are impacted. Gets passed the faction as `FACTION`, new goodwill as `{1}` and the change as `{2}` 
        public string relationInfoKey = "VEF.Factions.Contraband_Booze_RelationInfo";

        // The chance for the trade/gift to be discovered
        public float chanceToGetCaught = 0.5f;

        // Multiplier to the impact. Defaults to -1 so that impact is negative
        public float impactMultiplier = -1f;
        
        // Days until the sale/gifting is "discovered" by the faction
        public FloatRange daysToImpact = new(7f, 14f);
        
        // The tick at which to trigger discovery
        public int ImpactInTicks => Find.TickManager.TicksGame + 60 + (int)(GenDate.TicksPerDay * daysToImpact.RandomInRange);
        
        public override string ToString()
        {
            // For debugging
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.AppendLine();
            if (!things.NullOrEmpty())
            {
                sb.AppendLine($"Things: {things.Count}");
                foreach (var thing in things)
                {
                    sb.AppendLine($" - {thing.label}");
                }
            }
            if (!chemicals.NullOrEmpty())
            {
                sb.AppendLine($"Chemicals: {chemicals.Count}");
                foreach (var chemical in chemicals)
                {
                    sb.AppendLine($" - {chemical.label}");
                }
            }
            sb.AppendLine($"Gifted History Event: {giftedHistoryEvent}");
            sb.AppendLine($"Sold History Event: {soldHistoryEvent}");
            if (!factions.NullOrEmpty())
            {
                sb.AppendLine($"Factions: {factions.Count}");
                foreach (var faction in factions)
                {
                    sb.AppendLine($" - {faction.label}");
                }
            }
            sb.AppendLine($"Gift Warning: {giftWarningKey}");
            sb.AppendLine($"Sell Warning: {sellWarningKey}");
            sb.AppendLine($"Letter Label: {letterLabelKey}");
            sb.AppendLine($"Letter Description Gift: {letterDescGiftKey}");
            sb.AppendLine($"Letter Description Sold: {letterDescSoldKey}");
            sb.AppendLine($"Relation Info: {relationInfoKey}");
            sb.AppendLine($"Chance To Get Caught: {chanceToGetCaught}");
            
            return sb.ToString();
        }

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
        
        /// <summary>
        // check that this thing isn't contraband either through a direct def match, or category match
        /// </summary>
        /// <param name="thingDef">The thingdef to check</param>
        /// <returns>true if contraband</returns>
        public bool IsThingDefDirectlyContraband(ThingDef thingDef)
        {
            return (!things.NullOrEmpty() && things.Contains(thingDef)) || (!thingCategories.NullOrEmpty() && !thingDef.thingCategories.NullOrEmpty() && thingCategories.Any(tc=>thingDef.thingCategories.Contains(tc)));
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

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
                yield return error;

            if (factions.NullOrEmpty())
            {
                yield return $"No factions targetted in ContrabandDef {defName}";
            }

            if (illegalFactions.NullOrEmpty() && chemicals.NullOrEmpty() && thingCategories.NullOrEmpty())
            {
                yield return $"ContrabandDef {defName} has no illegal items defined";
            }
        }
}