using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VWEMakeshift 
{
    public class Verb_MakeshiftShoot : Verb_ShootWithSmoke
	{
        protected override int ShotsPerBurst
        {
            get
            {
                ThingDef projectile = this.GetProjectile();
				ThingDef equipmentDef = EquipmentSource?.def;
				if (equipmentDef is null)
				{
					Log.Error($"Unable to retrieve weapon def from <color=teal>{GetType()}</color>. Please report to Oskar or Smash Phil.");
					return base.ShotsPerBurst;
				}
				MakeshiftProperties makeshiftProps = equipmentDef.GetModExtension<MakeshiftProperties>();
				if (makeshiftProps is null)
				{
					Log.ErrorOnce($"<color=teal>{GetType()}</color> cannot be used without <color=teal>MakeshiftProperties</color> DefModExtension. Motes will not be thrown.", 
						Gen.HashCombine(projectile.GetHashCode(), "MakeshiftProperties".GetHashCode()));
					return base.ShotsPerBurst;
				}
				int shots = makeshiftProps.shots.RandomInRange;
				return shots;
            }
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
        {
			if (ShotsPerBurst <= 0)
            {
				ticksToNextBurstShot = verbProps.ticksBetweenBurstShots;
				if (CasterIsPawn && !verbProps.nonInterruptingSelfCast)
				{
					CasterPawn.stances.SetStance(new Stance_Cooldown(verbProps.ticksBetweenBurstShots + 1, currentTarget, this));
				}
				return false;
            }
            return base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns);
        }
    }
}
