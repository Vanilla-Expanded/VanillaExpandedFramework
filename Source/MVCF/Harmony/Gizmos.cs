using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    public class Pawn_DraftController_GetGizmos
    {
        // ReSharper disable InconsistentNaming
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
            // ReSharper enable InconsistentNaming
        {
            foreach (var gizmo in __result) yield return gizmo;

            if (!__instance.Drafted || !__instance.pawn.AllRangedVerbsPawnNoEquipment().Any()) yield break;
            yield return new Command_Toggle
            {
                hotKey = KeyBindingDefOf.Misc6,
                isActive = () => __instance.FireAtWill,
                toggleAction = () => { __instance.FireAtWill = !__instance.FireAtWill; },
                icon = TexCommand.FireAtWill,
                defaultLabel = "CommandFireAtWillLabel".Translate(),
                defaultDesc = "CommandFireAtWillDesc".Translate(),
                tutorTag = "FireAtWillToggle"
            };
        }
    }

    [HarmonyPatch(typeof(PawnAttackGizmoUtility), "GetAttackGizmos")]
    public class PawnAttackGizmoUtility_GetAttackGizmos
    {
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn pawn)
        {
            foreach (var gizmo in __result) yield return gizmo;

            if (pawn.AllRangedVerbsPawn().Count() >= 2) yield return pawn.GetMainAttackGizmoForPawn();

            foreach (var gizmo in from verb in pawn.Manager().ManagedVerbs
                where (verb.Source == VerbSource.Hediff || verb.Source == VerbSource.RaceDef) &&
                      verb.Verb.verbProps.hasStandardCommand
                from gizmo in verb.Verb
                    .GetGizmosForVerb(verb)
                select gizmo)
                yield return gizmo;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var gizmo in __result) yield return gizmo;

            if (!__instance.RaceProps.Animal || __instance.Faction != Faction.OfPlayer) yield break;
            foreach (var mv in __instance.Manager().ManagedVerbs.Where(mv => !mv.Verb.IsMeleeAttack))
                yield return new Command_ToggleVerbUsage(mv);
        }
    }
}