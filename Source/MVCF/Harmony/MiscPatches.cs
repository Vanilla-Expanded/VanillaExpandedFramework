using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF.Harmony
{
    [HarmonyPatch]
    public class MiscPatches
    {
        [HarmonyPatch(typeof(JobDriver_Wait), "CheckForAutoAttack")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void Postfix_JobDriver_Wait_CheckForAutoAttack(JobDriver_Wait __instance)
        {
            if (__instance.pawn.Downed ||
                __instance.pawn.stances.FullBodyBusy ||
                !__instance.pawn.RaceProps.Animal ||
                !__instance.job.canUseRangedWeapon ||
                __instance.job.def != JobDefOf.Wait_Combat)
                return;
            var currentEffectiveVerb = __instance.pawn.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null || currentEffectiveVerb.verbProps.IsMeleeAttack)
                return;
            var flags = TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (currentEffectiveVerb.IsIncendiary())
                flags |= TargetScanFlags.NeedNonBurning;
            var thing = (Thing) AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, flags);
//            Log.Message("Found target for auto attack: " + thing?.Label);
            if (thing == null)
                return;
            __instance.pawn.TryStartAttack((LocalTargetInfo) thing);
            __instance.collideWithPawns = true;
        }

        [HarmonyPatch(typeof(Pawn), "DrawAt")]
        [HarmonyPostfix]
        public static void Postfix_Pawn_DrawAt(Pawn __instance, Vector3 drawLoc, bool flip = false)
        {
            __instance.Manager().DrawAt(drawLoc);
        }

        [HarmonyPatch(typeof(Pawn), "Tick")]
        [HarmonyPostfix]
        public static void Postfix_Pawn_Tick(Pawn __instance)
        {
            __instance.Manager().Tick();
        }

        [HarmonyPatch(typeof(BattleLogEntry_RangedFire), MethodType.Constructor, typeof(Thing), typeof(Thing),
            typeof(ThingDef), typeof(ThingDef), typeof(bool))]
        [HarmonyPrefix]
        public static void BattleLogEntry_RangedFire_Constructor_Prefix(ref Thing initiator, ThingDef weaponDef)
        {
            if (initiator is IFakeCaster fc) initiator = fc.RealCaster();
        }

        [HarmonyPatch(typeof(BattleLogEntry_RangedImpact), MethodType.Constructor, typeof(Thing), typeof(Thing),
            typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef))]
        [HarmonyPrefix]
        public static void BattleLogEntry_RangedImpact_Constructor_Prefix(ref Thing initiator)
        {
            if (initiator is IFakeCaster fc) initiator = fc.RealCaster();
        }
    }

    public interface IFakeCaster
    {
        Thing RealCaster();
    }
}