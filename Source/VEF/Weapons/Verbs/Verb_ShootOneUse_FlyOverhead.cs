using RimWorld;
using Verse;

namespace VEF.Weapons;

public class Verb_ShootOneUse_FlyOverhead : Verb_ShootOneUse
{
    public override void OrderForceTarget(LocalTargetInfo target)
    {
        if (this.ProjectileFliesOverhead() && caster is { Map: { } map, Position: { IsValid: true } pos } && map.roofGrid?.Roofed(pos) == true)
        {
            Messages.Message($"{"CannotFire".Translate()}: {"Roofed".Translate().CapitalizeFirst()}", MessageTypeDefOf.RejectInput, false);
            return;
        }

        base.OrderForceTarget(target);
    }
}