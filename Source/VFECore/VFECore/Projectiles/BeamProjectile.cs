﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using MonoMod.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public class BeamProjectile : Projectile_Explosive
    {
        private static readonly Dictionary<ThingDef, ThingDef> DRAWERS;
        private static readonly Action<Def, Type>              giveShortHash;

        public         Vector3                        Origin => origin;
        public         Vector3                        Dest   => destination;

        static BeamProjectile()
        {
            DRAWERS       = new Dictionary<ThingDef, ThingDef>();
            giveShortHash = AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash").CreateDelegate<Action<Def, Type>>();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
                if (thingDef.thingClass != null && typeof(BeamProjectile).IsAssignableFrom(thingDef.thingClass))
                {
                    var drawer     = BaseBeamDrawer();
                    var affectsSky = thingDef.GetCompProperties<CompProperties_AffectsSky>();
                    var extension  = thingDef.GetModExtension<ProjectileExtension>();
                    drawer.comps         = new List<CompProperties>();
                    drawer.graphicData   = thingDef.graphicData;
                    drawer.modExtensions = new List<DefModExtension>();
                    drawer.defName       = thingDef.defName + "Drawer";
                    if (affectsSky != null) drawer.comps.Add(affectsSky);
                    if (extension  != null) drawer.modExtensions.Add(extension);
                    DRAWERS.Add(thingDef, drawer);
                }

            foreach (var drawer in DRAWERS.Values)
            {
                giveShortHash(drawer, typeof(ThingDef));
                DefGenerator.AddImpliedDef(drawer);
            }
        }

        private static ThingDef BaseBeamDrawer() =>
            new ThingDef
            {
                thingClass       = typeof(BeamDraw),
                drawOffscreen    = true,
                label            = "beam",
                category         = ThingCategory.Projectile,
                tickerType       = TickerType.Normal,
                altitudeLayer    = AltitudeLayer.MoteOverhead,
                useHitPoints     = false,
                selectable       = false,
                neverMultiSelect = true,
                drawerType       = DrawerType.RealtimeOnly
            };

        public override void Draw()
        {
        }

        protected override void Impact(Thing hitThing)
        {
            var drawer = (BeamDraw) ThingMaker.MakeThing(DRAWERS[def]);
            drawer.Setup(origin, destination);
            var pos = ExactPosition.ToIntVec3();
            var map = launcher.Map;
            GenSpawn.Spawn(drawer, pos, map);
            base.Impact(null);
        }
    }

    public class BeamDraw : ThingWithComps
    {
        private Vector3             a;
        private Vector3             b;
        private Matrix4x4           drawMatrix;
        private ProjectileExtension projectileExt;
        private int                 ticksRemaining;
        private Material            material;

        public void Setup(Vector3 origin, Vector3 dest)
        {
            a = origin.Yto0();
            b = dest.Yto0();
            Recache();
            ticksRemaining = projectileExt.beamLifetimeTicks;
            GetComp<CompAffectsSky>()
                ?.StartFadeInHoldFadeOut(projectileExt.beamSkyFadeInTicks, projectileExt.beakSkyHoldTikcs, projectileExt.beakSkyFadeOutTicks);
        }

        private void Recache()
        {
            projectileExt = def.GetModExtension<ProjectileExtension>() ?? new ProjectileExtension();
            drawMatrix.SetTRS((a          + b) / 2 + Vector3.up * def.Altitude,
                Quaternion.LookRotation(b - a), new Vector3(def.graphicData.drawSize.x, 1f, (b - a).magnitude));
            material = MaterialPool.MatFrom(def.graphicData.texPath, ShaderDatabase.MoteGlow);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) Recache();
        }

        public override void Tick()
        {
            if (ticksRemaining == projectileExt.beamLifetimeTicks)
            {
                if (projectileExt.flashIntensity > 0)
                    FleckMaker.Static(b + Vector3.up * def.Altitude + Altitudes.AltIncVect / 2, Map, FleckDefOf.ExplosionFlash, projectileExt.flashIntensity);

                if (projectileExt.hitFleck != null) FleckMaker.Static(b + Vector3.up * def.Altitude + Altitudes.AltIncVect, Map, projectileExt.hitFleck);
            }

            ticksRemaining--;
            if (ticksRemaining <= 0) Destroy();
        }

        public override void Draw()
        {
            Graphics.DrawMesh(MeshPool.plane10, drawMatrix,
                FadedMaterialPool.FadedVersionOf(material, (float) ticksRemaining / projectileExt.beamLifetimeTicks), 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
            Scribe_Values.Look(ref a,              "a");
            Scribe_Values.Look(ref b,              "b");
        }
    }
}