using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace VEF.Weapons
{
    public class Verb_ShootWithFlintlockSmoke : Verb_Shoot
    {
        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                Vector3 loc = this.caster.PositionHeld.ToVector3();
                Map mapHeld = this.caster.MapHeld;
                ThingDef projectile = this.GetProjectile();
                int? num;
                if (projectile == null)
                {
                    num = null;
                }
                else
                {
                    ProjectileProperties projectile2 = projectile.projectile;
                    num = ((projectile2 != null) ? new int?(projectile2.GetDamageAmount(this.caster, null)) : null);
                }
                int? num2 = num;
                float size = Mathf.Clamp01(((num2 != null) ? new float?((float)num2.GetValueOrDefault() / 32) : null) ?? 1f);
                SmokeMaker.ThrowFlintLockSmoke(loc, mapHeld, size);
                SmokeMaker.ThrowFlintLockSmoke(loc, mapHeld, size);
                return true;
            }
            return false;
        }

      
    }
}

