using System.Collections.Generic;
using System.Linq;
using MVCF.Commands;
using MVCF.Comps;
using MVCF.Features;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.Utilities;

public static class PawnVerbGizmoUtility
{
    public static readonly Dictionary<string, string> __truncateCache = new();

    public static IEnumerable<Gizmo> GetGizmosForVerb(this Verb verb, ManagedVerb man = null)
    {
        var props = man?.Props;

        Thing ownerThing = null;
        switch (verb.DirectOwner)
        {
            case ThingWithComps twc when twc.TryGetComp<Comp_VerbGiver>() is { } giver:
                ownerThing = twc;
                props = giver.PropsFor(verb);
                break;
            case Thing thing:
                ownerThing = thing;
                break;
            case Comp_VerbGiver comp:
                ownerThing = comp.parent;
                props = comp.PropsFor(verb);
                break;
            case CompEquippable eq:
                ownerThing = eq.parent;
                break;
            case HediffComp_ExtendedVerbGiver hediffGiver:
                props = hediffGiver.PropsFor(verb);
                break;
        }

        if (man != null)
        {
            foreach (var gizmo1 in man.GetGizmos(ownerThing)) yield return gizmo1;
            yield break;
        }

        Command gizmo;
        var command = new Command_VerbTarget { verb = verb };
        gizmo = command;


        if (ownerThing != null)
        {
            gizmo.defaultDesc = FirstNonEmptyString(props?.description, ownerThing.def.LabelCap + ": " +
                                                                        ownerThing
                                                                           .def?.description?
                                                                           .Truncate(500, __truncateCache)
                                                                        ?
                                                                       .CapitalizeFirst());
            gizmo.icon = verb.Icon(props, ownerThing, false);
        }
        else if (verb.DirectOwner is HediffComp_VerbGiver hediffGiver)
        {
            var hediff = hediffGiver.parent;
            gizmo.defaultDesc = FirstNonEmptyString(props?.description, hediff.def.LabelCap + ": " +
                                                                        hediff.def.description
                                                                           .Truncate(500, __truncateCache)
                                                                           .CapitalizeFirst());
            gizmo.icon = verb.Icon(props, null, false);
        }

        gizmo.tutorTag = "VerbTarget";
        gizmo.defaultLabel = verb.Label(props);

        if (verb.Caster.Faction != Faction.OfPlayer)
            gizmo.Disable("CannotOrderNonControlled".Translate());
        else if (verb.CasterIsPawn)
        {
            if (verb.verbProps.violent && verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                gizmo.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort,
                    verb.CasterPawn));
            else if (verb.CasterPawn.drafter != null && !verb.CasterPawn.drafter.Drafted &&
                     !(props != null && props.canFireIndependently))
                gizmo.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort,
                    verb.CasterPawn));
            else if (verb.CasterPawn.InMentalState && !(props != null && props.canFireIndependently))
                gizmo.Disable("CannotOrderNonControlled".Translate());
        }

        yield return gizmo;


        if ((props != null && props.canBeToggled && man != null && verb.caster.Faction == Faction.OfPlayer) ||
            (verb.CasterIsPawn && verb.CasterPawn.RaceProps.Animal))
        {
            if ((props != null && props.separateToggle) ||
                (verb.CasterIsPawn && verb.CasterPawn.RaceProps.Animal))
                yield return new Command_ToggleVerbUsage(man);
            else if (!MVCF.GetFeature<Feature_IntegratedToggle>().Enabled)
            {
                Log.ErrorOnce(
                    "[MVCF] " + (verb.EquipmentSource.LabelShortCap ?? "Hediff verb of " + verb.caster) +
                    " wants an integrated toggle but that feature is not enabled. Using seperate toggle.",
                    verb.GetHashCode());
                yield return new Command_ToggleVerbUsage(man);
            }
        }
    }

    public static Gizmo GetMainAttackGizmoForPawn(this Pawn pawn)
    {
        var verbs = pawn.Manager().ManagedVerbs;
        var gizmo = new Command_Action
        {
            defaultDesc = "Attack",
            hotKey = KeyBindingDefOf.Misc1,
            icon = TexCommand.SquadAttack,
            action = () =>
            {
                Find.Targeter.BeginTargeting(TargetingParameters.ForAttackAny(), target =>
                {
                    var manager = pawn.Manager();
                    manager.CurrentVerb = null;
                    var verb = pawn.BestVerbForTarget(target, verbs.Where(v => v.Enabled && !v.Verb.IsMeleeAttack));
                    verb.OrderForceTarget(target);
                }, pawn, null, TexCommand.Attack);
            }
        };

        if (pawn.Faction != Faction.OfPlayer)
            gizmo.Disable("CannotOrderNonControlled".Translate());
        if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            gizmo.Disable("IsIncapableOfViolence".Translate((NamedArgument)pawn.LabelShort, (NamedArgument)pawn));
        else if (!pawn.Drafted)
            gizmo.Disable("IsNotDrafted".Translate((NamedArgument)pawn.LabelShort, (NamedArgument)pawn));

        return gizmo;
    }

    private static string VerbLabel(Verb verb, AdditionalVerbProps props = null) =>
        FirstNonEmptyString(props?.visualLabel, verb.verbProps.label,
            (verb as Verb_LaunchProjectile)?.Projectile.LabelCap, verb.caster?.def?.label);

    public static string FirstNonEmptyString(params string[] strings)
    {
        foreach (var s in strings)
            if (!s.NullOrEmpty())
                return s;
        return "";
    }

    public static string Label(this Verb verb, AdditionalVerbProps props = null) => VerbLabel(verb, props).CapitalizeFirst();

    public static Texture2D Icon(this Verb verb, AdditionalVerbProps props, Thing ownerThing, bool toggle)
    {
        if (toggle && props?.ToggleIcon != null && props.ToggleIcon != BaseContent.BadTex) return props.ToggleIcon;
        if (props?.Icon != null && props.Icon != BaseContent.BadTex) return props.Icon;
        if (verb.UIIcon != null && verb.verbProps.commandIcon != null && verb.UIIcon != BaseContent.BadTex)
            return verb.UIIcon;
        if (ownerThing is ThingWithComps and not Pawn and not Apparel) return ownerThing.def.uiIcon;
        if (verb is Verb_LaunchProjectile proj) return proj.Projectile.uiIcon;
        if (ownerThing is not null) return ownerThing.def.uiIcon;
        return TexCommand.Attack;
    }
}
