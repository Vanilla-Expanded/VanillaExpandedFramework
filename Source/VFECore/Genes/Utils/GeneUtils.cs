using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using VFECore;

namespace VanillaGenesExpanded
{
    public static class GeneUtils
    {
        public static void ApplyGeneEffects(Gene gene)
        {
            try
            {
                if (gene?.pawn is null) return;
                if (!gene.Active) return;
                if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.Saving) return;
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
                        StaticCollectionsClass.AddSwappedGenderGenePawnToList(gene.pawn);
                    }
                    if (extension.forceMale == true)
                    {
                        gene.pawn.gender = Gender.Male;
                        if (gene.pawn.story?.bodyType == BodyTypeDefOf.Female)
                        {
                            gene.pawn.story.bodyType = BodyTypeDefOf.Male;
                        }
                        StaticCollectionsClass.AddSwappedGenderGenePawnToList(gene.pawn);
                    }

                    if (extension.forcedBodyType != null && gene.pawn.DevelopmentalStage.Adult())
                    {
                        gene.pawn.story.bodyType = extension.forcedBodyType;
                        gene.pawn.Drawer.renderer.SetAllGraphicsDirty();
                        
                    }

                    if (extension.customBloodThingDef != null)
                    {
                        StaticCollectionsClass.AddBloodtypeGenePawnToList(gene.pawn, extension.customBloodThingDef);
                    }
                    if (extension.customBloodSmearThingDef != null)
                    {
                        StaticCollectionsClass.AddBloodSmearGenePawnToList(gene.pawn, extension.customBloodSmearThingDef);
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
                    if (extension.customMeatThingDef != null)
                    {
                        StaticCollectionsClass.AddMeatGenePawnToList(gene.pawn, extension.customMeatThingDef);
                    }
                    if (extension.customLeatherThingDef != null)
                    {
                        StaticCollectionsClass.AddLeatherGenePawnToList(gene.pawn, extension.customLeatherThingDef);
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
            catch (Exception ex)
            {
                Log.Error($"[VEF] Error in GeneUtils.ApplyGeneEffects for gene {gene?.def?.defName.ToStringSafe()}: {ex}");
            }
        }

        public static void RemoveGeneEffects(Gene gene)
        {
            try
            {
                if (gene?.pawn is null) return;
                if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.Saving) return;
                GeneExtension extension = gene.def.GetModExtension<GeneExtension>();
                if (extension != null)
                {
                   
                    if (extension.customBloodThingDef != null)
                    {
                        StaticCollectionsClass.RemoveBloodtypeGenePawnFromList(gene.pawn);
                    }
                    if (extension.customBloodSmearThingDef != null)
                    {
                        StaticCollectionsClass.RemoveBloodSmearGenePawnFromList(gene.pawn);
                    }
                    if (extension.customBloodIcon != "")
                    {
                        StaticCollectionsClass.RemoveBloodIconGenePawnFromList(gene.pawn);

                    }
                    if (extension.customBloodEffect != null)
                    {
                        StaticCollectionsClass.RemoveBloodEffectGenePawnFromList(gene.pawn);

                    }
                    if (extension.customWoundsFromFleshtype != null)
                    {
                        StaticCollectionsClass.RemoveWoundsFromFleshtypeGenePawnFromList(gene.pawn);

                    }
                    if (extension.diseaseProgressionFactor != 1f)
                    {
                        StaticCollectionsClass.RemoveDiseaseProgressionFactorGenePawnFromList(gene.pawn);

                    }
                    if (extension?.hediffToWholeBody != null)
                    {
                        if (gene.pawn.health.hediffSet?.HasHediff(extension?.hediffToWholeBody) == true)
                        {
                            Hediff hediffToRemove = gene.pawn.health.hediffSet.GetFirstHediffOfDef(extension?.hediffToWholeBody);
                            if (hediffToRemove != null)
                            {
                                gene.pawn.health.RemoveHediff(hediffToRemove);
                            }

                        }
                    }
                    if (extension?.hediffsToBodyParts != null)
                    {
                        foreach (HediffToBodyparts hediffToBodypart in extension?.hediffsToBodyParts)
                        {
                            foreach (BodyPartDef bodypart in hediffToBodypart.bodyparts)
                            {
                                if (gene.pawn.health.hediffSet?.HasHediff(hediffToBodypart.hediff) == true)
                                {
                                    Hediff hediffToRemove = gene.pawn.health.hediffSet.GetFirstHediffOfDef(hediffToBodypart.hediff);
                                    if (hediffToRemove != null)
                                    {
                                        gene.pawn.health.RemoveHediff(hediffToRemove);
                                    }

                                }


                            }


                        }

                    }
                    if (extension.customMeatThingDef != null)
                    {
                        StaticCollectionsClass.RemoveMeatGenePawnFromList(gene.pawn);
                    }
                    if (extension.customLeatherThingDef != null)
                    {
                        StaticCollectionsClass.RemoveLeatherGenePawnFromList(gene.pawn);
                    }
                    if (extension.customVomitThingDef != null)
                    {
                        StaticCollectionsClass.RemoveVomitTypeGenePawnFromList(gene.pawn);
                    }

                    if (extension.customVomitEffect != null)
                    {
                        StaticCollectionsClass.RemoveVomitEffectGenePawnFromList(gene.pawn);
                    }

                    if (extension.noSkillLoss != null)
                    {
                        StaticCollectionsClass.RemoveNoSkillLossGenePawnFromList(gene.pawn);
                    }
                    if (extension.skillRecreation != null)
                    {
                        StaticCollectionsClass.RemoveSkillRecreationGenePawnFromList(gene.pawn);
                    }
                    if (extension.globalSkillLossMultiplier != 1f)
                    {
                        StaticCollectionsClass.RemoveSkillLossMultiplierGenePawnFromList(gene.pawn);
                    }
                    if (extension.skillDegradation != false)
                    {
                        StaticCollectionsClass.RemoveSkillDegradationGenePawnFromList(gene.pawn);
                    }
                    if (extension.pregnancySpeedFactor != 1f)
                    {
                        StaticCollectionsClass.RemovePregnancySpeedFactorGenePawnFromList(gene.pawn);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[VEF] Error in GeneUtils.RemoveGeneEffects for gene {gene?.def?.defName.ToStringSafe()}: {ex}");
            }
        }
    }
}