using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class CompExtendedBiosculpterPod : CompBiosculpterPod
{
    private static readonly AccessTools.FieldRef<CompBiosculpterPod, int> currentCycleTicksRemainingField = AccessTools.FieldRefAccess<CompBiosculpterPod, int>("currentCycleTicksRemaining");
    private static readonly AccessTools.FieldRef<CompBiosculpterPod, int> currentCyclePowerCutTicksField = AccessTools.FieldRefAccess<CompBiosculpterPod, int>("currentCyclePowerCutTicks");

    public new CompProperties_ExtendedBiosculpterPod Props => (CompProperties_ExtendedBiosculpterPod)props;

    public override void PostDraw()
    {
        if (Props.drawBackground)
        {
            var rotation = parent.Rotation;
            var drawPos = parent.DrawPos;
            drawPos.y -= 0.07317074f;

            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos + Props.backgroundOffset, rotation.AsQuat, Props.backgroundSize), Props.backgroundMaterial, 0);
        }

        if (State == BiosculpterPodState.Occupied && Props.drawPawn)
        {
            var pawnDrawPos = parent.DrawPos + FloatingOffset(currentCycleTicksRemainingField(this) + currentCyclePowerCutTicksField(this)) + Props.PawnOffsetFor(parent.Rotation);
            Occupant.Drawer.renderer.RenderPawnAt(pawnDrawPos, Props.pawnFacingDirectionOverride, true);
        }
    }
}