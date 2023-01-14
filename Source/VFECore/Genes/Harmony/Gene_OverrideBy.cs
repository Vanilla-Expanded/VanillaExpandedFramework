using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(Gene), "OverrideBy")]
    public static class VanillaGenesExpanded_Gene_OverrideBy_Patch
    {
        public static void Postfix(Gene __instance, Gene overriddenBy)
        {
            if (overriddenBy!=null)
            {
                RemoveGeneEffects(__instance);
            } else ApplyGeneEffects(__instance);
        }
        public static void RemoveGeneEffects(Gene gene)
        {
            GeneExtension extension = gene.def.GetModExtension<GeneExtension>();
            if (extension != null)
            {
                if (extension.customBloodThingDef != null)
                {
                    StaticCollectionsClass.RemoveBloodtypeGenePawnFromList(gene.pawn);

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
                if (extension.caravanCarryingFactor != 1f)
                {
                    StaticCollectionsClass.RemoveCaravanCarryingFactorGenePawnFromList(gene.pawn);

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
                }
                if (extension.forceMale == true)
                {
                    gene.pawn.gender = Gender.Male;
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


            }
        }

    }
}