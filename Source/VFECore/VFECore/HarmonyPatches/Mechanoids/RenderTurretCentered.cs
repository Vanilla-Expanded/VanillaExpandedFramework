using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore;
using VFEMech;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
    public static class RenderTurretCentered
    {
        static Vector3 south = new Vector3(0, 0, -0.33f);
        static Vector3 north = new Vector3(0, -1, -0.22f);
        static Vector3 east = new Vector3(0.2f, 0f, -0.22f);
        static Vector3 west = new Vector3(-0.2f, 0, -0.22f);

        public static bool Prefix(Thing eq, out (Pawn pawn, CompMachine comp) __state)
        {
            __state = default;
            var pawn = (eq.ParentHolder as Pawn_EquipmentTracker)?.pawn;
            if (pawn != null && CompMachine.cachedMachinesPawns?.TryGetValue(pawn, out CompMachine compMachine) != null && compMachine != null)
            {
                if (compMachine.turretAttached != null)
                {
                    __state.comp = compMachine;
                    __state.pawn = pawn;
                    return false;
                }
            }
            return true;
        }

        public static void Postfix(Thing eq, Vector3 drawLoc, float aimAngle, (Pawn pawn, CompMachine comp) __state)
        {
            if(__state != default)
            {
                if(!(__state.pawn.stances.curStance is Stance_Busy && ((Stance_Busy)__state.pawn.stances.curStance).focusTarg.IsValid))
                {
                    aimAngle = __state.comp.turretAngle;
                }

                if (__state.pawn.Rotation == Rot4.South)
                    drawLoc -= south;
                else if (__state.pawn.Rotation == Rot4.North)
                    drawLoc -= north;
                else if (__state.pawn.Rotation == Rot4.East)
                    drawLoc -= east;
                else if (__state.pawn.Rotation == Rot4.West)
                    drawLoc -= west;

                Mesh mesh = null;
                float num = aimAngle - 90f;
                if (aimAngle > 20f && aimAngle < 160f)
                {
                    mesh = MeshPool.plane20;
                    num += eq.def.equippedAngleOffset;
                }
                else if (aimAngle > 200f && aimAngle < 340f)
                {
                    mesh = Startup.plane20Flip;
                    num -= 180f;
                    num -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane20;
                    num += eq.def.equippedAngleOffset;
                }
                num %= 360f;
                Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
                Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingle : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
            }
        }
    }
}
