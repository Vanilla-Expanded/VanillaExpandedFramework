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
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    public static class RenderTurretCentered
    {
        static bool replaced = false;
        static Pawn pawn;
        static CompMachine machine;
        static Vector3 south = new Vector3(0, 0, -0.33f);
        static Vector3 north = new Vector3(0, -1, -0.22f);
        static Vector3 east = new Vector3(0.2f, 0f, -0.22f);
        static Vector3 west = new Vector3(-0.2f, 0, -0.22f);

        public static bool Prefix(PawnRenderer __instance)
        {
            if (CompMachine.cachedMachines.TryGetValue(__instance, out CompMachine compMachine))
            {
                if (compMachine != null)
                {
                    machine = compMachine;
                    pawn = CompMachine.cachedPawns[compMachine];
                    if (compMachine.turretAttached != null)
                        replaced = true;
                    else
                        replaced = false;
                    return !replaced;
                }
            }
            replaced = false;
            return true;
        }

        public static void Postfix(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if(replaced)
            {
                if(!(pawn.stances.curStance is Stance_Busy && ((Stance_Busy)pawn.stances.curStance).focusTarg.IsValid))
                {
                    aimAngle = machine.turretAngle;
                }

                if (pawn.Rotation == Rot4.South)
                    drawLoc -= south;
                else if (pawn.Rotation == Rot4.North)
                    drawLoc -= north;
                else if (pawn.Rotation == Rot4.East)
                    drawLoc -= east;
                else if (pawn.Rotation == Rot4.West)
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
            replaced = false;
        }
    }
}
