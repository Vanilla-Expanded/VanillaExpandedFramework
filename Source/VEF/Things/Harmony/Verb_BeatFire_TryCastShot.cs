using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(Verb_BeatFire), "TryCastShot")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_Verb_BeatFire_TryCastShot_Patch
{
    private static bool Prepare() => DefDatabase<CustomFireDef>.DefCount > 0;

    public static bool Prefix(Verb_BeatFire __instance, ref bool __result)
    {
        if (__instance.CurrentTarget.Thing is not CustomFire fire)
            return true;

        var casterPawn = __instance.CasterPawn;
        if (casterPawn.stances.FullBodyBusy || fire.TicksSinceSpawn == 0)
        {
            __result = false;
            return false;
        }

        fire.TakeDamage(new DamageInfo(fire.Def.extinguishDamageDef, fire.Def.beatFireExtinguishDamageAmount, 0f, -1f, __instance.caster));
        casterPawn.Drawer.Notify_MeleeAttackOn(fire);
        __result = true;
        return false;
    }
}