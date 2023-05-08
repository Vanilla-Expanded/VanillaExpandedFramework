using MVCF.Commands;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.Utilities;

public static class DrawUtility
{
    public static bool DrawToggle(Command command, Rect butRect, GizmoRenderParms parms)
    {
        var rect = butRect.LeftPart(0.15f).TopPart(0.15f);
        rect.x += butRect.width * 0.15f;
        rect.y += butRect.height * 0.15f;
        if (command is Command_VerbTargetExtended { Parts: var parts })
            foreach (var part in parts)
            {
                var count = 0;
                if (part.DrawExtraGUIButtons(rect, ref count)) return true;

                for (var i = 0; i < count; i++)
                {
                    rect.x += butRect.width * 0.15f;
                    if (rect.x > butRect.width * 0.5f)
                    {
                        rect.y += butRect.height * 0.15f;
                        rect.x = 0f;
                    }
                }
            }

        if (parms.shrunk) return false;
        if (command is not Command_VerbTarget gizmo) return false;
        var verb = gizmo.verb;
        if (!verb.CasterIsPawn) return false;
        var pawn = verb.CasterPawn;
        if (pawn.Faction != Faction.OfPlayer) return false;
        var man = verb.Managed(false);
        if (man == null) return false;
        if (man.GetToggleType() != ManagedVerb.ToggleType.Integrated) return false;
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
