using System.Collections.Generic;
using MVCF.Comps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MVCF.VerbComps;

public class VerbComp_Draw : VerbComp
{
    private static readonly Vector3 WestEquipOffset = new(-0.2f, 0.0367346928f);
    private static readonly Vector3 EastEquipOffset = new(0.2f, 0.28f, -0.22f);
    private static readonly Vector3 NorthEquipOffset = new(0f, 0f, -0.11f);
    private static readonly Vector3 SouthEquipOffset = new(0f, 0.0367346928f, -0.22f);
    private static readonly Vector3 EquipPointOffset = new(0f, 0f, 0.4f);

    private float idleRotationOffset;
    private float idleRotationOffsetTarget;
    private int ticksTillTurn = -1;
    public VerbCompProperties_Draw Props => props as VerbCompProperties_Draw;

    public override bool NeedsDrawing => true;

    public override void Initialize(VerbCompProperties props, bool fromLoad)
    {
        base.Initialize(props, fromLoad);
        if (Props.idleRotation)
        {
            ticksTillTurn = Props.idleRotationTicks.RandomInRange;
            idleRotationOffsetTarget = Props.idleRotationAngle.RandomInRange;
        }
    }

    public virtual float Scale(Pawn pawn) => Props.Scale(pawn) * pawn.BodySize;

    public override void DrawOnAt(Pawn p, Vector3 drawPos)
    {
        base.DrawOnAt(p, drawPos);
        if (!ShouldDraw(p)) return;
        drawPos.y += 0.0367346928f;
        var target = PointingTarget(p);
        DrawPointingAt(DrawPos(target, p, drawPos), DrawAngle(target, p, drawPos), Scale(p));
    }

    public override void CompTick()
    {
        base.CompTick();
        if (ticksTillTurn > 0)
        {
            ticksTillTurn--;
            if (ticksTillTurn <= 0) idleRotationOffsetTarget = Mathf.Sign(idleRotationOffsetTarget) * -1 * Props.idleRotationAngle.RandomInRange;
        }

        if (!Mathf.Approximately(idleRotationOffset, idleRotationOffsetTarget)) idleRotationOffset += Mathf.Sign(idleRotationOffsetTarget) * 0.1f;
    }

    public virtual float DrawAngle(LocalTargetInfo target, Pawn p, Vector3 drawPos)
    {
        if (target != null && target.IsValid)
        {
            var a = target.HasThing ? target.Thing.DrawPos : target.Cell.ToVector3Shifted();

            return (a - drawPos).MagnitudeHorizontalSquared() > 0.001f ? (a - drawPos).AngleFlat() : 0f;
        }

        if (Props.drawAsEquipment)
        {
            if (p.Rotation == Rot4.South) return 143f;

            if (p.Rotation == Rot4.North) return 143f;

            if (p.Rotation == Rot4.East) return 143f;

            if (p.Rotation == Rot4.West) return 217f;
        }

        return p.Rotation.AsAngle + idleRotationOffset;
    }

    public virtual Vector3 DrawPos(LocalTargetInfo target, Pawn p, Vector3 drawPos)
    {
        if (Props.drawAsEquipment)
        {
            if (target != null && target.IsValid)
                return drawPos + EquipPointOffset.RotatedBy(DrawAngle(target, p, drawPos));

            if (p.Rotation == Rot4.South) return drawPos + SouthEquipOffset;

            if (p.Rotation == Rot4.North) return drawPos + NorthEquipOffset;

            if (p.Rotation == Rot4.East) return drawPos + EastEquipOffset;

            if (p.Rotation == Rot4.West) return drawPos + WestEquipOffset;
        }

        return Props.DrawPos(p, drawPos, p.Rotation);
    }

    public virtual LocalTargetInfo PointingTarget(Pawn p)
    {
        if (p.stances.curStance is Stance_Busy { neverAimWeapon: false, focusTarg.IsValid: true } busy)
            return busy.focusTarg;
        return null;
    }

    private void DrawPointingAt(Vector3 drawLoc, float aimAngle, float scale)
    {
        var num = aimAngle - 90f;
        Mesh mesh;
        if (aimAngle is > 200f and < 340f)
        {
            mesh = MeshPool.plane10Flip;
            num -= 180f;
        }
        else
            mesh = MeshPool.plane10;

        num %= 360f;

        var matrix4X4 = new Matrix4x4();
        matrix4X4.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), Vector3.one * scale);

        Graphics.DrawMesh(mesh, matrix4X4, Props.Graphic.MatSingle, 0);
    }

    public virtual bool ShouldDraw(Pawn pawn) =>
        pawn.Spawned && !pawn.Dead && (!Props.onlyWhenDrafted || pawn.Drafted) && (!Props.drawAsEquipment || CarryWeaponOpenly(pawn))
     && parent.Verb.IsStillUsableBy(pawn) && (parent.Verb.verbProps is not { linkedBodyPartsGroup: { } parts, ensureLinkedBodyPartsGroupAlwaysUsable: false }
                                           || PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(pawn.health.hediffSet, parts) > 0f);

    private static bool CarryWeaponOpenly(Pawn pawn)
    {
        if (pawn.carryTracker is { CarriedThing: not null }) return false;

        if (pawn.Drafted) return true;

        if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) return true;

        if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon) return true;

        return pawn.GetLord()?.LordJob is { AlwaysShowWeapon: true };
    }
}

