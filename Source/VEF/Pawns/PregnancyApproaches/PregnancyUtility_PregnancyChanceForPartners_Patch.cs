using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace VEF.Pawns
{
    [StaticConstructorOnStartup]
    public static class VanillaExpandedFramework_PregnancyUtility_PregnancyChanceForPartners_Patch
    {
        public static MethodBase pregnancyChanceForPartnersWithoutPregnancyApproachInfo = AccessTools.Method(typeof(PregnancyUtility), "PregnancyChanceForPartnersWithoutPregnancyApproach");
        static VanillaExpandedFramework_PregnancyUtility_PregnancyChanceForPartners_Patch()
        {
            VEF_Mod.harmonyInstance.Patch(AccessTools.Method(typeof(PregnancyUtility), "PregnancyChanceForPartners"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(VanillaExpandedFramework_PregnancyUtility_PregnancyChanceForPartners_Patch), nameof(Postfix))));
        }

        public static void Postfix(Pawn woman, Pawn man, ref float __result)
        {
            if (woman.gender != man.gender)
            {
                var data = woman.relations.GetAdditionalPregnancyApproachData();
                if (data.partners.TryGetValue(man, out var def))
                {
                    if (def.pregnancyChanceForPartners.HasValue)
                    {
                        __result = def.pregnancyChanceForPartners.Value;
                    }
                    else if (def.pregnancyChanceFactorBase.HasValue)
                    {
                        float num = (float)pregnancyChanceForPartnersWithoutPregnancyApproachInfo.Invoke(null, new[] { woman, man });
                        float pregnancyChanceFactor = def.pregnancyChanceFactorBase.Value;
                        __result = num * pregnancyChanceFactor;
                    }
                }
            }
        }
    }
}
