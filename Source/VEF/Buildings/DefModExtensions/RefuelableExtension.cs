using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class RefuelableExtension : DefModExtension
{
    public bool ejectingFuelRespectsFuelMultiplier = false;
	// Data for custom fuel gauge drawing.
	// Could be done as a subtype of CompRefuelable, but using a patch means that it'll
	// work in any type of refuelable as long as it doesn't replace PostDraw method
    // Remember to disable vanilla fuel gauge drawing, as this one doesn't check for it!
    public CustomFuelGaugeData customFuelGauge = null;

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        if (ejectingFuelRespectsFuelMultiplier)
            VanillaExpandedFramework_CompRefuelable_EjectFuelPatches.patchActive = true;
        if (customFuelGauge != null)
        {
            VanillaExpandedFramework_CompRefuelable_PostDraw_Patch.patchActive = true;
            customFuelGauge.ResolveReferences();
        }
    }

    [StaticConstructorOnStartup]
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

        private Material fuelBarFilledMat;
        private Material fuelBarUnfilledMat;

        public virtual void DrawGauge(CompRefuelable comp)
        {
            var bar = new GenDraw.FillableBarRequest
            {
                center = comp.parent.DrawPos + Vector3.up * 0.1f + OffsetFor(comp.parent.Rotation),
                size = SizeFor(comp.parent.Rotation),
                fillPercent = comp.FuelPercentOfMax,
                filledMat = fuelBarFilledMat,
                unfilledMat = fuelBarUnfilledMat,
                margin = margin,
                rotation = RotationFor(comp.parent.Rotation),
            };

            GenDraw.DrawFillableBar(bar);
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

        internal void ResolveReferences()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                fuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(fuelBarFilledColor);
                fuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(fuelBarUnfilledColor);
            });
        }
    }
}