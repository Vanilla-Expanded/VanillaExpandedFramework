using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class CompExtendedBiosculpterPod : CompBiosculpterPod
{
    private static readonly AccessTools.FieldRef<CompBiosculpterPod, float> currentCycleTicksRemainingField = AccessTools.FieldRefAccess<CompBiosculpterPod, float>("currentCycleTicksRemaining");
    private static readonly AccessTools.FieldRef<CompBiosculpterPod, int> currentCyclePowerCutTicksField = AccessTools.FieldRefAccess<CompBiosculpterPod, int>("currentCyclePowerCutTicks");

    public new CompProperties_ExtendedBiosculpterPod Props => (CompProperties_ExtendedBiosculpterPod)props;

    public override void PostDraw()
    {
        if (Props.drawBackground)
        {
            var drawPos = parent.DrawPos;
            drawPos.y -= 0.07317074f;

            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos + Props.BackgroundOffsetFor(parent.Rotation), parent.Rotation.AsQuat, Props.backgroundSize), Props.backgroundMaterial, 0);
        }

        if (State == BiosculpterPodState.Occupied && Props.drawPawn)
        {
            var pawnDrawPos = parent.DrawPos + FloatingOffset(currentCycleTicksRemainingField(this) + currentCyclePowerCutTicksField(this)) + Props.PawnOffsetFor(parent.Rotation);
            Occupant.Drawer.renderer.RenderPawnAt(pawnDrawPos, Props.pawnFacingDirectionOverride, true);
        }
    }
}