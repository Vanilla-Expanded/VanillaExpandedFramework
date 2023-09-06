using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(Gene), "ExposeData")]
    public static class VanillaGenesExpanded_Gene_ExposeData_Patch
    {
        public static void Postfix(Gene __instance)
        {
            if (__instance.pawn != null && PawnGenerator.IsBeingGenerated(__instance.pawn) is false && __instance.Active)
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


            }
        }
    }
}