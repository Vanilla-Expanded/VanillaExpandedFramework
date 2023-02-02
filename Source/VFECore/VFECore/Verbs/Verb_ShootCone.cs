using System;
using UnityEngine;
using Verse;

namespace VFEPirates
{
    public class VerbProps_ShootCone : VerbProperties
    {
        public int coneAngle;
    }
    public class Verb_ShootCone : Verb_Shoot
    {
        public VerbProps_ShootCone VerbProps => this.verbProps as VerbProps_ShootCone;
        private Material lineMat = null;
        private Material LineMat
        {
            get
            {
                if (lineMat == null)
                {
                    lineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.white);
                }
                return lineMat;
            }
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (VerbProps.range <= GenRadial.MaxRadialPatternRadius)
            {
                DrawConeRounded(VerbProps.coneAngle);
            }
            else
            {
                DrawLines();
            }

            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                DrawHighlightFieldRadiusAroundTarget(target);
            }
        }


        private void DrawLines()
        {
            var startPos = this.Caster.Position.ToVector3Shifted();
            var quatLeft = Quaternion.Euler(0f, -VerbProps.coneAngle / 2f, 0f);
            var quatRight = Quaternion.Euler(0f, VerbProps.coneAngle / 2f, 0f);
            var targetLeft = startPos + (this.Caster.Rotation.AsQuat * quatLeft * new Vector3(0f, 0f, verbProps.range));
            var targetRight = startPos + (this.Caster.Rotation.AsQuat * quatRight * new Vector3(0f, 0f, verbProps.range));
            GenDraw.DrawLineBetween(startPos, targetLeft, AltitudeLayer.MetaOverlays.AltitudeFor(), LineMat, 0.5f);
            GenDraw.DrawLineBetween(startPos, targetRight, AltitudeLayer.MetaOverlays.AltitudeFor(), LineMat, 0.5f);

        }
        private void DrawConeRounded(float angle)
        {
            IntVec3 pos = this.Caster.Position;
            Rot4 rotation = this.caster.Rotation;

            Func<IntVec3, bool> predicate = ((IntVec3 c) => InCone(c, pos, rotation, angle));
            GenDraw.DrawRadiusRing(pos, this.verbProps.range, Color.white, predicate);
        }
        public override bool CanHitTarget(LocalTargetInfo targ)
        {
            return base.CanHitTarget(targ) && InCone(targ.Cell, this.caster.Position, this.caster.Rotation, VerbProps.coneAngle);
        }

        public bool InCone(IntVec3 evaluatedCell, IntVec3 from, Rot4 rotation, float degrees)
        {
            Vector3 dif = evaluatedCell.ToVector3() - from.ToVector3();
            Vector3 lookRotation = Quaternion.LookRotation(dif, Vector3.up).eulerAngles;
            if (GenGeo.AngleDifferenceBetween(lookRotation.y, rotation.AsAngle) <= degrees / 2f)
            {
                return true;
            }
            return false;
        }
    }
}