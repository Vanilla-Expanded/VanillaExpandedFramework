using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore;

public class Projectile_Shrapnel : Projectile_Explosive
{
    public ProjectileProperties_Shrapnel Props => def.projectile as ProjectileProperties_Shrapnel;

    public override void Tick()
    {
        base.Tick();
        if (this.IsHashIntervalTick(5) && def.projectile.SpeedTilesPerTick * ticksToImpact <= Props.shrapnelRange) Explode();
    }

    protected override void Explode()
    {
        def.projectile.soundExplode.PlayOneShot(this);
        for (var i = 0; i < Props.shrapnelCount; i++)
        {
            var angle = origin.AngleToFlat(destination) + Rand.Range(-Props.angleVariance, Props.angleVariance);
            var dist = def.projectile.SpeedTilesPerTick * ticksToImpact;
            var shrapnel = ThingMaker.MakeThing(Props.shrapnelProjectile);
            var dest = ExactPosition + (Vector3.right * dist).RotatedBy(angle) - Gen.RandomHorizontalVector(0.15f);
            GenSpawn.Spawn(shrapnel, ExactPosition.ToIntVec3(), Map);
            if (shrapnel is Projectile_ShrapnelPiece piece) piece.Launch(launcher, ExactPosition, dest, equipmentDef, weaponDamageMultiplier);
            else if (shrapnel is Projectile proj) proj.Launch(launcher, ExactPosition, dest.ToIntVec3(), dest.ToIntVec3(), ProjectileHitFlags.All);
            else shrapnel.Rotation = Rot4.FromAngleFlat(angle);
        }

        Destroy();
    }
}

public class Projectile_ShrapnelPiece : Bullet
{
    private IntVec3 prevPos;

    public void Launch(Thing launcher, Vector3 origin, Vector3 dest, ThingDef equipmentDef, float weaponDamageMult)
    {
        this.launcher = launcher;
        this.origin = origin;
        destination = dest;
        this.equipmentDef = equipmentDef;
        weaponDamageMultiplier = weaponDamageMult;
        HitFlags = ProjectileHitFlags.All;
        ticksToImpact = Mathf.CeilToInt(StartingTicksToImpact);
        if (ticksToImpact < 1) ticksToImpact = 1;
    }

    public override void Tick()
    {
        var pos = ExactPosition.ToIntVec3();
        if (prevPos != pos)
        {
            prevPos = pos;
            foreach (var thing in pos.GetThingList(Map))
                if (CanHit(thing) && thing.def.category is ThingCategory.Building or ThingCategory.Pawn)
                {
                    Impact(thing);
                    return;
                }
        }

        base.Tick();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref prevPos, nameof(prevPos));
    }
}

public class ProjectileProperties_Shrapnel : ProjectileProperties
{
    public float angleVariance;
    public int shrapnelCount;
    public ThingDef shrapnelProjectile;
    public float shrapnelRange;
}