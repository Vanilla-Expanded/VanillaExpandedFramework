using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class CustomFillableBarGaugeData
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

    public Color barFilledColor = new(0.6f, 0.56f, 0.13f);
    public Color barUnfilledColor = new(0.3f, 0.3f, 0.3f);
    public Color? barFullColor = null;

    private Material barFilledMat;
    private Material barUnfilledMat;
    private Material barFullColorMat;

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
            filledMat = fuelPercentage >= 1f ? barFullColorMat : barFilledMat,
            unfilledMat = barUnfilledMat,
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
            barFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(barFilledColor);
            barUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(barUnfilledColor);

            if (barFullColor != null)
                barFullColorMat = SolidColorMaterials.SimpleSolidColorMaterial(barFullColor.Value);
            else
                barFullColorMat = barFilledMat;
        });
    }
}