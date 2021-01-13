using System.Collections.Generic;
using MVCF.Comps;
using RimWorld;
using Verse;

namespace MVCF.Utilities
{
    public static class PawnVerbGizmoUtility
    {
        private static readonly Dictionary<string, string> __truncateCache = new Dictionary<string, string>();

        public static IEnumerable<Gizmo> GetGizmosForVerb(this Verb verb, ManagedVerb man = null)
        {
            var gizmo = new Command_VerbTarget {icon = verb.UIIcon, defaultLabel = VerbLabel(verb)};
            AdditionalVerbProps props = null;

            Thing ownerThing = null;
            switch (verb.DirectOwner)
            {
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
                case HediffComp_VerbGiver hediffGiver:
                    if (hediffGiver is HediffComp_ExtendedVerbGiver ext) props = ext.PropsFor(verb);
                    var hediff = hediffGiver.parent;
                    gizmo.defaultDesc = hediff.def.LabelCap + ": " +
                                        hediff.def.description.Truncate(500, __truncateCache).CapitalizeFirst();
                    gizmo.icon = gizmo.icon ?? TexCommand.Attack;
                    break;
            }

            if (ownerThing != null)
            {
                if (ownerThing is ThingWithComps twc && twc.TryGetComp<Comp_VerbGiver>() is Comp_VerbGiver giver)
                    props = giver.PropsFor(verb);
                gizmo.defaultDesc = ownerThing.def.LabelCap + ": " +
                                    ownerThing.def.description.Truncate(500, __truncateCache).CapitalizeFirst();
                gizmo.icon = gizmo.icon ?? ownerThing.def.uiIcon;
            }

            gizmo.tutorTag = "VerbTarget";
            gizmo.verb = verb;

            if (verb.caster.Faction != Faction.OfPlayer)
            {
                gizmo.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.CasterIsPawn)
            {
                if (verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                    gizmo.Disable("IsIncapableOfViolence".Translate((NamedArgument) verb.CasterPawn.LabelShort,
                        (NamedArgument) verb.CasterPawn));
                else if (!verb.CasterPawn.drafter.Drafted)
                    gizmo.Disable("IsNotDrafted".Translate((NamedArgument) verb.CasterPawn.LabelShort,
                        (NamedArgument) verb.CasterPawn));
            }

            yield return gizmo;

            if (props != null && props.canBeToggled && man != null) yield return new Command_ToggleVerbUsage(man);
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
                        var verb = pawn.BestVerbForTarget(target, verbs);
                        verb.OrderForceTarget(target);
                    }, pawn, null, TexCommand.Attack);
                }
            };

            if (pawn.Faction != Faction.OfPlayer)
                gizmo.Disable("CannotOrderNonControlled".Translate());
            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
                gizmo.Disable("IsIncapableOfViolence".Translate((NamedArgument) pawn.LabelShort, (NamedArgument) pawn));
            else if (!pawn.drafter.Drafted)
                gizmo.Disable("IsNotDrafted".Translate((NamedArgument) pawn.LabelShort, (NamedArgument) pawn));

            return gizmo;
        }

        private static string VerbLabel(Verb verb)
        {
            if (!string.IsNullOrEmpty(verb.verbProps.label)) return verb.verbProps.label;
            switch (verb)
            {
                case Verb_LaunchProjectile proj:
                    return proj.Projectile.LabelCap;
                default:
                    return verb.verbProps.label;
            }
        }

        public static string Label(this Verb verb)
        {
            return VerbLabel(verb);
        }
    }
}