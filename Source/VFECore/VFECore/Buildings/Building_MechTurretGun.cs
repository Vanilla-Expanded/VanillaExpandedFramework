using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
    public class Building_MechTurretGun : Building_TurretGun
    {
        public override void Draw()
        {
            base.Draw();
            if (this.TargetCurrentlyAimingAt.IsValid && (!this.TargetCurrentlyAimingAt.HasThing || this.TargetCurrentlyAimingAt.Thing.Spawned))
            {
                Vector3 b = (!this.TargetCurrentlyAimingAt.HasThing) ? this.TargetCurrentlyAimingAt.Cell.ToVector3Shifted() : this.TargetCurrentlyAimingAt.Thing.TrueCenter();
                Vector3 a = this.TrueCenter();
                b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                a.y = b.y;
                GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
            }
        }
    }
}
