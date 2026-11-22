using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using VEF.AnimalGenes;
using Verse;


namespace VEF.AnimalGenes
{

    // This Harmony patch will only be patched if AnimalGenesMechanics is added via XML to a mod using OptionalFeatures

    public class VEF_AnimalGenes_CompEggLayer_ProduceEgg_Patch
    {
     
        public static void ModifyEggs(CompEggLayer __instance, Pawn ___fertilizedBy, ref Thing __result)
        {
            CompHatcher compHatcher = __result.TryGetComp<CompHatcher>();
            if (compHatcher == null) { return; }

            Pawn mother = __instance.parent as Pawn;
            Pawn father = ___fertilizedBy;
            PawnKindDef pawn = compHatcher.Props.hatcherPawn;

            if (__result?.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes comp) return;
            if (mother?.TryGetComp<CompAnimalGenes>() is  not CompAnimalGenes compMother) return;
          
            var defaultGenes = CompAnimalGenes.FindFeratype(pawn)?.animalGenes;
            HashSet<AnimalGeneFamilyTagDef> childFamilies = (defaultGenes ?? comp.genes).Select(x => x.familyTag).ToHashSet();
            if (father?.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes compFather)
            {
                if (compMother.HasAssignedGenes)
                    comp.SetInheritedGenes(compMother.genes);
                return;
            }

            List<AnimalGeneDef> motherGenes = compMother.genes;
            List<AnimalGeneDef> fatherGenes = compFather.genes;

            HashSet<AnimalGeneFamilyTagDef> geneFamilies = motherGenes.Select(x => x.familyTag).ToHashSet();

            geneFamilies.UnionWith(fatherGenes.Select(x => x.familyTag));

            // Stability and pull factor from the parents
            int totalMotherStability = 0;
            foreach (AnimalGeneDef gene in compMother.genes)
            {
                totalMotherStability += gene.stability;
            }
            int totalFatherStability = 0;
            foreach (AnimalGeneDef gene in compFather.genes)
            {
                totalFatherStability += gene.stability;
            }

            float avgStability = (float)(totalMotherStability + totalFatherStability) / 2;
            float pullFactor = 0;
            if (avgStability < 0)
            {
                pullFactor = Math.Min(Math.Abs(avgStability) / WorldComponent_AnimalGenes.maxStabilityPenalty, 1);
            }

            var inheritedGenes = new List<AnimalGeneDef>();

            foreach (AnimalGeneFamilyTagDef familyTag in geneFamilies)
            {
                AnimalGeneDef motherGene = motherGenes.FirstOrDefault(x => x.familyTag == familyTag);

                AnimalGeneDef fatherGene = fatherGenes.FirstOrDefault(x => x.familyTag == familyTag);

                AnimalGeneDef inheritedGene = null;

                // Both parent have the gene for this family

                if (motherGene != null && fatherGene != null)
                {
                    float rawScore = (motherGene.GeneLevel + fatherGene.GeneLevel) / 2f;
                    int finalScore = (int)Math.Round(rawScore + (3f - rawScore) * pullFactor, MidpointRounding.AwayFromZero);
                    inheritedGene = DefDatabase<AnimalGeneDef>.AllDefsListForReading.FirstOrDefault(x => x.familyTag == familyTag && x.GeneLevel == finalScore);
                }
                // Only mother has the gene for this family
                else if (motherGene != null)
                {
                    if (!motherGene.isSpecialized || childFamilies.Contains(familyTag))
                    {
                        int finalScore = (int)Math.Round(motherGene.GeneLevel + (3f - motherGene.GeneLevel) * pullFactor, MidpointRounding.AwayFromZero);
                        inheritedGene = DefDatabase<AnimalGeneDef>.AllDefsListForReading.FirstOrDefault(x => x.familyTag == familyTag && x.GeneLevel == finalScore);
                    }
                }
                // Only father has the gene for this family              
                else if (fatherGene != null)
                {
                    if (!fatherGene.isSpecialized || childFamilies.Contains(familyTag))
                    {
                        int finalScore = (int)Math.Round(fatherGene.GeneLevel + (3f - fatherGene.GeneLevel) * pullFactor, MidpointRounding.AwayFromZero);
                        inheritedGene = DefDatabase<AnimalGeneDef>.AllDefsListForReading.FirstOrDefault(x => x.familyTag == familyTag && x.GeneLevel == finalScore);
                    }
                }

                if (inheritedGene != null)
                {
                    inheritedGenes.Add(inheritedGene);
                }

            }

            // Random mutations handling

            if (compMother.HasAssignedGenes || compFather.HasAssignedGenes)
            {
                comp.SetInheritedGenes(inheritedGenes);
                AnimalGeneUtility.HandleMutations(comp, __result);
            }

        }
    }
}
