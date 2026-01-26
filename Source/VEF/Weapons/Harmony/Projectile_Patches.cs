using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VEF.Hediffs;
using Verse.Sound;
using System.Reflection.Emit;
using System.Reflection;


namespace VEF.Weapons
{
    [HarmonyPatch]
    public static class VanillaExpandedFramework_VehicleFramework_Turret_Patch
    {
        public static bool VFLoaded = ModsConfig.IsActive("SmashPhil.VehicleFramework");
        public static MethodInfo targetMethod;
        public static FastInvokeHandler maxRangeInfo;
        public static FastInvokeHandler turretLocation;
        public static FastInvokeHandler turretRotation;
        public static AccessTools.FieldRef<object, Vector2> aimPieOffset;
        public static Type VehicleType;
        public static bool Prepare(MethodBase target)
        {
            // Always allow on second pass
            if (target != null)
                return true;

            if (VFLoaded)
            {
                VehicleType = AccessTools.TypeByName("Vehicles.VehiclePawn");
                targetMethod = AccessTools.Method("Vehicles.VehicleTurret:FireTurret");
                var maxRangeInfoGetter = AccessTools.PropertyGetter("Vehicles.VehicleTurret:MaxRange");
                var turretLocationGetter = AccessTools.PropertyGetter("Vehicles.VehicleTurret:TurretLocation");
                var turretRotationGetter = AccessTools.PropertyGetter("Vehicles.VehicleTurret:TurretRotation");
                var aimPieOffsetField = AccessTools.Field("Vehicles.VehicleTurret:aimPieOffset");
                if (VehicleType is not null && targetMethod is not null && maxRangeInfoGetter is not null && turretLocationGetter is not null 
                    && turretRotationGetter is not null && aimPieOffsetField is not null)
                {
                    maxRangeInfo = MethodInvoker.GetHandler(maxRangeInfoGetter);
                    turretLocation = MethodInvoker.GetHandler(turretLocationGetter);
                    turretRotation = MethodInvoker.GetHandler(turretRotationGetter);
                    aimPieOffset = AccessTools.FieldRefAccess<object, Vector2>(aimPieOffsetField);

                    return true;
                }
                Log.Error("[VEF] Failed to patch VehicleFramework, vehicle turrets will not work with some expendable projectiles");
            }
            return false;
        }

        public static MethodBase TargetMethod() => targetMethod;

        public static object currentFiringVehicleTurret;

        public static void Prefix(object __instance)
        {
            currentFiringVehicleTurret = __instance;
        }

        public static void Finalizer()
        {
            currentFiringVehicleTurret = null;
        }
    }
    [HarmonyPatch(typeof(Projectile), "Launch", new Type[]
    {
        typeof(Thing),
        typeof(Vector3),
        typeof(LocalTargetInfo),
        typeof(LocalTargetInfo),
        typeof(ProjectileHitFlags),
        typeof(bool),
        typeof(Thing),
        typeof(ThingDef)
    })]
    public static class VanillaExpandedFramework_Projectile_Launch_Patch
    {

        public static void Postfix(Projectile __instance, Thing launcher, Vector3 origin, ref LocalTargetInfo usedTarget, 
            LocalTargetInfo intendedTarget, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            if (__instance is ExpandableProjectile expandableProjectile)
            {
                if (VanillaExpandedFramework_VehicleFramework_Turret_Patch.VFLoaded && VanillaExpandedFramework_VehicleFramework_Turret_Patch.currentFiringVehicleTurret is not null)
                {
                    var turretLocation = (Vector3)VanillaExpandedFramework_VehicleFramework_Turret_Patch
                        .turretLocation(VanillaExpandedFramework_VehicleFramework_Turret_Patch.currentFiringVehicleTurret, null);
                    var turretRotation = (float)VanillaExpandedFramework_VehicleFramework_Turret_Patch
                        .turretRotation(VanillaExpandedFramework_VehicleFramework_Turret_Patch.currentFiringVehicleTurret, null);
                    var aimPieOffset = VanillaExpandedFramework_VehicleFramework_Turret_Patch
                        .aimPieOffset(VanillaExpandedFramework_VehicleFramework_Turret_Patch.currentFiringVehicleTurret);
                    expandableProjectile.startingPosition = turretLocation 
                        + (new Vector3(aimPieOffset.x, Altitudes.AltInc, aimPieOffset.y).RotatedBy(turretRotation));
                }
                if (expandableProjectile.def.reachMaxRangeAlways && equipment != null)
                {
                    expandableProjectile.SetDestinationToMax(equipment);
                }
            }
            else if (__instance.IsHomingProjectile(out var comp))
            {
                __instance.usedTarget = __instance.intendedTarget;
                __instance.SetDestination(__instance.intendedTarget.CenterVector3 + comp.DispersionOffset);
                comp.originLaunchCell = NonPublicFields.Projectile_origin(__instance);
            }
            else if (launcher is Pawn pawn && pawn.health.hediffSet.hediffs.Any(x => x.TryGetComp<HediffComp_Targeting>()?.Props.neverMiss ?? false))
            {
                __instance.usedTarget = __instance.intendedTarget;
                __instance.SetDestination(__instance.intendedTarget.CenterVector3);
            }
        }


