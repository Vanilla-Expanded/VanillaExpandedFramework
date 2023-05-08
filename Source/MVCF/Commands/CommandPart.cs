using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MVCF.Commands;

public abstract class CommandPart
{
    public Command_VerbTargetExtended parent;

    public virtual void PreGizmoOnGUI(Rect butRect, GizmoRenderParms parms) { }

    public virtual void PostGizmoOnGUI(Rect butRect, GizmoRenderParms parms, ref GizmoResult result) { }

    public virtual IEnumerable<FloatMenuOption> GetRightClickOptions()
    {
        yield break;
    }

    public virtual void ModifyInfo(ref string label, ref string topRightLabel, ref string desc, ref Texture icon) { }

    public virtual void PostInit() { }

    public virtual bool DrawExtraGUIButtons(Rect rect, ref int buttonCount) => false;
}
