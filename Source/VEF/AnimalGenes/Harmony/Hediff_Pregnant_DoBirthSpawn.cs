using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using System;

namespace VEF.AnimalGenes
{
    [HarmonyPatch(typeof(Hediff_Pregnant))]
    [HarmonyPatch("DoBirthSpawn")]
    public static class VEF_AnimalGenes_Hediff_Pregnant_DoBirthSpawn_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddCrossbreedGenes(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var adjustGenesTargetMethod = AccessTools.Method(typeof(TaleRecorder), "RecordTale");
            var adjustLitterSizeTargetMethod = AccessTools.Method(typeof(Rand), "ByCurve");

            var adjustGenesMethod = AccessTools.Method(typeof(VEF_AnimalGenes_Hediff_Pregnant_DoBirthSpawn_Patch), "AdjustGenes");
            var adjustLitterSizeMethod = AccessTools.Method(typeof(VEF_AnimalGenes_Hediff_Pregnant_DoBirthSpawn_Patch), "AdjustLitterSize");

            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 0 && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].OperandIs(adjustGenesTargetMethod))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, adjustGenesMethod);
                }
                else
                if (codes[i].opcode == OpCodes.Call && codes[i].OperandIs(adjustLitterSizeTargetMethod))
                {


                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, adjustLitterSizeMethod);
                    yield return codes[i];
                }
                else yield return codes[i];
            }
        }

        public static void AdjustGenes(Pawn pawn, Pawn mother, Pawn father)
        {
            if (!WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn)) { return; }
            CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
            if (comp is null) { return; }
            if (mother is null || !WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(mother)) { return; }
            CompAnimalGenes compMother = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[mother];
            if (compMother is null) { return; }

            HashSet<AnimalGeneFamilyTagDef> childFamilies = comp.genes.Select(x => x.familyTag).ToHashSet();
            if (father is null || !WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(father))
            {
                comp.genes = compMother.genes.ToList();
                return;
            }
            CompAnimalGenes compFather = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[father];
            if (compFather is null) { return; }

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

            comp.genes.Clear();

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
                    AnimalGeneUtility.AddGene(comp, inheritedGene);
                }

                // Stillbirth calculations
                float stillbirthChance = motherGene?.stillbirthChance ?? fatherGene.stillbirthChance;
                AnimalGeneUtility.TryDoStillBirth(pawn, stillbirthChance);
            }

            // Random mutations handling
            if (!pawn.Dead)
            {
                AnimalGeneUtility.HandleMutations(comp, pawn);
            }
        }

        public static SimpleCurve AdjustLitterSize(SimpleCurve existingCurve, Pawn mother)
        {

            if (mother is null || !WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(mother)) { return existingCurve; }
            CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[mother];
            if (comp != null)
            {
                foreach (AnimalGeneDef motherAnimalGene in comp.genes)
                {
                    if (motherAnimalGene.litterSizeCurveOverride != null)
                    {
                        return motherAnimalGene.litterSizeCurveOverride;
                    }
                }
            }
            return existingCurve;
        }



    }
}