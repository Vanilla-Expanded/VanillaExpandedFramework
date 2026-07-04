using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class CustomFuelGaugeData
{
    public float margin = 0.15f;
    public bool horizontalBar = false;
    public bool rotateBarWithBuilding = true;

    public Vector3 centerPositionOffsetNorth = Vector3.zero;
    public Vector3 centerPositionOffsetSouth = Vector3.zero;
    public Vector3 centerPositionOffsetEast = Vector3.zero;
    public Vector3 centerPositionOffsetWest = Vector3.zero;

    public Vector2 sizeNorth = new(1f, 0.2f);
    public Vector2 sizeSouth = new(1f, 0.2f);
    public Vector2 sizeEast = new(1f, 0.2f);
    public Vector2 sizeWest = new(1f, 0.2f);

    public Color fuelBarFilledColor = new(0.6f, 0.56f, 0.13f);
    public Color fuelBarUnfilledColor = new(0.3f, 0.3f, 0.3f);
    public Color? fuelBarFullColor = null;

    private Material fuelBarFilledMat;
    private Material fuelBarUnfilledMat;
    private Material fuelBarFullColorMat;

    public virtual void DrawGauge(Thing parent, float fuelPercentage)
    {
        GenDraw.DrawFillableBar(GetFillableBarRequest(parent, fuelPercentage));
    }

    public virtual GenDraw.FillableBarRequest GetFillableBarRequest(Thing parent, float fuelPercentage)
    {
        return new GenDraw.FillableBarRequest
        {
            center = parent.DrawPos + Vector3.up * 0.1f + OffsetFor(parent.Rotation),
            size = SizeFor(parent.Rotation),
            fillPercent = fuelPercentage,
            filledMat = fuelPercentage >= 1f ? fuelBarFullColorMat : fuelBarFilledMat,
            unfilledMat = fuelBarUnfilledMat,
            margin = margin,
            rotation = RotationFor(parent.Rotation),
        };
    }

    private Vector3 OffsetFor(Rot4 rotation)
    {
        return rotation.AsInt switch
        {
            Rot4.NorthInt => centerPositionOffsetNorth,
            Rot4.SouthInt => centerPositionOffsetSouth,
            Rot4.EastInt => centerPositionOffsetEast,
            Rot4.WestInt => centerPositionOffsetWest,
            _ => Vector3.zero,
        };
    }

    private Vector2 SizeFor(Rot4 rotation)
    {
        return rotation.AsInt switch
        {
            Rot4.NorthInt => sizeNorth,
            Rot4.SouthInt => sizeSouth,
            Rot4.EastInt => sizeEast,
            Rot4.WestInt => sizeWest,
            _ => new Vector2(1.0f, 0.2f),
        };
    }

    private Rot4 RotationFor(Rot4 rotation)
    {
        return (horizontalBar, rotateBarWithBuilding) switch
        {
            (false, false) => Rot4.East,
            (true, false) => Rot4.North,
            (false, true) => rotation.Rotated(RotationDirection.Clockwise),
            (true, true) => rotation,
        };
    }

    public virtual void ResolveReferences()
    {
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            fuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(fuelBarFilledColor);
            fuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(fuelBarUnfilledColor);

            if (fuelBarFullColor != null)
                fuelBarFullColorMat = SolidColorMaterials.SimpleSolidColorMaterial(fuelBarFullColor.Value);
            else
                fuelBarFullColorMat = fuelBarFilledMat;
        });
    }
}