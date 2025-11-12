using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Weapons
{

    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", [typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)],
    [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    public static class VanillaExpandedFramework_EquipmentUtility_CanEquip_Patch
    {
        public static void Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
        {
            if (__result)
            {
                var options = thing.def.GetModExtension<HeavyWeapon>();
                if (options != null && options.isHeavy)
                {
                    if (!CanEquip(thing, pawn, options))
                    {
                        cantReason = options.disableOptionLabelKey.Translate(pawn.LabelShort);
                        __result = false;
                    }
                }
            }
        }

        public static bool CanEquip(Thing thing, Pawn pawn, HeavyWeapon options)
        {
            if (pawn.story?.traits != null && options.supportedTraits != null)
            {
                if (pawn.story.traits.allTraits.Any(x => options.supportedTraits.Contains(x.def.defName)))
                {
                    return true;
                }
            }
            if (pawn.genes != null && options.supportedGenes != null)
            {
                foreach (string gene in options.supportedGenes)
                {
                    var geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(gene);
                    if (geneDef != null && pawn.genes.HasActiveGene(geneDef))
                    {
                        return true;
                    }
                }

            }
            if (pawn.apparel.WornApparel != null)
            {
                foreach (var ap in pawn.apparel.WornApparel)
                {
                    if (options.supportedArmors?.Contains(ap.def.defName) ?? false)
                    {
                        return true;
                    }
                    if (ap.def.apparel?.bodyPartGroups?.Contains(BodyPartGroupDefOf.Torso) ?? false)
                    {
                        if (ap.def.tradeTags != null)
                        {
                            if (ap.def.tradeTags.Contains("HiTechArmor") 
                                && (thing.def.tradeTags is null || thing.def.tradeTags.Contains("VFEP_WarcasketWeapon")) is false)
                            {
                                return true;
                            }
                            else if (ap.def.tradeTags.Contains("Warcasket"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
