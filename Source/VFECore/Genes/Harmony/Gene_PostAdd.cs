using HarmonyLib;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    public static class PawnGenerator_GenerateGenes_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.genes != null)
            {
                List<Gene> genes = pawn.genes.GenesListForReading;
                foreach (Gene gene in genes)
                {
                    if (gene.Active)
                    {
                        VanillaGenesExpanded_Gene_PostAdd_Patch.ApplyGeneEffects(gene);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Gene), "PostAdd")]
    public static class VanillaGenesExpanded_Gene_PostAdd_Patch
    {
        public static void Postfix(Gene __instance)
        {
            if (PawnGenerator.IsBeingGenerated(__instance.pawn) is false && __instance.Active)
            {
                ApplyGeneEffects(__instance);
            }
        }
        public static void ApplyGeneEffects(Gene gene)
        {
            GeneExtension extension = gene.def.GetModExtension<GeneExtension>();
            if (extension != null)
            {
                if (extension.forceFemale == true)
                {
                    gene.pawn.gender = Gender.Female;
                    if (gene.pawn.story?.bodyType == BodyTypeDefOf.Male)
                    {
                        gene.pawn.story.bodyType = BodyTypeDefOf.Female;
                    }
                }
                if (extension.forceMale == true)
                {
                    gene.pawn.gender = Gender.Male;
                    if (gene.pawn.story?.bodyType == BodyTypeDefOf.Female)
                    {
                        gene.pawn.story.bodyType = BodyTypeDefOf.Male;
                    }
                }

                if (extension.forcedBodyType != null)
                {
                    gene.pawn.story.bodyType = extension.forcedBodyType;
                    gene.pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                }

                if (extension.customBloodThingDef != null)
                {
                    StaticCollectionsClass.AddBloodtypeGenePawnToList(gene.pawn, extension.customBloodThingDef);
                }
                if (extension.customBloodIcon != "")
                {
                    StaticCollectionsClass.AddBloodIconGenePawnToList(gene.pawn, extension.customBloodIcon);
                }
                if (extension.customBloodEffect != null)
                {
                    StaticCollectionsClass.AddBloodEffectGenePawnToList(gene.pawn, extension.customBloodEffect);
                }
                if (extension.customWoundsFromFleshtype != null)
                {
                    StaticCollectionsClass.AddWoundsFromFleshtypeGenePawnToList(gene.pawn, extension.customWoundsFromFleshtype);
                }
                if (extension.caravanCarryingFactor != 1f)
                {
                    StaticCollectionsClass.AddCaravanCarryingFactorGenePawnToList(gene.pawn, extension.caravanCarryingFactor);
                }
                if (extension.diseaseProgressionFactor != 1f)
                {
                    StaticCollectionsClass.AddDiseaseProgressionFactorGenePawnToList(gene.pawn, extension.diseaseProgressionFactor);
                }
                if (extension.hediffToWholeBody != null)
                {
                    gene.pawn.health.AddHediff(extension?.hediffToWholeBody);

                }
                if (extension.hediffsToBodyParts != null)
                {

                    foreach (HediffToBodyparts hediffToBodypart in extension?.hediffsToBodyParts)
                    {
                        int enumerator = 0;
                        foreach (BodyPartDef bodypart in hediffToBodypart.bodyparts)
                        {
                            if (!gene.pawn.RaceProps.body.GetPartsWithDef(bodypart).EnumerableNullOrEmpty())
                            {
                                if (enumerator <= gene.pawn.RaceProps.body.GetPartsWithDef(bodypart).Count)
                                {
                                    gene.pawn.health.AddHediff(hediffToBodypart.hediff, gene.pawn.RaceProps.body.GetPartsWithDef(bodypart).ToArray()[enumerator]);
                                    enumerator++;
                                }

                            }

                        }
                    }
                }
                if (extension.customVomitThingDef != null)
                {
                    StaticCollectionsClass.AddVomitTypeGenePawnToList(gene.pawn, extension.customVomitThingDef);
                }
               
                if (extension.customVomitEffect != null)
                {
                    StaticCollectionsClass.AddVomitEffectGenePawnToList(gene.pawn, extension.customVomitEffect);
                }

                if (extension.noSkillLoss != null)
                {
                    StaticCollectionsClass.AddNoSkillLossGenePawnToList(gene.pawn, extension.noSkillLoss);
                }
                if (extension.skillRecreation != null)
                {
                    StaticCollectionsClass.AddSkillRecreationGenePawnToList(gene.pawn, extension.skillRecreation);
                }
                if (extension.globalSkillLossMultiplier != 1f)
                {
                    StaticCollectionsClass.AddSkillLossMultiplierGenePawnToList(gene.pawn, extension.globalSkillLossMultiplier);
                }
                if (extension.skillDegradation != false)
                {
                    StaticCollectionsClass.AddSkillDegradationGenePawnToList(gene.pawn);
                }
                if (extension.pregnancySpeedFactor != 1f)
                {
                    StaticCollectionsClass.AddPregnancySpeedFactorGenePawnToList(gene.pawn, extension.pregnancySpeedFactor);
                }


            }
        }
    }
}