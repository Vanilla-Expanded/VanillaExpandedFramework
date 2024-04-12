using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFEMech
{
    public class TeslaChainingProps : DefModExtension
    {
        public bool addFire;
        public float bounceRange;
        public int maxBounceCount;
        public DamageDef damageDef;
        public DamageDef explosionDamageDef;
        public float impactRadius;
        public bool targetFriendly;
        public int maxLifetime;
        public SoundDef impactSound;
    }

    public class TeslaProjectile : Bullet
    {
        public int curLifetime;
        protected int numBounces;
        protected List<TeslaProjectile> allProjectiles = new List<TeslaProjectile>();
        protected List<Thing> prevTargets = new List<Thing>();
        private Thing holder;
        private Thing mainLauncher;
        private bool shotAnything;
        public Thing Holder
        {
            get
            {
                if (holder == null)
                {
                    return this.launcher;
                }
                return holder;
            }
        }

        /*[HarmonyPatch]
        public static class CheckPreAbsorbDamagePatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetMethods()
            {
                yield return AccessTools.Method(typeof(ShieldBelt), "CheckPreAbsorbDamage");
            }

            public static void Postfix(bool __result)
            {
                wasDeflected = __result;
            }
        }*/

        [HarmonyPatch]
        public static class ProjectilePatches
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetMethods()
            {
                yield return AccessTools.Method(typeof(Projectile), "ImpactSomething");
                yield return AccessTools.Method(typeof(Projectile), "CheckForFreeIntercept");
            }
            public static void Postfix()
            {
                wasDeflected = false;
            }
        }

        protected virtual int GetDamageAmount => this.def.projectile.GetDamageAmount(1f);

        protected virtual DamageInfo GetDamageInfo(Thing hitThing)
        {
            return new DamageInfo(Props.damageDef, GetDamageAmount, def.projectile.GetArmorPenetration(this.launcher), Holder.DrawPos.AngleToFlat(hitThing.DrawPos), this.Launcher);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            var oldValue = def.projectile.damageDef.isRanged; // all of this jazz is to make shield belt deflecting tesla projectiles
            def.projectile.damageDef.isRanged = true;
            base.Impact(hitThing);
            def.projectile.damageDef.isRanged = oldValue;

            if (this.mainLauncher == null)
            {
                this.mainLauncher = this.launcher;
            }

            if (equipmentDef == null)
            {
                equipmentDef = ThingDef.Named("Gun_Autopistol");
            }

            if (wasDeflected)
            {
                wasDeflected = false;
                if (Rand.Chance(0.3f))
                {
                    this.DestroyAll();
                }
                return;
            }
            if (hitThing == null && !shotAnything)
            {
                shotAnything = true;
            }
            else if (hitThing != null && !shotAnything)
            {
                BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
                Find.BattleLog.Add(battleLogEntry_RangedImpact);
                var dinfo = GetDamageInfo(hitThing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                if (Props.addFire && hitThing.TryGetComp<CompAttachBase>() != null && hitThing.Map!=null)
                {
                    var fire = (Fire)GenSpawn.Spawn(ThingDefOf.Fire, hitThing.Position, hitThing.Map);
                    fire.AttachTo(hitThing);
                }
                if (Props.impactRadius > 0f)
                {
                    GenExplosion.DoExplosion(hitThing.Position, Map, Props.impactRadius, Props.explosionDamageDef, this.Launcher, this.def.projectile.GetDamageAmount(1f));
                }

                Props.impactSound?.PlayOneShot(hitThing);
                RegisterHit(hitThing);
                if (numBounces < MaxBounceCount)
                {
                    var target = NextTarget(hitThing);
                    if (target != null)
                    {
                        FireAt(target);
                    }
                }
                shotAnything = true;
            }
        }

        protected virtual int MaxBounceCount => Props.maxBounceCount;

        public static bool wasDeflected;
        private void RegisterHit(Thing hitThing)
        {
            RegisterHit(this, hitThing);
            foreach (var projectile in allProjectiles)
            {
                RegisterHit(projectile, hitThing);
            }
        }

        private void RegisterHit(TeslaProjectile projectile, Thing hitThing)
        {
            if (!projectile.prevTargets.Contains(hitThing))
            {
                projectile.prevTargets.Add(hitThing);
            }
            projectile.curLifetime = 0;
        }

        public TeslaChainingProps Props => def.GetModExtension<TeslaChainingProps>();

        public Thing PrimaryEquipment
        {
            get
            {
                var launcher = PrimaryLauncher;
                if (launcher is Building_TurretGun turretGun)
                {
                    return turretGun.gun;
                }
                return null;
            }
        }

        public Verb PrimaryVerb
        {
            get
            {
                var launcher = PrimaryLauncher;
                if (launcher is Building_TurretGun turretGun)
                {
                    return turretGun.AttackVerb;
                }
                return null;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var vec1 = Holder.TrueCenter();
            var vec2 = this.DrawPos;
            if (vec2.magnitude > vec1.magnitude)
            {
                var t = vec1;
                vec1 = vec2;
                vec2 = t;
            }

            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(vec2 + (vec1 - vec2) / 2, Quaternion.AngleAxis(vec1.AngleToFlat(vec2) + 90f, Vector3.up), new Vector3(1f, 1f, (vec1 - vec2).magnitude)),
                Graphic.MatSingle, 0);
        }

        public void FireAt(Thing target)
        {
            var projectile = (TeslaProjectile)GenSpawn.Spawn(this.def, Position, Map);
            projectile.Launch(launcher, target, target, this.HitFlags, false, PrimaryEquipment);
            projectile.holder = this;
            if (this.mainLauncher != null)
            {
                projectile.mainLauncher = this.mainLauncher;
            }

            allProjectiles.Add(projectile);
            prevTargets.Add(target);
            if (projectile.prevTargets is null)
            {
                projectile.prevTargets = new List<Thing>();
            }
            projectile.prevTargets.AddRange(prevTargets);
            numBounces++;
            projectile.numBounces = numBounces;
            projectile.curLifetime = curLifetime;
        }

        private static readonly Func<Building_TurretGun, Thing, bool> isValidTarget = (Func<Building_TurretGun, Thing, bool>)Delegate.CreateDelegate(typeof(Func<Building_TurretGun, Thing, bool>),
            AccessTools.Method(typeof(Building_TurretGun), "IsValidTarget"));
        private bool IsValidTarget(Thing thing)
        {
            var launcher = PrimaryLauncher;
            if (launcher is Building_TurretGun turretGun && !isValidTarget(turretGun, thing))
            {
                return false;
            }
            var primaryVerb = PrimaryVerb;
            if (primaryVerb != null && !primaryVerb.targetParams.CanTarget(thing))
            {
                return false;
            }
            return true;
        }
        private Thing NextTarget(Thing currentTarget)
        {
            var things = GenRadial.RadialDistinctThingsAround(currentTarget.PositionHeld, Map, Props.bounceRange, false)
                .Where(t => (Props.targetFriendly || t.HostileTo(this.launcher)) && IsValidTarget(t)).Except(new[] { this, usedTarget.Thing });
            things = things.Except(prevTargets);
            things = things.OrderBy(t => t.Position.DistanceTo(Holder.Position));
            var target = things.FirstOrDefault();
            return target;
        }

        private Thing PrimaryLauncher
        {
            get
            {
                if (this.mainLauncher != null)
                {
                    return this.mainLauncher;
                }
                foreach (var projectile in this.allProjectiles)
                {
                    if (projectile.mainLauncher != null)
                    {
                        return projectile.mainLauncher;
                    }
                }
                return null;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (shotAnything)
            {
                this.curLifetime++;
            }
            if (curLifetime > Props.maxLifetime)
            {
                DestroyAll();
            }
            else if (this.Holder.Destroyed)
            {
                DestroyAll();
            }
            else if (allProjectiles.Any(x => x.Destroyed))
            {
                DestroyAll();
            }
        }
        public void DestroyAll()
        {
            destroyAll = true;
            for (var i = allProjectiles.Count - 1; i >= 0; i--)
            {
                if (!allProjectiles[i].Destroyed)
                {
                    allProjectiles[i].Destroy();
                }
            }
            this.Destroy();
            destroyAll = false;
        }

        public static bool destroyAll;
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (destroyAll)
            {
                base.Destroy(mode);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mainLauncher, "mainLauncher");
            Scribe_References.Look(ref holder, "holder");
            Scribe_Values.Look(ref numBounces, "numBounces");
            Scribe_Values.Look(ref curLifetime, "curLifetime");
            Scribe_Values.Look(ref shotAnything, "firedOnce");
            Scribe_Collections.Look(ref allProjectiles, "allProjectiles", LookMode.Reference);
            Scribe_Collections.Look(ref prevTargets, "prevTargets", LookMode.Reference);
        }
    }

}
