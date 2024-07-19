using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch]
    public class CompExplosive_Shells : CompExplosive
    {
        public new CompProperties_Explosive_Shells Props => props as CompProperties_Explosive_Shells;

        public void DetonateExtra(Map map, bool ignoreUnspawned = false)
        {
            var shell = Props.shell ?? ThingDefOf.Shell_HighExplosive;
            var bullet = shell?.projectileWhenLoaded ?? shell;
            var bullets = new List<Thing>();
            for (var i = Props.shellCount.RandomInRange; i-- > 0;)
            {
                var proj = (Projectile)ThingMaker.MakeThing(bullet);
                var rot = Rot4.Random;
                var angle = rot.AsAngle + Rand.Range(-45f, 45f);
                var dest = (parent.TrueCenter() + (Vector3.right * Props.shellDist.RandomInRange).RotatedBy(angle) - Gen.RandomHorizontalVector(0.15f)).ToIntVec3();
                var origin = parent.OccupiedRect().ExpandedBy(1).ClosestCellTo(dest);
                GenSpawn.Spawn(proj, origin, map, rot);
                proj.Launch(parent, dest, dest, ProjectileHitFlags.All);
                bullets.Add(proj);
            }

            AddThingsIgnoredByExplosion(bullets);
        }

        [HarmonyPatch(typeof(CompExplosive), "Detonate")]
        [HarmonyPrefix]
        public static void Detonate_Prefix(CompExplosive __instance, Map map, bool ignoreUnspawned = false)
        {
            if (__instance is CompExplosive_Shells shells) shells.DetonateExtra(map, ignoreUnspawned);
        }
    }

    public class CompProperties_Explosive_Shells : CompProperties_Explosive
    {
        public ThingDef shell;
        public IntRange shellCount;
        public IntRange shellDist;

        public CompProperties_Explosive_Shells() => compClass = typeof(CompExplosive_Shells);
    }
}

