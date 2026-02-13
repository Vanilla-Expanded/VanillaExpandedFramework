using RimWorld;
using Verse;

namespace VEF.Weapons;

public class Verb_Shoot_FlyOverhead : Verb_Shoot
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