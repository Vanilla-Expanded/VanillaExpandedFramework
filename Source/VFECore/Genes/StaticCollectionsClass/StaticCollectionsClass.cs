
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VanillaGenesExpanded
{
    [StaticConstructorOnStartup]
    public static class StaticCollectionsClass
    {

        static StaticCollectionsClass() {

            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                GeneExtension extension = geneDef.GetModExtension<GeneExtension>();
                if (extension?.hideGene == true)
                {
                    hidden_genes.Add(geneDef);
                }
            }
        
        
        }

        //This static class stores lists of pawns for different things.

        // A list of pawns containing meat changing genes
        public static IDictionary<Thing, ThingDef> meat_gene_pawns = new Dictionary<Thing, ThingDef>();
        // A list of pawns containing leather changing genes
        public static IDictionary<Thing, ThingDef> leather_gene_pawns = new Dictionary<Thing, ThingDef>();
        // A list of pawns containing blood changing genes
        public static IDictionary<Thing, ThingDef> bloodtype_gene_pawns = new Dictionary<Thing, ThingDef>();
        public static IDictionary<Thing, ThingDef> bloodsmear_gene_pawns = new Dictionary<Thing, ThingDef>();

        // A list of pawns with custom blood icons
        public static IDictionary<Thing, string> bloodIcon_gene_pawns = new Dictionary<Thing, string>();
        // A list of pawns with custom blood effects
        public static IDictionary<Thing, EffecterDef> bloodEffect_gene_pawns = new Dictionary<Thing, EffecterDef>();
        // A list of pawns with custom wounds
        public static IDictionary<Thing, FleshTypeDef> woundsFromFleshtype_gene_pawns = new Dictionary<Thing, FleshTypeDef>();
        // A list of pawns with custom disease progression factors
        public static IDictionary<Thing, float> diseaseProgressionFactor_gene_pawns = new Dictionary<Thing, float>();
        // A list of pawns containing vomit changing genes
        public static IDictionary<Thing, ThingDef> vomitType_gene_pawns = new Dictionary<Thing, ThingDef>();      
        // A list of pawns with custom vomit effects
        public static IDictionary<Thing, EffecterDef> vomitEffect_gene_pawns = new Dictionary<Thing, EffecterDef>();
        // A list of pawns with skills that won't be lost over time
        public static IDictionary<Thing, SkillDef> noSkillLoss_gene_pawns = new Dictionary<Thing, SkillDef>();
        // A list of pawns with a skill loss multiplier
        public static IDictionary<Thing, float> skillLossMultiplier_gene_pawns = new Dictionary<Thing, float>();
        // A list of pawns with skilldegradation
        public static HashSet<Pawn> skillDegradation_gene_pawns = new HashSet<Pawn>();
        // A list of pawns with skills that give recreation when gaining XP
        public static IDictionary<Thing, SkillDef> skillRecreation_gene_pawns = new Dictionary<Thing, SkillDef>();
        // A list of genes that should be hidden on the xenotype editor
        public static HashSet<GeneDef> hidden_genes = new HashSet<GeneDef>();
        // A list of pawns with pregnancy speed modifiers
        public static IDictionary<Thing, float> pregnancySpeedFactor_gene_pawns = new Dictionary<Thing, float>();
        // A list of pawns with swapped gender genes
        public static HashSet<Pawn> swappedgender_gene_pawns = new HashSet<Pawn>();


        public static void AddMeatGenePawnToList(Thing thing, ThingDef thingDef)
        {

            if (!meat_gene_pawns.ContainsKey(thing))
            {
                meat_gene_pawns[thing] = thingDef;
            }
        }

        public static void RemoveMeatGenePawnFromList(Thing thing)
        {
            if (meat_gene_pawns.ContainsKey(thing))
            {
                meat_gene_pawns.Remove(thing);
            }

        }
        public static void AddLeatherGenePawnToList(Thing thing, ThingDef thingDef)
        {

            if (!leather_gene_pawns.ContainsKey(thing))
            {
                leather_gene_pawns[thing] = thingDef;
            }
        }

        public static void RemoveLeatherGenePawnFromList(Thing thing)
        {
            if (leather_gene_pawns.ContainsKey(thing))
            {
                leather_gene_pawns.Remove(thing);
            }

        }

        public static void AddBloodtypeGenePawnToList(Thing thing, ThingDef thingDef)
        {

            if (!bloodtype_gene_pawns.ContainsKey(thing))
            {
                bloodtype_gene_pawns[thing] = thingDef;
            }
        }

        public static void RemoveBloodtypeGenePawnFromList(Thing thing)
        {
            if (bloodtype_gene_pawns.ContainsKey(thing))
            {
                bloodtype_gene_pawns.Remove(thing);
            }

        }

        public static void AddBloodSmearGenePawnToList(Thing thing, ThingDef thingDef)
        {
            if (!bloodsmear_gene_pawns.ContainsKey(thing))
            {
                bloodsmear_gene_pawns[thing] = thingDef;
            }
        }

        public static void RemoveBloodSmearGenePawnFromList(Thing thing)
        {
            if (bloodsmear_gene_pawns.ContainsKey(thing))
            {
                bloodsmear_gene_pawns.Remove(thing);
            }
        }

        public static void AddBloodIconGenePawnToList(Thing thing, string icon)
        {

            if (!bloodIcon_gene_pawns.ContainsKey(thing))
            {
                bloodIcon_gene_pawns[thing] = icon;
            }
        }

        public static void RemoveBloodIconGenePawnFromList(Thing thing)
        {
            if (bloodIcon_gene_pawns.ContainsKey(thing))
            {
                bloodIcon_gene_pawns.Remove(thing);
            }

        }

        public static void AddBloodEffectGenePawnToList(Thing thing, EffecterDef effect)
        {

            if (!bloodEffect_gene_pawns.ContainsKey(thing))
            {
                bloodEffect_gene_pawns[thing] = effect;
            }
        }

        public static void RemoveBloodEffectGenePawnFromList(Thing thing)
        {
            if (bloodEffect_gene_pawns.ContainsKey(thing))
            {
                bloodEffect_gene_pawns.Remove(thing);
            }

        }

        public static void AddWoundsFromFleshtypeGenePawnToList(Thing thing, FleshTypeDef fleshtype)
        {

            if (!woundsFromFleshtype_gene_pawns.ContainsKey(thing))
            {
                woundsFromFleshtype_gene_pawns[thing] = fleshtype;
            }
        }

        public static void RemoveWoundsFromFleshtypeGenePawnFromList(Thing thing)
        {
            if (woundsFromFleshtype_gene_pawns.ContainsKey(thing))
            {
                woundsFromFleshtype_gene_pawns.Remove(thing);
            }

        }

        public static void AddDiseaseProgressionFactorGenePawnToList(Thing thing, float factor)
        {

            if (!diseaseProgressionFactor_gene_pawns.ContainsKey(thing))
            {
                diseaseProgressionFactor_gene_pawns[thing] = factor;
            }
        }

        public static void RemoveDiseaseProgressionFactorGenePawnFromList(Thing thing)
        {
            if (diseaseProgressionFactor_gene_pawns.ContainsKey(thing))
            {
                diseaseProgressionFactor_gene_pawns.Remove(thing);
            }

        }

        public static void AddPregnancySpeedFactorGenePawnToList(Thing thing, float factor)
        {

            if (!pregnancySpeedFactor_gene_pawns.ContainsKey(thing))
            {
                pregnancySpeedFactor_gene_pawns[thing] = factor;
            }
        }

        public static void RemovePregnancySpeedFactorGenePawnFromList(Thing thing)
        {
            if (pregnancySpeedFactor_gene_pawns.ContainsKey(thing))
            {
                pregnancySpeedFactor_gene_pawns.Remove(thing);
            }
        }

        public static void AddVomitTypeGenePawnToList(Thing thing, ThingDef thingDef)
        {

            if (!vomitType_gene_pawns.ContainsKey(thing))
            {
                vomitType_gene_pawns[thing] = thingDef;
            }
        }

        public static void RemoveVomitTypeGenePawnFromList(Thing thing)
        {
            if (vomitType_gene_pawns.ContainsKey(thing))
            {
                vomitType_gene_pawns.Remove(thing);
            }

        }

       

        public static void AddVomitEffectGenePawnToList(Thing thing, EffecterDef effect)
        {

            if (!vomitEffect_gene_pawns.ContainsKey(thing))
            {
                vomitEffect_gene_pawns[thing] = effect;
            }
        }

        public static void RemoveVomitEffectGenePawnFromList(Thing thing)
        {
            if (vomitEffect_gene_pawns.ContainsKey(thing))
            {
                vomitEffect_gene_pawns.Remove(thing);
            }

        }

        public static void AddNoSkillLossGenePawnToList(Thing thing, SkillDef skill)
        {

            if (!noSkillLoss_gene_pawns.ContainsKey(thing))
            {
                noSkillLoss_gene_pawns[thing] = skill;
            }
        }

        public static void RemoveNoSkillLossGenePawnFromList(Thing thing)
        {
            if (noSkillLoss_gene_pawns.ContainsKey(thing))
            {
                noSkillLoss_gene_pawns.Remove(thing);
            }

        }

        public static void AddSkillLossMultiplierGenePawnToList(Thing thing, float multiplier)
        {

            if (!skillLossMultiplier_gene_pawns.ContainsKey(thing))
            {
                skillLossMultiplier_gene_pawns[thing] = multiplier;
            }
        }

        public static void RemoveSkillLossMultiplierGenePawnFromList(Thing thing)
        {
            if (skillLossMultiplier_gene_pawns.ContainsKey(thing))
            {
                skillLossMultiplier_gene_pawns.Remove(thing);
            }

        }

        public static void AddSkillDegradationGenePawnToList(Pawn pawn)
        {

            if (!skillDegradation_gene_pawns.Contains(pawn))
            {
                skillDegradation_gene_pawns.Add(pawn);
            }
        }

        public static void RemoveSkillDegradationGenePawnFromList(Pawn pawn)
        {
            if (skillDegradation_gene_pawns.Contains(pawn))
            {
                skillDegradation_gene_pawns.Remove(pawn);
            }

        }

        public static void AddSwappedGenderGenePawnToList(Pawn pawn)
        {

            if (!swappedgender_gene_pawns.Contains(pawn))
            {
                swappedgender_gene_pawns.Add(pawn);
            }
        }

      

        public static void AddSkillRecreationGenePawnToList(Thing thing, SkillDef skill)
        {

            if (!skillRecreation_gene_pawns.ContainsKey(thing))
            {
                skillRecreation_gene_pawns[thing] = skill;
            }
        }

        public static void RemoveSkillRecreationGenePawnFromList(Thing thing)
        {
            if (skillRecreation_gene_pawns.ContainsKey(thing))
            {
                skillRecreation_gene_pawns.Remove(thing);
            }

        }

    }
}
