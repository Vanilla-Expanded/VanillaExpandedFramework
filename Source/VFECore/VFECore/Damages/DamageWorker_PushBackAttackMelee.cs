using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFECore
{
    public class DamageWorker_PushBackAttackMelee : DamageWorker_Blunt
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (dinfo.Instigator != null && thing is Pawn)
            {
                var extension = this.def.GetModExtension<DamageExtension>();
                TryToKnockBack(dinfo.Instigator, thing, extension.pushBackDistance.RandomInRange, extension);
            }
            return base.Apply(dinfo, thing);
        }

        private void TryToKnockBack(Thing attacker, Thing thing, float knockBackDistance, DamageExtension extension)
        {
            float distanceDiff = attacker.Position.DistanceTo(thing.Position) < knockBackDistance ? attacker.Position.DistanceTo(thing.Position) : knockBackDistance;
            Predicate<IntVec3> validator = delegate (IntVec3 x)
            {
                if (x.DistanceTo(thing.Position) < knockBackDistance)
                {
                    return false;
                }
                if (!x.Walkable(thing.Map) || !GenSight.LineOfSight(thing.Position, x, thing.Map))
                {
                    return false;
                }
                var attackerToVictimDistance = attacker.Position.DistanceTo(thing.Position);
                var attackerToCellDistance = attacker.Position.DistanceTo(x);
                var victimToCellDistance = thing.Position.DistanceTo(x);

                if (attackerToVictimDistance > attackerToCellDistance)
                {
                    return false;
                }
                if (attackerToCellDistance > victimToCellDistance + (distanceDiff - 1))
                {
                    return true;
                }
                else if (attacker.Position == thing.Position)
                {
                    return true;
                }
                return false;
            };
            var cells = GenRadial.RadialCellsAround(thing.Position, knockBackDistance + 1, true).Where(x => validator(x));
            if (cells.Any())
            {
                var cell = cells.RandomElement();
                thing.Position = cell;
                if (thing is Pawn victim)
                {
                    victim.pather.StopDead();
                    victim.jobs.StopAll();
                }
                if (extension.fleckOnDamage != null)
                {
                    var target = extension.fleckOnInstigator ? attacker : thing;
                    FleckMaker.Static(target.Position, target.Map, extension.fleckOnDamage, extension.fleckRadius);
                }
            }
        }
    }
}
