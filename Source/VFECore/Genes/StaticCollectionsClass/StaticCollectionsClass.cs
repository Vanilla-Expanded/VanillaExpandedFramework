
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VanillaGenesExpanded
{

    public static class StaticCollectionsClass
    {

        //This static class stores lists of pawns for different things.


        // A list of pawns containing blood changing genes
        public static IDictionary<Thing, ThingDef> bloodtype_gene_pawns = new Dictionary<Thing, ThingDef>();
        // A list of pawns with custom blood icons
        public static IDictionary<Thing, string> bloodIcon_gene_pawns = new Dictionary<Thing, string>();
        // A list of pawns with custom blood effects
        public static IDictionary<Thing, EffecterDef> bloodEffect_gene_pawns = new Dictionary<Thing, EffecterDef>();
        // A list of pawns with custom wounds
        public static IDictionary<Thing, FleshTypeDef> woundsFromFleshtype_gene_pawns = new Dictionary<Thing, FleshTypeDef>();
        // A list of pawns with custom disease progression factors
        public static IDictionary<Thing, float> diseaseProgressionFactor_gene_pawns = new Dictionary<Thing, float>();
        // A list of pawns with custom caravan carrying capacity factors
        public static IDictionary<Thing, float> caravanCarryingFactor_gene_pawns = new Dictionary<Thing, float>();
        // A list of pawns containing vomit changing genes
        public static IDictionary<Thing, ThingDef> vomitType_gene_pawns = new Dictionary<Thing, ThingDef>();      
        // A list of pawns with custom vomit effects
        public static IDictionary<Thing, EffecterDef> vomitEffect_gene_pawns = new Dictionary<Thing, EffecterDef>();
        // A list of pawns with skills that won't be lost over time
        public static IDictionary<Thing, SkillDef> noSkillLoss_gene_pawns = new Dictionary<Thing, SkillDef>();
        // A list of pawns with skills that give recreation when gaining XP
        public static IDictionary<Thing, SkillDef> skillRecreation_gene_pawns = new Dictionary<Thing, SkillDef>();


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

        public static void AddCaravanCarryingFactorGenePawnToList(Thing thing, float factor)
        {

            if (!caravanCarryingFactor_gene_pawns.ContainsKey(thing))
            {
                caravanCarryingFactor_gene_pawns[thing] = factor;
            }
        }

        public static void RemoveCaravanCarryingFactorGenePawnFromList(Thing thing)
        {
            if (caravanCarryingFactor_gene_pawns.ContainsKey(thing))
            {
                caravanCarryingFactor_gene_pawns.Remove(thing);
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
