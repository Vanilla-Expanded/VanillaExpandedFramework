using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VEF.Weapons;

public static class WeaponsPawnGizmos
{
    public static IEnumerable<Gizmo> GetGizmos(Pawn pawn, IEnumerable<Gizmo> gizmos)
    {
        var CompLaserCapacitor = pawn.equipment?.Primary?.GetComp<CompLaserCapacitor>();
        if (CompLaserCapacitor != null)
            if (GizmoGetter(CompLaserCapacitor).Any())
                if (pawn.Faction == Faction.OfPlayer)
                    return gizmos.Concat(GizmoGetter(CompLaserCapacitor));

        return gizmos;
    }

    public static IEnumerable<Gizmo> GizmoGetter(CompLaserCapacitor CompWargearWeapon) => CompWargearWeapon.CompGetGizmosExtra();
}