using System;
using System.Xml;
using UnityEngine;
using VEF.Utils;
using Verse;
using Color = UnityEngine.Color;

namespace VEF.Buildings;

public class CustomFillableBarGaugeData
{
    protected const string CenterPositionOffsetNodeName = "centerPositionOffset";
    protected const string SizeNodeName = "size";

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

    [Unsaved] protected Material barFilledMat;
    [Unsaved] protected Material barUnfilledMat;
    [Unsaved] protected Material barFullColorMat;

    public virtual void DrawGauge(Thing parent, float fuelPercentage)
    {
        GenDraw.DrawFillableBar(GetFillableBarRequest(parent, fuelPercentage));
    }

    public virtual GenDraw.FillableBarRequest GetFillableBarRequest(Thing parent, float fuelPercentage)
    {
        return new GenDraw.FillableBarRequest
        {
            center = parent.DrawPos + Vector3.up * 0.1f + OffsetFor(parent),
            size = SizeFor(parent),
            fillPercent = fuelPercentage,
            filledMat = fuelPercentage >= 1f ? BarFullColorMatFor(parent) : BarFilledMatFor(parent),
            unfilledMat = BarUnfilledMatFor(parent),
            margin = MarginFor(parent),
            rotation = RotationFor(parent),
        };
    }

    protected virtual Vector3 OffsetFor(Thing parent)
    {
        return parent.Rotation.AsInt switch
        {
            Rot4.NorthInt => centerPositionOffsetNorth,
            Rot4.SouthInt => centerPositionOffsetSouth,
            Rot4.EastInt => centerPositionOffsetEast,
            Rot4.WestInt => centerPositionOffsetWest,
            _ => Vector3.zero,
        };
    }

    protected virtual Vector2 SizeFor(Thing parent)
    {
        return parent.Rotation.AsInt switch
        {
            Rot4.NorthInt => sizeNorth,
            Rot4.SouthInt => sizeSouth,
            Rot4.EastInt => sizeEast,
            Rot4.WestInt => sizeWest,
            _ => new Vector2(1.0f, 0.2f),
        };
    }

    protected virtual Rot4 RotationFor(Thing parent)
    {
        return (horizontalBar, rotateBarWithBuilding) switch
        {
            (false, false) => Rot4.East,
            (true, false) => Rot4.North,
            (false, true) => parent.Rotation.Rotated(RotationDirection.Clockwise),
            (true, true) => parent.Rotation,
        };
    }

    protected virtual float MarginFor(Thing parent) => margin;

    protected virtual Material BarFilledMatFor(Thing parent) => barFilledMat;

    protected virtual Material BarUnfilledMatFor(Thing parent) => barUnfilledMat;

    protected virtual Material BarFullColorMatFor(Thing parent) => barFullColorMat;

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

    public virtual Dialog_GaugeAdjustment GetConfigurationDialog(Thing parent) => new Dialog_GaugeAdjustment(parent, this);

    protected virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        TryParse(xmlRoot, nameof(margin), ref margin);
        TryParse(xmlRoot, nameof(horizontalBar), ref horizontalBar);
        TryParse(xmlRoot, nameof(rotateBarWithBuilding), ref rotateBarWithBuilding);

        // Default value for all offsets
        if (TryParse(xmlRoot, CenterPositionOffsetNodeName, ref centerPositionOffsetNorth))
        {
            centerPositionOffsetSouth = centerPositionOffsetNorth;
            centerPositionOffsetEast = centerPositionOffsetNorth;
            centerPositionOffsetWest = centerPositionOffsetNorth;
        }

        // Try parsing each offset as well, in case they're non-default
        TryParse(xmlRoot, nameof(centerPositionOffsetNorth), ref centerPositionOffsetNorth);
        TryParse(xmlRoot, nameof(centerPositionOffsetSouth), ref centerPositionOffsetSouth);
        TryParse(xmlRoot, nameof(centerPositionOffsetEast), ref centerPositionOffsetEast);
        TryParse(xmlRoot, nameof(centerPositionOffsetWest), ref centerPositionOffsetWest);

        // Default value for all sizes
        if (TryParse(xmlRoot, SizeNodeName, ref sizeNorth))
        {
            sizeSouth = sizeNorth;
            sizeEast = sizeNorth;
            sizeWest = sizeNorth;
        }

        // Try parsing each size as well, in case they're non-default
        TryParse(xmlRoot, nameof(sizeNorth), ref sizeNorth);
        TryParse(xmlRoot, nameof(sizeSouth), ref sizeSouth);
        TryParse(xmlRoot, nameof(sizeEast), ref sizeEast);
        TryParse(xmlRoot, nameof(sizeWest), ref sizeWest);

