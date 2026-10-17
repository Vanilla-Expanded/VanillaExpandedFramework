using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(DamageWorker_Extinguish), nameof(DamageWorker_Extinguish.Apply))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public class VanillaExpandedFramework_DamageWorker_Extinguish_Apply_Patch
{
    private static bool Prepare() => CustomFireDef.AnyFireAffectedByVanillaExtinguishDamage;

    private static void Postfix(DamageInfo dinfo, Thing victim)
    {
        if (victim is CustomFire { Destroyed: false } victimFire)
            Extinguish(victimFire, dinfo);

        var attachComp = victim.TryGetComp<CompAttachBase>();
        if (attachComp != null)
        {
            // Reverse for loop, if fire is destroyed we keep looping undisturbed
            for (var i = attachComp.attachments.Count - 1; i >= 0; i--)
            {
                if (attachComp.attachments[i] is CustomFire { Destroyed: false } fire)
                    Extinguish(fire, dinfo);
            }
        }
    }

    private static void Extinguish(CustomFire fire, DamageInfo dinfo)
    {
        if ((dinfo.Def == DamageDefOf.Extinguish && fire.Def.affectedByExtinguishDamage) || dinfo.Def == fire.Def.extinguishDamageDef)
        {
            fire.fireSize -= dinfo.Amount * DamageWorker_ExtinguishCustomFire.DamageAmountToFireSizeRatio;
            if (fire.fireSize < 0.1f)
                fire.Destroy();
        }
    }
}