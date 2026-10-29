using System.Linq;
using HarmonyLib;
using VEF.Weapons;
using Verse;

namespace VEF.Hediffs
{
    [HarmonyPatch(typeof(Stance_Warmup), MethodType.Constructor, new[] { typeof(int), typeof(LocalTargetInfo), typeof(Verb) })]
    public static class VanillaExpandedFramework_Stance_Warmup_Constructor_Patch
    {
        public static void Postfix(Stance_Warmup __instance, LocalTargetInfo focusTarg, Verb verb)
        {
            if (focusTarg.Thing is not Pawn target || verb?.caster is not Pawn caster)
            {
                return;
            }
            var lockTicks = verb.GetProjectile()?.GetCompProperties<CompProperties_HomingProjectile>()?.targetLockTicks;
            if (lockTicks == null)
            {
                return;
            }
            var lockComp = caster.health.hediffSet.hediffs
                .Select(hediff => hediff.TryGetComp<HediffComp_Targeting>())
                .FirstOrDefault(comp => comp?.Props.maintainLockAfterShot == true);
            if (lockComp == null)
            {
                return;
            }
            if (lockComp.LockMaintained)
            {
                if (lockComp.lockedTarget == target)
                {
                    return;
                }
                lockComp.DropLock();
            }
            if (HediffComp_Targeting.SkipsTargetLock(target))
            {
                return;
            }
            __instance.ticksLeft += lockTicks.Value;
        }
    }
}