        TryParse(xmlRoot, nameof(barFilledColor), ref barFilledColor);
        TryParse(xmlRoot, nameof(barUnfilledColor), ref barUnfilledColor);
        TryParse(xmlRoot, nameof(barFullColor), ref barFullColor);
    }

    protected static bool TryParse<T>(XmlNode xmlRoot, string name, ref T value)
    {
        var newValue = xmlRoot[name]?.InnerText;
        if (newValue == null)
            return false;

        value = ParseHelper.FromString<T>(newValue);
        return true;
    }

    public class Dialog_GaugeAdjustment : Window
    {
        public Action onChanged;
        public Action postClosed;

        protected Thing parent;
        protected CustomFillableBarGaugeData data;

        protected string marginBuffer;
        protected string offsetBufferX;
        protected string offsetBufferZ;
        protected string sizeBufferX;
        protected string sizeBufferY;

        protected bool anythingChanged = false;

        public override Vector2 InitialSize => new(320f, 260f);

        public Dialog_GaugeAdjustment(Thing parent, CustomFillableBarGaugeData data)
        {
            this.parent = parent;
            this.data = data;
            resizeable = true;
            doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            using var _ = new TextBlock(GameFont.Small, TextAnchor.UpperLeft, false, Color.white);
            anythingChanged = false;

            DrawCheckbox(ref inRect, "Rotate", ref data.rotateBarWithBuilding);
            DrawCheckbox(ref inRect, "Horizontal", ref data.horizontalBar);

            DrawCountAdjuster(ref inRect, "Margin", ref data.margin, ref marginBuffer, 0f, 1f);

            // DrawRotationChanger(ref inRect, currentRotation.ToStringWord(), ref currentRotation);

            ref var offset = ref OffsetFor(parent.Rotation);
            DrawCountAdjuster(ref inRect, $"{parent.Rotation.ToStringWord()} offset.x", ref offset.x, ref offsetBufferX, -1000f, 1000f);
            DrawCountAdjuster(ref inRect, $"{parent.Rotation.ToStringWord()} offset.z", ref offset.z, ref offsetBufferZ, -1000f, 1000f);

            ref var size = ref SizeFor(parent.Rotation);
            DrawCountAdjuster(ref inRect, $"{parent.Rotation.ToStringWord()} size.x", ref size.x, ref sizeBufferX, 0.1f / 1000f, 1000f);
            DrawCountAdjuster(ref inRect, $"{parent.Rotation.ToStringWord()} size.y", ref size.y, ref sizeBufferY, 0.1f / 1000f, 1000f);

            if (anythingChanged)
            {
                anythingChanged = false;
                onChanged?.Invoke();
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            postClosed?.Invoke();
        }

        protected ref Vector3 OffsetFor(Rot4 rotation)
        {
            switch (rotation.AsInt)
            {
                case Rot4.SouthInt:
                    return ref data.centerPositionOffsetSouth;
                case Rot4.EastInt:
                    return ref data.centerPositionOffsetEast;
                case Rot4.WestInt:
                    return ref data.centerPositionOffsetWest;
                default:
                    return ref data.centerPositionOffsetNorth;
            }
        }

        protected ref Vector2 SizeFor(Rot4 rotation)
        {
            switch (rotation.AsInt)
            {
                case Rot4.SouthInt:
                    return ref data.sizeSouth;
                case Rot4.EastInt:
                    return ref data.sizeEast;
                case Rot4.WestInt:
                    return ref data.sizeWest;
                default:
                    return ref data.sizeNorth;
            }
        }

        protected void DrawRotationChanger(ref Rect inRect, string label, ref Rot4 rotation)
        {
            if (!rotation.IsValid)
                rotation = Rot4.North;

            inRect.SplitHorizontally(30f, out var top, out inRect);
            top.ContractedBy(0f, 5f).SplitVertically(top.width * 0.35f, out var left, out var right);
            Widgets.Label(left, label);

            right.SplitVertically(30f, out var leftButton, out right);
            right.SplitVertically(right.width - 30f, out right, out var rightButton);

            if (Widgets.ButtonText(leftButton, "<"))
            {
                rotation.Rotate(RotationDirection.Counterclockwise);
                ClearBuffers();
            }

            if (Widgets.ButtonText(rightButton, ">"))
            {
                rotation.Rotate(RotationDirection.Clockwise);
                ClearBuffers();
            }

            using (new TextBlock(TextAnchor.MiddleCenter))
                Widgets.Label(right, label);

            void ClearBuffers()
            {
                offsetBufferX = null;
                offsetBufferZ = null;
                sizeBufferX = null;
                sizeBufferY = null;
            }
        }

        protected void DrawCountAdjuster(ref Rect inRect, string label, ref float value, ref string buffer, float min, float max, float adjustment = 0.1f, bool inversedAdjustmentMultiplier = true)
        {
            inRect.SplitHorizontally(30f, out var top, out inRect);
            top.ContractedBy(0, 5f).SplitVertically(top.width * 0.35f, out var left, out var right);
            Widgets.Label(left, label);

            var oldValue = value;
            UIUtility.DrawCountAdjuster(ref value, right, ref buffer, min, max, adjustment: adjustment, inversedAdjustmentMultiplier: inversedAdjustmentMultiplier, marginX: 0f, drawSetToMinMax: false);
            if (oldValue != value)
                anythingChanged = true;
        }

        protected void DrawCheckbox(ref Rect inRect, string label, ref bool value)
        {
            inRect.SplitHorizontally(30f, out var top, out inRect);

            var oldValue = value;
            Widgets.CheckboxLabeled(top.ContractedBy(0f, 5f), label, ref value);
            if (oldValue != value)
                anythingChanged = true;
        }
    }
}