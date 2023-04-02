using MVCF.Commands;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.Utilities;

public static class DrawUtility
{
    public static bool DrawToggle(Command command, Rect butRect, GizmoRenderParms parms)
    {
        if (parms.shrunk) return false;
        if (command is not Command_VerbTarget gizmo) return false;
        var verb = gizmo.verb;
        if (!verb.CasterIsPawn) return false;
        var pawn = verb.CasterPawn;
        if (pawn.Faction != Faction.OfPlayer) return false;
        var man = verb.Managed(false);
        if (man == null) return false;
        if (man.GetToggleType() != ManagedVerb.ToggleType.Integrated) return false;
        var rect = command.TopRightLabel.NullOrEmpty()
            ? butRect.RightPart(0.35f).TopPart(0.35f)
            : butRect
               .LeftPart(0.35f)
               .TopPart(0.35f);
        if (Mouse.IsOver(rect)) TooltipHandler.TipRegion(rect, "MVCF.ToggleAuto".Translate());

        if (Widgets.ButtonImage(rect, man.GetToggleStatus() ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
        {
            Event.current.Use();
            if (command is Command_VerbTargetExtended { groupedVerbs: { Count: > 0 } verbs })
                foreach (var mv in verbs)
                    mv.Toggle();
            man.Toggle();
            return true;
        }

        return false;
    }
}
