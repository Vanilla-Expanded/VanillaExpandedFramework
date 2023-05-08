using System.Collections.Generic;
using System.Linq;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Commands;

[StaticConstructorOnStartup]
public class Command_VerbTargetExtended : Command_VerbTarget
{
    private readonly List<CommandPart> parts;
    private readonly string topRightLabel;
    public List<ManagedVerb> groupedVerbs;

    public ManagedVerb managedVerb;
    public Thing owner;

    public Command_VerbTargetExtended(ManagedVerb mv, Thing ownerThing = null)
    {
        managedVerb = mv;
        verb = mv.Verb;
        owner = ownerThing;
        parts = mv.GetCommandParts(this).ToList();
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

        for (var i = 0; i < parts.Count; i++) parts[i].ModifyInfo(ref defaultLabel, ref topRightLabel, ref defaultDesc, ref icon);

        if (verb.Caster.Faction != Faction.OfPlayer)
            Disable("CannotOrderNonControlled".Translate());
        else if (verb.CasterIsPawn && verb.verbProps.violent && verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
            Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort,
                verb.CasterPawn));
        else if (verb.CasterIsPawn && verb.CasterPawn.drafter is { Drafted: false } && mv.Props is not { canFireIndependently: true })
            Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort,
                verb.CasterPawn));
        else if (verb.CasterIsPawn && verb.CasterPawn.InMentalState && mv.Props is not { canFireIndependently: true })
            Disable("CannotOrderNonControlled".Translate());
        for (var i = 0; i < parts.Count; i++) parts[i].PostInit();
    }

    public IEnumerable<CommandPart> Parts => parts;

    public override string TopRightLabel => topRightLabel;
    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => parts.SelectMany(part => part.GetRightClickOptions());

    public override bool GroupsWith(Gizmo other)
    {
        if (other is Command_VerbTargetExtended { managedVerb: { } mv })
        {
            // Do not merge verbs on the same pawn, so that you can have multiple hediffs providing the same verbs that you can target separately
            if (mv is { Manager.Pawn: { } pawn1 } && managedVerb is { Manager.Pawn: { } pawn2 } && pawn1 == pawn2) return false;
            if (mv.GetToggleType() != managedVerb.GetToggleType()) return false;
        }

        return base.GroupsWith(other);
    }

    public override void MergeWith(Gizmo other)
    {
        if (other is Command_VerbTargetExtended command)
        {
            groupedVerbs ??= new List<ManagedVerb>();
            groupedVerbs.Add(command.managedVerb);
            if (command.groupedVerbs is not null) groupedVerbs.AddRange(command.groupedVerbs);
        }

        base.MergeWith(other);
    }

    protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
    {
        for (var i = 0; i < parts.Count; i++) parts[i].PreGizmoOnGUI(butRect, parms);
        var result = base.GizmoOnGUIInt(butRect, parms);
        for (var i = 0; i < parts.Count; i++) parts[i].PostGizmoOnGUI(butRect, parms, ref result);
        return result;
    }
}