public class VerbCompProperties_Draw : VerbCompProperties
{
    public static BodyTypeDef NA = new();
    public DrawPosition defaultPosition;
    public bool drawAsEquipment;
    public float drawScale = 1f;
    public GraphicData graphic;
    public bool humanAsDefault;
    public bool idleRotation;
    public FloatRange idleRotationAngle;
    public IntRange idleRotationTicks;
    public bool onlyWhenDrafted;
    public List<Scaling> scalings;
    public List<DrawPosition> specificPositions;
    private Dictionary<string, Dictionary<BodyTypeDef, DrawPosition>> positions;
    private Dictionary<string, Dictionary<BodyTypeDef, Scaling>> scale;
    public Graphic Graphic { get; private set; }

    public Vector3 DrawPos(Pawn pawn, Vector3 drawPos, Rot4 rot)
    {
        DrawPosition pos = null;
        if (positions.TryGetValue(pawn.def.defName, out var dic) ||
            (humanAsDefault && positions.TryGetValue(ThingDefOf.Human.defName, out dic)))
            if (!(pawn.story?.bodyType != null && dic.TryGetValue(pawn.story.bodyType, out pos)))
                dic.TryGetValue(NA, out pos);

        pos ??= defaultPosition ?? DrawPosition.Zero;
        return drawPos + pos.ForRot(rot);
    }

    public float Scale(Pawn pawn)
    {
        Scaling s = null;
        if (scale.TryGetValue(pawn.def.defName, out var dic) ||
            (humanAsDefault && scale.TryGetValue(ThingDefOf.Human.defName, out dic)))
            if (!(pawn.story?.bodyType != null && dic.TryGetValue(pawn.story.bodyType, out s)))
                dic.TryGetValue(NA, out s);

        return s?.scaling ?? (drawScale == 0 ? 1f : drawScale);
    }

    public override void PostLoadSpecial(VerbProperties verbProps, AdditionalVerbProps additionalProps, Def parentDef)
    {
        base.PostLoadSpecial(verbProps, additionalProps, parentDef);
        if (graphic != null)
        {
            Graphic = graphic.Graphic;
            additionalProps.Icon = (Texture2D)Graphic.ExtractInnerGraphicFor(null).MatNorth.mainTexture;
        }

        if (positions == null)
        {
            positions = new Dictionary<string, Dictionary<BodyTypeDef, DrawPosition>>();
            if (specificPositions != null)
                foreach (var pos in specificPositions)
                    if (!positions.ContainsKey(pos.defName))
                        positions.Add(pos.defName, new Dictionary<BodyTypeDef, DrawPosition> { { pos.BodyType, pos } });
                    else
                        positions[pos.defName].Add(pos.BodyType, pos);
        }

        if (scale == null)
        {
            scale = new Dictionary<string, Dictionary<BodyTypeDef, Scaling>>();
            if (scalings != null)
                foreach (var scaling in scalings)
                    if (scale.ContainsKey(scaling.defName))
                        scale[scaling.defName].Add(scaling.BodyType, scaling);
                    else
                        scale.Add(scaling.defName,
                            new Dictionary<BodyTypeDef, Scaling> { { scaling.BodyType, scaling } });
        }
    }
}

public class DrawPosition
{
    private static readonly Vector2 PLACEHOLDER = Vector2.positiveInfinity;
    public BodyTypeDef BodyType = AdditionalVerbProps.NA;
    public Vector2 Default = PLACEHOLDER;
    public string defName;
    public Vector2 Down = PLACEHOLDER;
    public Vector2 Left = PLACEHOLDER;
    public Vector2 Right = PLACEHOLDER;
    public Vector2 Up = PLACEHOLDER;

    public static DrawPosition Zero =>
        new()
        {
            defName = "",
            Default = Vector2.zero
        };

    public Vector3 ForRot(Rot4 rot)
    {
        var vec = PLACEHOLDER;
        switch (rot.AsInt)
        {
            case 0:
                vec = Up;
                break;
            case 1:
                vec = Right;
                break;
            case 2:
                vec = Down;
                break;
            case 3:
                vec = Left;
                break;
            default:
                vec = Default;
                break;
        }

        if (double.IsPositiveInfinity(vec.x)) vec = Default;
        if (double.IsPositiveInfinity(vec.x)) vec = Vector2.zero;
        return new Vector3(vec.x, 0, vec.y);
    }
}

public class Scaling
{
    public BodyTypeDef BodyType;
    public string defName;
    public float scaling;
}
