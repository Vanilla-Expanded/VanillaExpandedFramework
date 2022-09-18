using System;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using HarmonyLib;

namespace VanillaWeaponsExpandedLaser.HarmonyPatches
{
    
    [HarmonyPatch(typeof(Verb), "TryStartCastOn", new Type[] 
    { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
    public static class VWEL_Verb_Shoot_TryStartCastOn_RapidFire_Patch
    {
        [HarmonyPrefix, HarmonyPriority(100)]
        public static void TryStartCastOn_RapidFire_Prefix(ref Verb __instance, LocalTargetInfo castTarg)
        {
            if (__instance.GetType() == typeof(Verb_Shoot))
            {
                if (__instance.EquipmentSource != null && __instance.CasterPawn != null)
                {
                    Pawn pawn = __instance.CasterPawn;
                    if (castTarg != null /*&& castTarg.HasThing*/ && __instance.CanHitTarget(castTarg))
                    {
                        if (__instance.EquipmentSource.TryGetComp<CompLaserCapacitor>() != null)
                        {
                            if (__instance.EquipmentSource.GetComp<CompLaserCapacitor>() is CompLaserCapacitor GunExt)
                            {
                                if (GunExt.initalized == false)
                                {
                                    GunExt.originalwarmupTime = __instance.verbProps.warmupTime;
                                    GunExt.initalized = true;
                                }
                                if (GunExt.hotshot)
                                {
                                    CompEquippable eq = __instance.EquipmentSource.TryGetComp<CompEquippable>();
                                    IntVec3 lastpos = (IntVec3)GunExt.lastFiringLocation;
                                    if (lastpos == pawn.Position)
                                    {
                                        GunExt.shotstack++;
                                        __instance.verbProps.warmupTime = Math.Max(GunExt.originalwarmupTime - (GunExt.Props.WarmUpReductionPerShot * GunExt.shotstack), 0);
                                    }
                                    else
                                    {
                                        GunExt.shotstack = 0;
                                        __instance.verbProps.warmupTime = GunExt.originalwarmupTime;
                                    }
                                    GunExt.lastFiringLocation = pawn.Position;
                                    if (GunExt.Props.Overheats && __instance.verbProps.warmupTime == 0f)
                                    {
                                        if (Rand.Chance(GunExt.Props.OverheatChance))
                                        {
                                            GunExt.CriticalOverheatExplosion((Verb_Shoot)__instance);
                                            if (GunExt.Props.OverheatDestroys)
                                            {
                                                eq.parent.Destroy();
                                            }
                                        }
                                        else
                                        {
                                            if (GunExt.Props.OverheatMoteThrown != null)
                                            {

                                                MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(GunExt.Props.OverheatMoteThrown, null);
                                                moteThrown.Scale = Rand.Range(2f, 3f) * GunExt.Props.OverheatMoteSize;
                                                moteThrown.exactPosition = GunExt.lastFiringLocation.CenterVector3;
                                                moteThrown.rotationRate = Rand.Range(-0.5f, 0.5f);
                                                moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.008f, 0.012f)); 
                                                GenSpawn.Spawn(moteThrown, GunExt.lastFiringLocation.Cell, pawn.Map, WipeMode.Vanish);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (GunExt.originalwarmupTime!=0)
                                    {
                                        __instance.verbProps.warmupTime = GunExt.originalwarmupTime;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPostfix, HarmonyPriority(401)]
        public static void TryStartCastOn_RapidFire_Postfix(ref Verb __instance, LocalTargetInfo castTarg, float __state)
        {
            if (__instance.GetType() == typeof(Verb_Shoot))
            {
                if (__instance.EquipmentSource != null)
                {
                    if (!__instance.EquipmentSource.AllComps.NullOrEmpty())
                    {
                        if (__instance.EquipmentSource.TryGetComp<CompLaserCapacitor>() != null)
                        {
                            if (__instance.EquipmentSource.TryGetComp<CompLaserCapacitor>() is CompLaserCapacitor GunExt)
                            {
                                if (GunExt != null && GunExt.initalized)
                                {

                                    __instance.verbProps.warmupTime = GunExt.originalwarmupTime;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


}
