using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Weapons
{
	public class GaussProjectile : ExpandableProjectile
	{
		public float damageFalloff;

		public override int DamageAmount => def.gauss.Worker.DamageAmount(this, equipment, hitThings);

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);

			// Only draw effects if projectile isn't stopped, not out of bounds, and players is on current map
			if (!stopped && ExactPosition.ShouldSpawnMotesAt(Map))
			{
				Vector3 velocityDirection = this.ExactRotation * Vector3.forward;
				// Rather than drawing the effect ahead of center point of the projectile (like with the comp),
				// we instead draw it behind the tip of the projectile (so it's slightly trailing behind it).
				Vector3 effectPos = ExactPosition - velocityDirection;
				float angle = this.ExactPosition.AngleToFlat(effectPos) - 90;

				if (def.gauss.lightningGlow)
					VefFleckMaker.MakeLightningGlow(Map, effectPos, angle, 0.01f * def.projectile.speed, Rand.Range(0.3f, 0.6f));

				if (def.gauss.gaussDistortion)
					VefFleckMaker.MakeGaussDistortion(Map, effectPos, angle + Rand.Range(-15f, 15f), def.projectile.speed, Rand.Range(0.2f, 0.5f));
			}
		}

		public override void DoDamage(IntVec3 pos)
		{
			if (!stopped)
			{
                base.DoDamage(pos);
                if (pos != this.launcher.Position && this.launcher.Map != null && pos.InBounds(this.launcher.Map))
                {
                    var list = this.launcher.Map.thingGrid.ThingsListAt(pos);
                    for (int num = list.Count - 1; num >= 0; num--)
                    {
                        if (IsDamagable(list[num]) && !def.gauss.altitudeLayersBlackList.Contains(list[num].def.altitudeLayer))
                        {
	                        try
	                        {
	                            this.customImpact = true;
		                        base.Impact(list[num]);
	                        }
	                        finally
	                        {
			                    this.customImpact = false;
	                        }
                        }
                    }
                }
            }
        }

        public override bool IsDamagable(Thing t)
        {
	        // Damage checks against a pawn to avoid damage, but only if not an intended target
            if (t is Pawn pawn && intendedTarget.Thing != pawn)
            {
	            // Friendly fire check
                if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                {
	                if (preventFriendlyFire)
		                return false;
                    if (!Rand.Chance(Find.Storyteller.difficulty.friendlyFireChanceFactor))
                        return false;
                    if (def.gauss.includeInterceptChanceFromDistanceForFriendlyFire && !Rand.Chance(Verse.VerbUtility.InterceptChanceFactorFromDistance(startingPosition, t.Position)))
	                    return false;
                }
                if (!Rand.Chance(def.gauss.chanceToHitUnintendedLayingTarget) && pawn.GetPosture() != PawnPosture.Standing)
                    return false;
            }
            return base.IsDamagable(t);
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
	        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

	        if (equipment == null || def.gauss.damageModifierStat == null)
		        damageFalloff = VEFDefOf.VEF_GaussProjectileDamageModifier.defaultBaseValue;
	        else
		        damageFalloff = equipment.GetStatValue(def.gauss.damageModifierStat);
        }

        public override void ExposeData()
        {
	        base.ExposeData();

	        Scribe_Values.Look(ref damageFalloff, nameof(damageFalloff));
        }
	}
}
