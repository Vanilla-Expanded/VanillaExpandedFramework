using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Commands
{
    [StaticConstructorOnStartup]
    public class Command_VerbTargetExtended : Command_VerbTarget
    {
        public static readonly Texture2D CooldownTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public ManagedVerb managedVerb;
        public Thing owner;

        public Command_VerbTargetExtended(ManagedVerb mv, Thing ownerThing = null)
        {
            managedVerb = mv;
            verb = mv.Verb;
            owner = ownerThing;
            if (ownerThing != null)
            {
                defaultDesc = PawnVerbGizmoUtility.FirstNonEmptyString(mv.Props?.description, ownerThing.def.LabelCap + ": " +
                                                                                              ownerThing
                                                                                                  .def?.description?
                                                                                                  .Truncate(500, PawnVerbGizmoUtility.__truncateCache)?
                                                                                                  .CapitalizeFirst());
                icon = verb.Icon(mv.Props, ownerThing, false);
            }
            else if (verb.DirectOwner is HediffComp_VerbGiver hediffGiver)
            {
                var hediff = hediffGiver.parent;
                defaultDesc = PawnVerbGizmoUtility.FirstNonEmptyString(mv.Props?.description, hediff.def.LabelCap + ": " +
                                                                                              hediff.def.description
                                                                                                  .Truncate(500, PawnVerbGizmoUtility.__truncateCache)
                                                                                                  .CapitalizeFirst());
                icon = verb.Icon(mv.Props, null, false);
            }

            tutorTag = "VerbTarget";
            defaultLabel = verb.Label(mv.Props);

            if (verb.Caster.Faction != Faction.OfPlayer)
                Disable("CannotOrderNonControlled".Translate());
            else if (verb.CasterIsPawn && verb.verbProps.violent && verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort,
                    verb.CasterPawn));
            else if (verb.CasterIsPawn && verb.CasterPawn.drafter is {Drafted: false} && mv.Props is not {canFireIndependently: true})
                Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort,
                    verb.CasterPawn));
            else if (verb.CasterIsPawn && verb.CasterPawn.InMentalState && mv.Props is not {canFireIndependently: true})
                Disable("CannotOrderNonControlled".Translate());
        }

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUIInt(butRect, parms);

            // if (disabled && managedVerb.AdditionalCooldownPercent > 0)
            //     GUI.DrawTexture(butRect.RightPartPixels(butRect.width * managedVerb.AdditionalCooldownPercent),
            //         CooldownTex);

            return result;
        }
    }
}