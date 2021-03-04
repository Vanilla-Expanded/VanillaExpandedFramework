using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VWEMakeshift 
{
    public class Verb_ShootWithSmoke : Verb_Shoot
	{
		protected override bool TryCastShot()
		{
			if (base.TryCastShot())
			{
				ThingDef projectile = this.GetProjectile();
				ProjectileProperties projectile2 = projectile.projectile;
				ThingDef equipmentDef = EquipmentSource?.def;
				if (equipmentDef is null)
                {
					Log.Error($"Unable to retrieve weapon def from <color=teal>{GetType()}</color>. Please report to Oskar or Smash Phil.");
					return true;
                }
				MoteProperties moteProps = equipmentDef.GetModExtension<MoteProperties>();
				if (moteProps is null)
                {
					Log.ErrorOnce($"<color=teal>{GetType()}</color> cannot be used without <color=teal>MoteProperties</color> DefModExtension. Motes will not be thrown.", 
						Gen.HashCombine(projectile.GetHashCode(), "MoteProperties".GetHashCode()));
					return true;
                }
				float size = moteProps.Size(projectile2.GetDamageAmount(caster, null));
				for (int i = 0; i < moteProps.numTimesThrown; i++)
                {
					float relAngle = Quaternion.LookRotation(CurrentTarget.CenterVector3 - Caster.Position.ToVector3Shifted()).eulerAngles.y;
					SmokeMaker.ThrowMoteDef(moteProps.moteDef, caster.PositionHeld.ToVector3Shifted(), caster.MapHeld, size, moteProps.Velocity, relAngle + moteProps.Angle, moteProps.Rotation);
                }
				return true;
			}
			return false;
		}
	}
}
