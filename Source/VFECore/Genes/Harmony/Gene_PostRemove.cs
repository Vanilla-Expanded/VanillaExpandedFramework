using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(Gene), "PostRemove")]
    public static class VanillaGenesExpanded_Gene_PostRemove_Patch
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
                if (extension.customBloodThingDef != null)
                {
                    StaticCollectionsClass.RemoveBloodtypeGenePawnFromList(gene.pawn);

                }
                if (extension.customBloodIcon != null)
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
                    gene.pawn.health.RemoveHediff(gene.pawn.health.hediffSet.GetFirstHediffOfDef(extension?.hediffToWholeBody));
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


            }
        } 
    
    }
}