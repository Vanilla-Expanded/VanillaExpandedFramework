using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VEF.AnimalGenes
{
    public static class AnimalGeneUtility
    {
        public static void AddGene(CompAnimalGenes comp, AnimalGeneDef gene)
        {
            comp.genes.Add(gene);
            comp.ResetCaches();
            Pawn pawn = comp.parent as Pawn;
            if (pawn != null)
            {
                foreach (StatModifier statModifier in gene.statFactors)
                {
                    statModifier.stat.Worker.ClearCacheForThing(pawn);
                }
                foreach (StatModifier statModifier2 in gene.statOffsets)
                {
                    statModifier2.stat.Worker.ClearCacheForThing(pawn);
                }
                if (gene.hediffToAdd != null)
                {
                    pawn.health.AddHediff(gene.hediffToAdd);
                }
                if (gene.abilityToAdd != null)
                {
                    if (pawn.abilities is null)
                    {
                        pawn.abilities = new Pawn_AbilityTracker(pawn);
                    }
                    pawn.abilities.GainAbility(gene.abilityToAdd);
                }
            }
        }

        public static void AddGeneRespectingFamily(CompAnimalGenes comp, AnimalGeneDef gene)
        {
            if (comp.genes.ContainsAny(x => x.familyTag == gene.familyTag))
            {
                RemoveGene(comp, comp.genes.Where(x => x.familyTag == gene.familyTag).First());
            }
            AddGene(comp, gene);
        }

        public static void RemoveGene(CompAnimalGenes comp, AnimalGeneDef gene)
        {
            comp?.genes?.Remove(gene);
            comp.ResetCaches();
            Pawn pawn = comp.parent as Pawn;
            if (pawn != null)
            {
                foreach (StatModifier statModifier in gene.statFactors)
                {
                    statModifier.stat.Worker.ClearCacheForThing(pawn);
                }
                foreach (StatModifier statModifier2 in gene.statOffsets)
                {
                    statModifier2.stat.Worker.ClearCacheForThing(pawn);
                }
                if (gene.hediffToAdd != null)
                {
                    Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(gene.hediffToAdd);
                    if (hediffToRemove != null)
                    {
                        pawn.health.RemoveHediff(hediffToRemove);
                    }
                }
                if (gene.abilityToAdd != null)
                {
                    pawn.abilities.RemoveAbility(gene.abilityToAdd);
                }
            }
        }

        public static int GetTotalStability(CompAnimalGenes comp)
        {
            int totalStability = 0;
            foreach (AnimalGeneDef gene in comp.genes)
            {
                totalStability += gene.stability;
            }
            return totalStability;
        }

        public static void TryDoStillBirth(Pawn pawn, float stillbirthChance)
        {
            if (!Find.Storyteller.difficulty.babiesAreHealthy)
            {
                if (Rand.Chance(stillbirthChance))
                {
                    Find.LetterStack.ReceiveLetter("VRE_StillbornLabel".Translate(pawn.def.label), "VRE_StillbornDesc".Translate(pawn.def.label, pawn.Name.ToString()), LetterDefOf.NeutralEvent, pawn);
                    Hediff culpritHediff = pawn.health.AddHediff(InternalDefOf.VEF_StillbornAnimal);
                    Find.BattleLog.Add(new BattleLogEntry_StateTransition(pawn, pawn.RaceProps.DeathActionWorker.DeathRules, null, culpritHediff, null));
                }
            }
        }

        public static void HandleMutations(CompAnimalGenes comp, Thing pawn)
        {
            int amountOfMutations = 0;
            float roll = Rand.Value;
            if (roll < 0.0005f)
                amountOfMutations = 5;
            else if (roll < 0.0021f) // 0.0005 + 0.0016
                amountOfMutations = 4;
            else if (roll < 0.0066f) // + 0.0045
                amountOfMutations = 3;
            else if (roll < 0.0196f) // + 0.013
                amountOfMutations = 2;
            else if (roll < 0.0616f) // + 0.042 
                amountOfMutations = 1;
            else
                amountOfMutations = 0;
            //For debug testing
            // amountOfMutations = 5;
            if (amountOfMutations > 0)
            {
                List<AnimalGeneDef> mutatedGenes = comp.genes.Where(x => !x.singleRankGene).ToList().InRandomOrder().Take(amountOfMutations).ToList();
                foreach (AnimalGeneDef mutatedGene in mutatedGenes)
                {
                    bool goingUpOrDown = Rand.Chance(0.5f);
                    int geneLevel = mutatedGene.GeneLevel;
                    AnimalGeneFamilyTagDef family = mutatedGene.familyTag;
                    int newGeneLevel = goingUpOrDown ? Math.Min(mutatedGene.GeneLevel + 1, 5) : Math.Max(mutatedGene.GeneLevel - 1, 1);
                    AnimalGeneDef geneToRemove = comp.genes.Where(x => x.familyTag == family && x.GeneLevel == geneLevel).FirstOrDefault();
                    if (geneToRemove != null)
                    {
                        RemoveGene(comp, geneToRemove);
                    }
                    AnimalGeneDef newGene = DefDatabase<AnimalGeneDef>.AllDefsListForReading.Where(x => x.familyTag == family && x.GeneLevel == newGeneLevel).FirstOrDefault();
                    AddGene(comp, newGene);
                }
            }

        }
    }
}
