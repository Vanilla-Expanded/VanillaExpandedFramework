using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VEF.Buildings;

public class Gizmo_SetSecondaryFuelLevel : Gizmo_Slider
{
    private CompRefuelable_DualFuel refuelable;

    protected override float Target
    {
        get
        {
            return refuelable.SecondaryTargetFuelLevel / refuelable.Props.secondaryFuelCapacity;
        }
        set
        {
            refuelable.SecondaryTargetFuelLevel = value * refuelable.Props.secondaryFuelCapacity;
        }
    }

    protected override float ValuePercent => refuelable.SecondaryFuelPercentOfMax;

    protected override string Title => refuelable.Props.SecondaryFuelGizmoLabel;

    protected override bool IsDraggable => refuelable.Props.targetSecondaryFuelLevelConfigurable;

    protected override string BarLabel => refuelable.SecondaryFuel.ToStringDecimalIfSmall() + " / " +
                                         refuelable.Props.secondaryFuelCapacity.ToStringDecimalIfSmall();

    protected override bool DraggingBar { get; set; }

    public Gizmo_SetSecondaryFuelLevel(CompRefuelable_DualFuel refuelable)
    {
        this.refuelable = refuelable;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        if (!refuelable.Props.showAllowAutoRefuelSecondaryToggle)
        {
            return base.GizmoOnGUI(topLeft, maxWidth, parms);
        }

        return base.GizmoOnGUI(topLeft, maxWidth, parms);
    }

    protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
    {
        if (refuelable.Props.showAllowAutoRefuelSecondaryToggle)
        {
            headerRect.xMax -= 24f;
            Rect rect = new Rect(headerRect.xMax, headerRect.y, 24f, 24f);
            GUI.DrawTexture(rect, refuelable.Props.SecondaryFuelIcon);
            GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f),
                refuelable.allowAutoRefuelSecondary ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
            if (Widgets.ButtonInvisible(rect))
            {
                ToggleAutoRefuel();
            }
        }
        base.DrawHeader(headerRect, ref mouseOverElement);
    }

    private void ToggleAutoRefuel()
    {
        refuelable.allowAutoRefuelSecondary = !refuelable.allowAutoRefuelSecondary;
        if (refuelable.allowAutoRefuelSecondary)
        {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        }
        else
        {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
        }
    }

    protected override string GetTooltip()
    {
        return "";
    }
}