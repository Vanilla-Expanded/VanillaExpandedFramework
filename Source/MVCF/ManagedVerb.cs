using System.Collections.Generic;
using MVCF.Comps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MVCF
{
    public class ManagedVerb
    {
        public enum ToggleType
        {
            Separate,
            Integrated,
            None
        }

        private static readonly Vector3 WestEquipOffset = new Vector3(-0.2f, 0.0367346928f, -0.22f);
        private static readonly Vector3 EastEquipOffset = new Vector3(0.2f, 0.0367346928f, -0.22f);
        private static readonly Vector3 NorthEquipOffset = new Vector3(0f, 0f, -0.11f);
        private static readonly Vector3 SouthEquipOffset = new Vector3(0f, 0.0367346928f, -0.22f);

        private static readonly Vector3 EquipPointOffset = new Vector3(0f, 0f, 0.4f);
        protected readonly VerbManager man;

        private int additionalCooldownTicksLeft;

        private bool enabledInt;
        public AdditionalVerbProps Props;
        public VerbSource Source;
        public Verb Verb;

        public ManagedVerb(Verb verb, VerbSource source, AdditionalVerbProps props, VerbManager man)
        {
            Verb = verb;
            Source = source;
            Props = props;
            this.man = man;
            if (Props != null && Props.draw && !Base.Features.Drawing)
            {
                Log.Error(
                    "[MVCF] Found a verb marked to draw while that feature is not enabled. Enabling now. This is not recommend.");
                Base.Features.Drawing = true;
                Base.ApplyPatches();
            }

            if (Props != null && Props.canFireIndependently && !Base.Features.IndependentFire)
            {
                Log.Error(
                    "[MVCF] Found a verb marked to fire independently while that feature is not enabled. Enabling now. This is not recommend.");
                Base.Features.IndependentFire = true;
                Base.ApplyPatches();
            }

            if (Props != null && !Props.separateToggle && !Base.Features.IntegratedToggle)
            {
                Log.Error(
                    "[MVCF] Found a verb marked for an integrated toggle while that feature is not enabled. Enabling now. This is not recommend.");
                Base.Features.IntegratedToggle = true;
                Base.ApplyPatches();
            }
        }

        public float AdditionalCooldownPercent => (float) additionalCooldownTicksLeft / (Props?.additionalCooldownTime.SecondsToTicks() ?? 1);
        public string AdditionalCooldownDesc => additionalCooldownTicksLeft.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor);

        public virtual bool Enabled
        {
            get => enabledInt && additionalCooldownTicksLeft > 0;
            set => enabledInt = value;
        }

        public virtual bool NeedsTicking => Props?.additionalCooldownTime > 0.001f;

        public void Toggle()
        {
            enabledInt = !enabledInt;
            man.RecalcSearchVerb();
        }

        public virtual void DrawOn(Pawn p, Vector3 drawPos)
        {
            if (Props == null) return;
            if (!Props.draw) return;
            if (p.Dead || !p.Spawned) return;
            drawPos.y += 0.0367346928f;
            var target = PointingTarget(p);
            DrawPointingAt(DrawPos(target, p, drawPos),
                DrawAngle(target, p, drawPos), Props.Scale(p) * p.BodySize);
        }

        public virtual void Tick()
        {
            if (additionalCooldownTicksLeft > 0) additionalCooldownTicksLeft--;
        }

        public virtual float DrawAngle(LocalTargetInfo target, Pawn p, Vector3 drawPos)
        {
            if (target != null && target.IsValid)
            {
                var a = target.HasThing ? target.Thing.DrawPos : target.Cell.ToVector3Shifted();

                return (a - drawPos).MagnitudeHorizontalSquared() > 0.001f ? (a - drawPos).AngleFlat() : 0f;
            }

            if (Source == VerbSource.Equipment)
            {
                if (p.Rotation == Rot4.South) return 143f;

                if (p.Rotation == Rot4.North) return 143f;

                if (p.Rotation == Rot4.East) return 143f;

                if (p.Rotation == Rot4.West) return 217f;
            }

            return p.Rotation.AsAngle;
        }

        public virtual IEnumerable<Gizmo> GetGizmos(Thing ownerThing)
        {
            yield return new Command_VerbTargetExtended(this);

            if (GetToggleType() == ToggleType.Separate)
                yield return new Command_ToggleVerbUsage(this);
        }

        public virtual ToggleType GetToggleType()
        {
            if (Props == null) return Verb.CasterIsPawn && Verb.CasterPawn.RaceProps.Animal ? ToggleType.Separate : ToggleType.None;

            if (!Props.canBeToggled) return ToggleType.None;
            if (Props.separateToggle) return ToggleType.Separate;
            if (Base.Features.IntegratedToggle) return ToggleType.Integrated;

            Log.ErrorOnce(
                "[MVCF] " + (Verb.EquipmentSource.LabelShortCap ?? "Hediff verb of " + Verb.caster) +
                " wants an integrated toggle but that feature is not enabled. Using seperate toggle.",
                Verb.GetHashCode());
            return ToggleType.Separate;
        }

        public virtual Vector3 DrawPos(LocalTargetInfo target, Pawn p, Vector3 drawPos)
        {
            if (Source == VerbSource.Equipment)
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
            if (p.stances.curStance is Stance_Busy busy && !busy.neverAimWeapon && busy.focusTarg.IsValid)
                return busy.focusTarg;
            return null;
        }

        private void DrawPointingAt(Vector3 drawLoc, float aimAngle, float scale)
        {
            var num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 200f && aimAngle < 340f)
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

        private static bool CarryWeaponOpenly(Pawn pawn)
        {
            if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null) return false;

            if (pawn.Drafted) return true;

            if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) return true;

            if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon) return true;

            var lord = pawn.GetLord();
            return lord?.LordJob != null && lord.LordJob.AlwaysShowWeapon;
        }
    }
}