using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Reloading
{
    public class ReloadingGizmos
    {
        public static HarmonyMethod Create => new HarmonyMethod(typeof(ReloadingGizmos), nameof(CreateReloadableVerbTargetCommand));
        public static HarmonyMethod Use => new HarmonyMethod(typeof(ReloadingGizmos), nameof(UseReloadableCommand));

        public static bool CreateReloadableVerbTargetCommand(Thing ownerThing, Verb verb,
            ref Command_VerbTarget __result)
        {
            if (verb.GetReloadable() is IReloadable reloadable)
            {
                var command = new Command_ReloadableVerbTarget(reloadable)
                {
                    defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description.CapitalizeFirst(),
                    icon = ownerThing.def.uiIcon,
                    iconAngle = ownerThing.def.uiIconAngle,
                    iconOffset = ownerThing.def.uiIconOffset,
                    tutorTag = "VerbTarget",
                    verb = verb
                };

                if (verb.caster.Faction != Faction.OfPlayer)
                    command.Disable("CannotOrderNonControlled".Translate());
                else if (verb.CasterIsPawn && verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                    command.Disable(
                        "IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                else if (verb.CasterIsPawn && !verb.CasterPawn.drafter.Drafted)
                    command.Disable(
                        "IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                else if (reloadable.ShotsRemaining < verb.verbProps.burstShotCount)
                    command.Disable("CommandReload_NoAmmo".Translate("ammo".Named("CHARGENOUN"),
                        reloadable.AmmoExample.Named("AMMO"),
                        ((reloadable.MaxShots - reloadable.ShotsRemaining) * reloadable.ItemsPerShot).Named("COUNT")));

                __result = command;

                return false;
            }

            return true;
        }

        public static IEnumerable<Gizmo> UseReloadableCommand(IEnumerable<Gizmo> __result)
        {
            foreach (var gizmo in __result)
                if (gizmo is Command_VerbTarget command && command.verb.GetReloadable() is IReloadable reloadable)
                {
                    var verbReloadable = command.verb;

                    var reloadableVerbTarget = new Command_ReloadableVerbTarget(reloadable)
                    {
                        defaultDesc = command.Desc,
                        defaultLabel = command.Label,
                        icon = command.icon,
                        iconAngle = command.iconAngle,
                        iconOffset = command.iconOffset,
                        tutorTag = "VerbTarget",
                        verb = verbReloadable
                    };

                    if (verbReloadable.caster.Faction != Faction.OfPlayer)
                        reloadableVerbTarget.Disable("CannotOrderNonControlled".Translate());
                    else if (verbReloadable.CasterIsPawn &&
                             verbReloadable.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                        reloadableVerbTarget.Disable(
                            "IsIncapableOfViolence".Translate(verbReloadable.CasterPawn.LabelShort,
                                verbReloadable.CasterPawn));
                    else if (verbReloadable.CasterIsPawn && !verbReloadable.CasterPawn.drafter.Drafted)
                        reloadableVerbTarget.Disable(
                            "IsNotDrafted".Translate(verbReloadable.CasterPawn.LabelShort, verbReloadable.CasterPawn));
                    else if (reloadable.ShotsRemaining < verbReloadable.verbProps.burstShotCount)
                        reloadableVerbTarget.Disable("CommandReload_NoAmmo".Translate("ammo".Named("CHARGENOUN"),
                            reloadable.AmmoExample.Named("AMMO"),
                            ((reloadable.MaxShots - reloadable.ShotsRemaining) * reloadable.ItemsPerShot)
                            .Named("COUNT")));

                    yield return reloadableVerbTarget;
                }
                else
                    yield return gizmo;
        }
    }
}