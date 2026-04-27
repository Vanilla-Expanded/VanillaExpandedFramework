using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VEF.Weapons;

public abstract class GaussProjectileDamageWorker
{
    public ExpandableProjectileDef def;

    public abstract int DamageAmount(GaussProjectile projectile, Thing equipment, List<Thing> hitThings);
}

public class GaussProjectileDefaultDamageWorker : GaussProjectileDamageWorker
{
    // This worker must either not use the def, or check for and handle null, as it's used for default worker (no def there) in case properties aren't specified.

    public override int DamageAmount(GaussProjectile projectile, Thing equipment, List<Thing> hitThings)
    {
        var baseDamage = projectile.def.projectile.GetDamageAmount(equipment);
        var damageMultiplier = 1f;
        damageMultiplier += projectile.hitThings.Count / 10f;
        var damageAmount = (int)(baseDamage / damageMultiplier);
        return damageAmount;
    }
}

public class GaussProjectileLinearDamageWorker : GaussProjectileDamageWorker
{
    public override int DamageAmount(GaussProjectile projectile, Thing equipment, List<Thing> hitThings)
    {
        if (projectile.damageFalloff == 0)
            return projectile.def.projectile.GetDamageAmount(equipment);

        var hits = 0;
        for (var i = 0; i < hitThings.Count; i++)
        {
            if (hitThings[i] is Pawn)
                hits++;
        }

        var falloff = 1 + (projectile.damageFalloff * hits);
        if (falloff <= 0)
            return 0;

        return Mathf.Max(0, Mathf.RoundToInt(projectile.def.projectile.GetDamageAmount(equipment) * falloff));
    }
}