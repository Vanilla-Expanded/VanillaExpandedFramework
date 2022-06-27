using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using Verse.AI.Group;

namespace VFECore
{
    public class Apparel_Shield : Apparel
    {
        private bool CarryWeaponOpenly()
        {
            if (this.Wearer.carryTracker != null && this.Wearer.carryTracker.CarriedThing != null)
            {
                return false;
            }
            if (this.Wearer.Drafted)
            {
                return true;
            }
            if (this.Wearer.CurJob != null && this.Wearer.CurJob.def.alwaysShowWeapon)
            {
                return true;
            }
            if (this.Wearer.mindState.duty != null && this.Wearer.mindState.duty.def.alwaysShowWeapon)
            {
                return true;
            }
            Lord lord = this.Wearer.GetLord();
            if (lord != null && lord.LordJob != null && lord.LordJob.AlwaysShowWeapon)
            {
                return true;
            }
            return false;
        }

        private Vector3 GetAimingVector(Vector3 rootLoc)
        {
            // copied from vanilla DrawEquipment method
            Stance_Busy stance_Busy = this.Wearer.stances.curStance as Stance_Busy;
            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                Vector3 a = (!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : stance_Busy.focusTarg.Thing.DrawPos;
                float num = 0f;
                if ((a - this.Wearer.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    num = (a - this.Wearer.DrawPos).AngleFlat();
                }
                Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
                drawLoc.y += 9f / 245f;
                return drawLoc;
            }
            else if (CarryWeaponOpenly())
            {
                if (this.Wearer.Rotation == Rot4.South)
                {
                    Vector3 drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.22f);
                    drawLoc2.y += 9f / 245f;
                    return drawLoc2;
                }
                else if (this.Wearer.Rotation == Rot4.North)
                {
                    Vector3 drawLoc3 = rootLoc + new Vector3(0f, 0f, -0.11f);
                    drawLoc3.y += 0f;
                    return drawLoc3;
                }
                else if (this.Wearer.Rotation == Rot4.East)
                {
                    Vector3 drawLoc4 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    drawLoc4.y += 9f / 245f;
                    return drawLoc4;
                }
                else if (this.Wearer.Rotation == Rot4.West)
                {
                    Vector3 drawLoc5 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    drawLoc5.y += 9f / 245f;
                    return drawLoc5;
                }
            }
            return default(Vector3);
        }

        private CompShield comp = null;
        public CompShield CompShield
        {
            get
            {
                if (comp == null)
                {
                    comp = this.GetComp<CompShield>();
                }
                return comp;
            } 
        }

        private Graphic shieldGraphic = null;
        public Graphic ShieldGraphic
        {
            get
            {
                if (shieldGraphic == null)
                {
                    shieldGraphic = CompShield.Props.offHandGraphicData.GraphicColoredFor(this);
                }
                return shieldGraphic;
            }
        }

        public override void DrawWornExtras()
        {
            if (this.Wearer.Dead || !this.Wearer.Spawned || (this.Wearer.CurJob != null && this.Wearer.CurJob.def.neverShowWeapon))
            {
                return;
            }
            var comp = CompShield;
            if (comp.UsableNow)
            {
                var curHoldOffset = comp.Props.offHandHoldOffset.Pick(Wearer.Rotation);
                var finalDrawLoc = this.GetAimingVector(this.Wearer.DrawPos) + curHoldOffset.offset + new Vector3(0, (curHoldOffset.behind ? -0.0390625f : 0.0390625f), 0);
                ShieldGraphic.Draw(finalDrawLoc, (curHoldOffset.flip ? Wearer.Rotation.Opposite : Wearer.Rotation), Wearer);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.Wearer != null)
            {
                var comp = this.GetComp<CompEquippable>();
                if (comp != null)
                {
                    foreach (var verb in comp.AllVerbs)
                    {
                        verb.caster = this.Wearer;
                        verb.Reset();
                    }
                }
            }
        }
    }   
}