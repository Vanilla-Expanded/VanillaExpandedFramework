using System.Collections.Generic;
using Verse;

namespace VFECore.Abilities
{
    using RimWorld.Planet;

    public class Ability_Explode : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (def.GetModExtension<AbilityExtension_Explosion>() is AbilityExtension_Explosion ext)
            {
                foreach (GlobalTargetInfo target in targets)
                {
                    GenExplosion.DoExplosion(ext.onCaster ? pawn.Position : target.Cell, pawn.Map, ext.explosionRadius, ext.explosionDamageDef, pawn,
                                             ext.explosionDamageAmount, ext.explosionArmorPenetration, ext.explosionSound, null, null, null, ext.postExplosionSpawnThingDef,
                                             ext.postExplosionSpawnChance, ext.postExplosionSpawnThingCount, ext.postExplosionGasType, ext.applyDamageToExplosionCellsNeighbors, ext.preExplosionSpawnThingDef,
                                             ext.preExplosionSpawnChance, ext.preExplosionSpawnThingCount, ext.chanceToStartFire, ext.damageFalloff, ext.explosionDirection,
                                             ext.casterImmune ? new List<Thing> { this.pawn } : null);
                }
            }
        }
    }

    public class AbilityExtension_Explosion : DefModExtension
    {
        public bool      applyDamageToExplosionCellsNeighbors;
        public float     chanceToStartFire;
        public bool      damageFalloff;
        public float     explosionArmorPenetration = -1f;
        public int       explosionDamageAmount     = -1;
        public DamageDef explosionDamageDef;
        public float?    explosionDirection;
        public float     explosionRadius;
        public SoundDef  explosionSound;
        public bool      onCaster;
        public float     postExplosionSpawnChance;
        public int       postExplosionSpawnThingCount = 1;
        public GasType?  postExplosionGasType = null;
        public ThingDef  postExplosionSpawnThingDef;
        public float     preExplosionSpawnChance;
        public int       preExplosionSpawnThingCount = 1;
        public ThingDef  preExplosionSpawnThingDef;
        public bool      casterImmune;
    }
}