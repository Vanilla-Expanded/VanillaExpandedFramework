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
        
        // Ignore a thing's stuff when checking for contraband
        public bool ignoreStuff = false;
        
        // Ignore a things recipes or costlist when checking for contraband
        public bool ignoreRecipes = false;
        
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
        public string sellIllegalFactionWarningKey = "VEF.Factions.Contraband_Booze_SellIllegalWarning";

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
        
        public virtual IEnumerable<Def> AllContraband()
        {
            if(!things.NullOrEmpty())
                foreach(ThingDef thingDef in things)
                    yield return thingDef;
            
            if(!chemicals.NullOrEmpty())
                foreach(ChemicalDef chemicalDef in chemicals)
                    yield return chemicalDef;
    
            if(!thingCategories.NullOrEmpty())        
                foreach(ThingCategoryDef thingCategoryDef in thingCategories)
                    yield return thingCategoryDef;
        }
        
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
        /// <param name="thing">The Thing to check</param>
        /// <param name="count">number of contraband items found (1 if the thing is contraband, otherwise it's the number of items required to make the thing)</param>
        /// <param name="contrabandThingDef">The specific contraband ThingDef that matched if found</param>
        /// <param name="contrabandChemicalDef">The specific contraband ChemicalDef that matched if found</param>
        /// <returns>True if the ThingDef is contraband false otherwise</returns>
        public bool IsThingContraband(Thing thing, out int count, out ThingDef contrabandThingDef, out ChemicalDef contrabandChemicalDef)
        {
            count = 0;
            contrabandThingDef = null;
            contrabandChemicalDef = null;

            // check the thing def directly
            if (IsThingDirectlyContraband(thing))
            {
                contrabandThingDef = thing.def;
                count += 1;
                return true;
            }
            if (IsThingDefChemicalDirectlyContraband(thing.def, out contrabandChemicalDef))
            {
                count += 1;
                return true;
            }
            
            if (!ignoreRecipes) return false;
            
            // Check each item in the cost list for thingDef to see if it's contraband
            if (thing.def.costList != null)
            {
                foreach (var thingDefCountClass in thing.def.costList)
                {
                    if (contrabandThingDef != null && thingDefCountClass.thingDef == contrabandThingDef)
                    {
                        count += thingDefCountClass.count;
                    }else if (contrabandChemicalDef != null &&
                              IsThingDefChemicalDirectlyContraband(thingDefCountClass.thingDef,
                                  out ChemicalDef chemicalDef) && chemicalDef == contrabandChemicalDef)
                    {
                        count += thingDefCountClass.count;
                    }else if (things.Contains(thingDefCountClass.thingDef))
                    {
                        count += thingDefCountClass.count;
                        
                    }
                }
                
                // better matches existing behaviour in psycasts to return here 
                return count > 0;
            }

            // Check if any ingredients for the thing def are contraband
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs.Where(r=>r.ProducedThingDef == thing.def))
            {
                foreach (IngredientCount ingredientCount in recipe.ingredients.Where(x=>x.IsFixedIngredient))
                {
                    if (things.Contains(ingredientCount.FixedIngredient))
                    {
                        contrabandThingDef = ingredientCount.FixedIngredient;
                        count += Mathf.CeilToInt(ingredientCount.GetBaseCount());
                    }
                }
            }

            return count > 0;
        }
        
        /// <summary>
        /// check that this thing isn't contraband either through a direct def match, stuff match, or category match
        /// </summary>
        /// <param name="thing">The Thing to check</param>
        /// <returns>true if contraband</returns>
        public bool IsThingDirectlyContraband(Thing thing)
        {
            if (!things.NullOrEmpty())
            {
                if (things.Contains(thing.def)) return true;
                
                if(!ignoreStuff && thing.Stuff != null && things.Contains(thing.Stuff)) return true;
            }

            if (thingCategories.NullOrEmpty()) return false;
            if(!thing.def.thingCategories.NullOrEmpty() && thingCategories.Any(tc=>thing.def.thingCategories.Contains(tc)))  return true;
                
            return !ignoreStuff && thing.Stuff != null && thingCategories.Any(tc=>thing.Stuff.thingCategories.Contains(tc));
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
            if (chemicals.NullOrEmpty() || !thingDef.HasComp<CompDrug>()) return false;
            foreach (CompProperties_Drug compProperties in thingDef.comps.OfType<CompProperties_Drug>())
            {
                if (!chemicals.Contains(compProperties.chemical)) continue;
                contrabandChemicalDef = compProperties.chemical;
                return true;
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