        public static void SetDestination(this Projectile projectile, Vector3 destination)
        {
            ref var projDestination = ref NonPublicFields.Projectile_destination(projectile);
            float distanceBetweenDestinations = Vector3.Distance(projDestination.Yto0(), destination.Yto0());
            if (distanceBetweenDestinations >= 0.1f)
            {
                ref Vector3 origin = ref NonPublicFields.Projectile_origin(projectile);
                Vector3 newPos = new Vector3(projectile.ExactPosition.x, origin.y, projectile.ExactPosition.z);
                origin = newPos;
                projDestination = destination;
                NonPublicFields.Projectile_ticksToImpact(projectile) =
                    Mathf.CeilToInt(NonPublicProperties.Projectile_get_StartingTicksToImpact(projectile) - 1);
            }
        }
        public static bool IsHomingProjectile(this Projectile projectile, out CompHomingProjectile comp)
        {
            comp = projectile.GetComp<CompHomingProjectile>();
            return comp != null;
        }
    }

    [HarmonyPatch]
    public static class VanillaExpandedFramework_Projectile_SetTrueOrigin_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(Projectile).Method("CheckForFreeInterceptBetween");
            yield return typeof(Projectile).Method("CheckForFreeIntercept");
            yield return typeof(Projectile).Method("ImpactSomething");
        }

        public static MethodInfo InterceptChanceFactorFromDistanceInfo = typeof(Verse.VerbUtility).Method("InterceptChanceFactorFromDistance");
        public static FieldInfo Projectile_origin = typeof(Projectile).Field("origin");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, MethodBase baseMethod)
        {
            var patched = false;
            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var codeInstruction = codes[i];
                bool shouldPatch = codeInstruction.LoadsField(Projectile_origin) &&
                                  codes.Skip(i + 1).Any(c => c.Calls(InterceptChanceFactorFromDistanceInfo));

                yield return codeInstruction;

                if (shouldPatch)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_Projectile_SetTrueOrigin_Patch), nameof(GetTrueOrigin)));
                    patched = true;
                }
            }

            if (!patched)
                Log.Error($"[VEF] Error patching homing projectiles - couldn't patch Projectile.origin in {baseMethod.DeclaringType?.Namespace}.{baseMethod.DeclaringType?.Name}:{baseMethod.Name}");
        }

        public static Vector3 GetTrueOrigin(Vector3 origin, Projectile projectile)
        {
            if (projectile.IsHomingProjectile(out var comp))
            {
                return comp.originLaunchCell;
            }
            return origin;
        }
    }

    [HarmonyPatch(typeof(Projectile), "Tick")]
    public static class VanillaExpandedFramework_Projectile_Tick_Patch
    {
        public static void Postfix(Projectile __instance)
        {
            if (__instance.IsHomingProjectile(out var comp))
            {
                if (comp.CanChangeTrajectory())
                {
                    __instance.SetDestination(__instance.intendedTarget.CenterVector3);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class VanillaExpandedFramework_Projectile_Impact_Patch
    {
        public static bool Prefix(Projectile __instance, ref Thing hitThing)
        {
            if (__instance.IsHomingProjectile(out var comp))
            {
                if (hitThing != __instance.intendedTarget.Thing)
                {
                    foreach (var thing in GenRadial.RadialDistinctThingsAround(__instance.Position, __instance.Map, 3f, true))
                    {
                        if (thing == __instance.intendedTarget.Thing)
                        {
                            if (Vector3.Distance(thing.DrawPos.Yto0(), __instance.ExactPosition.Yto0()) <= 0.5f)
                            {
                                hitThing = thing;
                            }
                        }
                    }
                }
                if (hitThing != null && comp.Props.hitSound != null)
                {
                    comp.Props.hitSound.PlayOneShot(hitThing);
                }
            }
            return true;
        }
    }
}
