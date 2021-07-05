using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CompOversizedWeapon
{
    [StaticConstructorOnStartup]
    internal static class HarmonyCompOversizedWeapon
    {
        static HarmonyCompOversizedWeapon()
        {
            var harmony = new Harmony("jecstools.jecrell.comps.oversized");
            var type = typeof(HarmonyCompOversizedWeapon);

            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming)),
                prefix: new HarmonyMethod(type, nameof(DrawEquipmentAimingPreFix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.DefaultGraphic)),
                postfix: new HarmonyMethod(type, nameof(get_DefaultGraphic_PostFix)));
        }

        /// <summary>
        ///     Adds another "layer" to the equipment aiming if they have a
        ///     weapon with a CompActivatableEffect.
        /// </summary>
        public static bool DrawEquipmentAimingPreFix(Pawn ___pawn, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            var compOversizedWeapon = eq.TryGetCompOversizedWeapon();
            if (compOversizedWeapon == null)
                return true;
            //If the deflector is animating now, deflector handles drawing (and already has the drawSize fix).
            if (compOversizedWeapon.CompDeflectorIsAnimatingNow)
                return false;

            var props = compOversizedWeapon.Props;
            var isFighting = ___pawn.IsFighting();
            var rotation = ___pawn.Rotation;

            var flip = false;
            var num = aimAngle - 90f;

            if (aimAngle > 20f && aimAngle < 160f)
            {
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                flip = true;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                num += AngleOffsetAtPeace(eq, isFighting, props);
            }

            if (!isFighting && props != null)
            {
                if (props.verticalFlipNorth && rotation == Rot4.North)
                {
                    num += 180f;
                }
                num += NonCombatAngleAdjustment(rotation, props);
            }
            num %= 360f;

            var matSingle = eq.Graphic is Graphic_StackCount graphic_StackCount
                ? graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle
                : eq.Graphic.MatSingle;
            var s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            var curOffset = props != null ? OffsetFromRotation(rotation, props) : Vector3.zero;
            var matrix = Matrix4x4.TRS(drawLoc + curOffset, Quaternion.AngleAxis(num, Vector3.up), s);

            Graphics.DrawMesh(flip ? MeshPool.plane10Flip : MeshPool.plane10, matrix, matSingle, 0);
            if (props != null && props.isDualWeapon)
            {
                curOffset = new Vector3(-1f * curOffset.x, curOffset.y, curOffset.z);
                Mesh curPool;
                if (rotation == Rot4.North || rotation == Rot4.South)
                {
                    num += 135f;
                    num %= 360f;
                    curPool = flip ? MeshPool.plane10 : MeshPool.plane10Flip;
                }
                else
                {
                    curOffset = new Vector3(curOffset.x, curOffset.y - 0.1f, curOffset.z + 0.15f);
                    curPool = flip ? MeshPool.plane10Flip : MeshPool.plane10;
                }
                matrix.SetTRS(drawLoc + curOffset, Quaternion.AngleAxis(num, Vector3.up), s);
                Graphics.DrawMesh(curPool, matrix, matSingle, 0);
            }
            return false;
        }

        private static float AngleOffsetAtPeace(Thing eq, bool isFighting, CompProperties_OversizedWeapon props)
        {
            var offsetAtPeace = eq.def.equippedAngleOffset;
            if (!isFighting && props != null && props.verticalFlipOutsideCombat)
            {
                offsetAtPeace += 180f;
            }
            return offsetAtPeace;
        }

        private static float NonCombatAngleAdjustment(Rot4 rotation, CompProperties_OversizedWeapon props)
        {
            if (rotation == Rot4.North)
                return props.angleAdjustmentNorth;
            else if (rotation == Rot4.East)
                return props.angleAdjustmentEast;
            else if (rotation == Rot4.West)
                return props.angleAdjustmentWest;
            else
                return props.angleAdjustmentSouth;
        }

        private static Vector3 OffsetFromRotation(Rot4 rotation, CompProperties_OversizedWeapon props)
        {
            if (rotation == Rot4.North)
                return props.northOffset;
            else if (rotation == Rot4.East)
                return props.eastOffset;
            else if (rotation == Rot4.West)
                return props.westOffset;
            else
                return props.southOffset;
        }

        public static void get_DefaultGraphic_PostFix(Thing __instance, Graphic ___graphicInt, ref Graphic __result)
        {
            if (___graphicInt == null)
                return;
            if (__instance.ParentHolder is Pawn)
                return;

            var compOversizedWeapon = __instance.TryGetCompOversizedWeapon();
            if (compOversizedWeapon != null)
            {
                var groundGraphic = compOversizedWeapon.Props?.groundGraphic;
                if (groundGraphic != null && compOversizedWeapon.IsOnGround &&
                    groundGraphic.GraphicColoredFor(__instance) is Graphic newResult)
                {
                    // See comment below on drawSize.
                    newResult.drawSize = groundGraphic.drawSize;
                    __result = newResult;
                }
                else
                {
                    // Note: This is originally a workaround for a bug where the new Graphic returned by
                    // Graphic_RandomRotated.GetColoredVersion does not inherit the original drawSize,
                    // instead always using a drawSize of (1,1). This bug has been fixed in RW 1.2.2723+,
                    // but since older supported RW versions are still afflicted, for now, always apply the workaround.
                    __result.drawSize = __instance.def.graphicData.drawSize;
                }
            }
        }
    }
}
