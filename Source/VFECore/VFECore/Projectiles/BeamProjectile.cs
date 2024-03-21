using System;
using System.Collections.Generic;
using HarmonyLib;
using MonoMod.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VFECore;

[StaticConstructorOnStartup]
public class BeamProjectile : Projectile_Explosive
{
    private static readonly Dictionary<ThingDef, ThingDef> DRAWERS;


    private static readonly Dictionary<Type, HashSet<ushort>> takenHashesPerDeftype;

    private static readonly Action<Def, Type, HashSet<ushort>> giveShortHash;

    static BeamProjectile()
    {
        DRAWERS               = new();
        takenHashesPerDeftype = (Dictionary<Type, HashSet<ushort>>)AccessTools.Field(typeof(ShortHashGiver), "takenHashesPerDeftype").GetValue(null);
        giveShortHash = (Action<Def, Type, HashSet<ushort>>)
            AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash").CreateDelegate(typeof(Action<Def, Type, HashSet<ushort>>));
        foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            if (thingDef.thingClass != null && typeof(BeamProjectile).IsAssignableFrom(thingDef.thingClass))
            {
                var drawer     = BaseBeamDrawer();
                var affectsSky = thingDef.GetCompProperties<CompProperties_AffectsSky>();
                var extension  = thingDef.GetModExtension<ProjectileExtension>();
                drawer.comps         = new();
                drawer.graphicData   = thingDef.graphicData;
                drawer.modExtensions = new();
                drawer.defName       = thingDef.defName + "Drawer";
                if (affectsSky != null) drawer.comps.Add(affectsSky);
                if (extension  != null) drawer.modExtensions.Add(extension);
                DRAWERS.Add(thingDef, drawer);
            }

        foreach (var drawer in DRAWERS.Values)
        {
            GiveShortHash(drawer, typeof(ThingDef));
            DefGenerator.AddImpliedDef(drawer);
        }
    }

    public Vector3 Origin => origin;
    public Vector3 Dest   => destination;

    public static void GiveShortHash(Def def, Type defType)
    {
        giveShortHash(def, defType, takenHashesPerDeftype[defType]);
    }

    private static ThingDef BaseBeamDrawer() =>
        new()
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

    protected override void DrawAt(Vector3 drawLoc, bool flip = false) { }

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var drawer = (BeamDraw)ThingMaker.MakeThing(DRAWERS[def]);
        drawer.Setup(origin, destination);
        var pos = ExactPosition.ToIntVec3();
        var map = launcher.Map;
        GenSpawn.Spawn(drawer, pos, map);
        base.Impact(null);
    }

    public override int DamageAmount => Mathf.RoundToInt(base.DamageAmount
                                                       * (this.TryGetComp<CompAbilityProjectile>() is { } comp ? comp.ability.GetPowerForPawn() : 1f));
}

public class BeamDraw : ThingWithComps
{
    private Vector3             a;
    private Vector3             b;
    private Matrix4x4           drawMatrix;
    private Material            material;
    private ProjectileExtension projectileExt;
    private int                 ticksRemaining;

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
            Quaternion.LookRotation(b - a), new(def.graphicData.drawSize.x, 1f, (b - a).magnitude));
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

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Graphics.DrawMesh(MeshPool.plane10, drawMatrix,
            FadedMaterialPool.FadedVersionOf(material, (float)ticksRemaining / projectileExt.beamLifetimeTicks), 0);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
        Scribe_Values.Look(ref a,              "a");
        Scribe_Values.Look(ref b,              "b");
    }
}
