using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace VEF.Weapons
{
    [HarmonyPatch(typeof(TurretTop), "DrawTurret"), StaticConstructorOnStartup]
    class VanillaExpandedFramework_TurretTop_DrawTurret_Patch
    {
        static bool Prefix(TurretTop __instance, Building_Turret ___parentTurret)
        {
            if (___parentTurret is not Building_LaserGun turret) return true;

            float rotation = __instance.CurRotation;
            if (turret.TargetCurrentlyAimingAt.HasThing)
            {
                rotation = (turret.TargetCurrentlyAimingAt.CenterVector3 - turret.TrueCenter()).AngleFlat();
            }

            if (turret.gun is IDrawnWeaponWithRotation gunRotation) rotation += gunRotation.RotationOffset;

            Material material = turret.def.building.turretTopMat;
            if (turret.gun is SpinningLaserGunTurret spinningGun)
            {
                spinningGun.turret = turret;
                material = spinningGun.Graphic.MatSingle;
            }

            Vector3 b = new Vector3(turret.def.building.turretTopOffset.x, 0f, turret.def.building.turretTopOffset.y);
            float turretTopDrawSize = turret.def.building.turretTopDrawSize;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(turret.DrawPos + Altitudes.AltIncVect + b, rotation.ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);

            return false;
        }
    }
}
