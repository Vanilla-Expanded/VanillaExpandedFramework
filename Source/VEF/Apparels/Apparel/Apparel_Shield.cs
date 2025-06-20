using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using Verse.AI.Group;

namespace VEF.Apparels
{
    public class Apparel_Shield : Apparel
    {
        private bool CarryWeaponOpenly()
        {
            var wearer = this.Wearer;
            if (wearer.carryTracker != null && wearer.carryTracker.CarriedThing != null)
            {
                return false;
            }
            if (wearer.Drafted)
            {
                return true;
            }
            if (wearer.CurJob != null && wearer.CurJob.def.alwaysShowWeapon)
            {
                return true;
            }
            if (wearer.mindState.duty != null && wearer.mindState.duty.def.alwaysShowWeapon)
            {
                return true;
            }
            Lord lord = wearer.GetLord();
            if (lord != null && lord.LordJob != null && lord.LordJob.AlwaysShowWeapon)
            {
                return true;
            }
            return false;
        }

        private Vector3 GetAimingVector(Vector3 rootLoc, Rot4 rot4)
        {
            var wearer = this.Wearer;
            if (wearer != null)
            {
                // copied from vanilla DrawEquipment method
                Stance_Busy stance_Busy = wearer.stances.curStance as Stance_Busy;
                if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
                {
                    Vector3 a = (!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : stance_Busy.focusTarg.Thing.DrawPos;
                    float num = 0f;
                    if ((a - wearer.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                    {
                        num = (a - wearer.DrawPos).AngleFlat();
                    }
                    Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
                    drawLoc.y += 9f / 245f;
                    return drawLoc;
                }
            }
            if (wearer is null || CarryWeaponOpenly())
            {
                if (rot4 == Rot4.South)
                {
                    Vector3 drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.22f);
                    drawLoc2.y += 9f / 245f;
                    return drawLoc2;
                }
                else if (rot4 == Rot4.North)
                {
                    Vector3 drawLoc3 = rootLoc + new Vector3(0f, 0f, -0.11f);
                    drawLoc3.y += 0f;
                    return drawLoc3;
                }
                else if (rot4 == Rot4.East)
                {
                    Vector3 drawLoc4 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    drawLoc4.y += 9f / 245f;
                    return drawLoc4;
                }
                else if (rot4 == Rot4.West)
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
            var wearer = this.Wearer;
            if (wearer.Dead || !wearer.Spawned || (wearer.CurJob != null && wearer.CurJob.def.neverShowWeapon))
            {
                return;
            }
            var comp = CompShield;
            if (comp.UsableNow)
            {
                DrawShield(comp, wearer.DrawPos, wearer.Rotation);
            }
        }

        public void DrawShield(CompShield comp, Vector3 drawPos, Rot4 rot4)
        {
            var curHoldOffset = comp.Props.offHandHoldOffset.Pick(rot4);
            var finalDrawLoc = this.GetAimingVector(drawPos, rot4) + curHoldOffset.offset + new Vector3(0, (curHoldOffset.behind ? -0.0390625f : 0.0390625f), 0);
            ShieldGraphic.Draw(finalDrawLoc, (curHoldOffset.flip ? rot4.Opposite : rot4), this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Wearer != null)
            {
                var comp = this.GetComp<CompEquippable>();
                if (comp != null)
                {
                    foreach (var verb in comp.AllVerbs)
                    {
                        verb.caster = Wearer;
                        verb.Reset();
                    }
                }
            }
        }
    }   
}