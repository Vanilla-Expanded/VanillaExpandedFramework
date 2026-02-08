using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
    public static class VanillaExpandedFramework_SkillRecord_Learn_Patch
    {
        public static void Prefix(SkillRecord __instance, Pawn ___pawn, ref float xp, bool direct = false)
        {
            AddSkillGainModifier(___pawn.apparel?.WornApparel, ref xp);
            AddSkillGainModifier(___pawn.equipment?.AllEquipmentListForReading, ref xp);
        }

        private static void AddSkillGainModifier<T>(List<T> list, ref float xp) where T : Thing
        {
            if (list == null)
                return;

            foreach (var apparel in list)
            {
                var extension = apparel.def.GetModExtension<ApparelExtension>();
                if (extension is { skillGainModifier: not 1f })
                {
                    xp *= extension.skillGainModifier;
                }
            }
        }
    }
}