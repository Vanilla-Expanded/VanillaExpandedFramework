using System.Collections.Generic;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Grammar;

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
            __instance.Manager(false)?.DrawAt(drawLoc);
        }

        [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
        [HarmonyPostfix]
        public static void Postfix_Pawn_SpawnSetup(Pawn __instance)
        {
            var man = __instance.Manager();
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.GetComp().TickManagers.Add(new System.WeakReference<VerbManager>(man));
        }

        [HarmonyPatch(typeof(Pawn), "DeSpawn")]
        [HarmonyPostfix]
        public static void Postfix_Pawn_DeSpawn(Pawn __instance)
        {
            var man = __instance.Manager(false);
            if (man == null) return;
            if (man.NeedsTicking)
                WorldComponent_MVCF.GetComp().TickManagers.RemoveAll(wr =>
                {
                    if (!wr.TryGetTarget(out var vm)) return true;
                    return vm == man;
                });
        }

        [HarmonyPatch(typeof(BattleLogEntry_RangedFire), MethodType.Constructor, typeof(Thing), typeof(Thing),
            typeof(ThingDef), typeof(ThingDef), typeof(bool))]
        [HarmonyPrefix]
        public static void BattleLogEntry_RangedFire_Constructor_Prefix(ref Thing initiator, Thing target,
            ref ThingDef weaponDef, ThingDef projectileDef)
        {
            if (initiator is IFakeCaster fc) initiator = fc.RealCaster();
            // if (weaponDef == null)
            // {
            //     weaponDef = ThingDef.Named("MVCF_PlaceholderForHediffs");
            //     weaponDef.Verbs[0].defaultProjectile = projectileDef;
            // }
            //
            // Log.Message("RangedFire with weaponDef " + weaponDef + " projectile def " + projectileDef + " and target " +
            //             target);
        }

        [HarmonyPatch(typeof(PlayLogEntryUtility), "RulesForOptionalWeapon")]
        [HarmonyPostfix]
        public static IEnumerable<Rule> PlayLogEntryUtility_RulesForOptionalWeapon_Postfix(IEnumerable<Rule> __result,
            string prefix, ThingDef weaponDef, ThingDef projectileDef)
        {
            foreach (var rule in __result) yield return rule;
            if (weaponDef != null) yield break;

            // Log.Message("weaponDef null with projectileDef " + projectileDef);
            foreach (var rule in GrammarUtility.RulesForDef(prefix + "_projectile", projectileDef))
                yield return rule;
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