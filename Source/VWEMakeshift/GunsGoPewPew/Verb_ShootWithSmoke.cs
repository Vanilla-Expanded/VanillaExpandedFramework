using UnityEngine;
using Verse;

namespace VWEMakeshift
{
  public class Verb_ShootWithSmoke : Verb_Shoot
  {
    protected override bool TryCastShot()
    {
      if (base.TryCastShot())
      {
        var projectile   = this.GetProjectile();
        var projectile2  = projectile.projectile;
        var equipmentDef = EquipmentSource?.def;
        if (equipmentDef is null)
        {
          Log.Error($"Unable to retrieve weapon def from <color=teal>{GetType()}</color>. Please report to Oskar or Smash Phil.");
          return true;
        }

        var moteProps = equipmentDef.GetModExtension<MoteProperties>();
        if (moteProps is null)
        {
          Log.ErrorOnce($"<color=teal>{GetType()}</color> cannot be used without <color=teal>MoteProperties</color> DefModExtension. Motes will not be thrown.", Gen.HashCombine(projectile.GetHashCode(), "MoteProperties".GetHashCode()));
          return true;
        }

        var size = moteProps.Size(projectile2.GetDamageAmount(caster));
        for (var i = 0; i < moteProps.numTimesThrown; i++)
        {
          var relAngle = Quaternion.LookRotation(CurrentTarget.CenterVector3 - Caster.Position.ToVector3Shifted()).eulerAngles.y;
          if (moteProps.moteDef != null)
            SmokeMaker.ThrowMoteDef(moteProps.moteDef, caster.PositionHeld.ToVector3Shifted(), caster.MapHeld, size, moteProps.Velocity, relAngle + moteProps.Angle, moteProps.Rotation);
          if (moteProps.fleckDef != null)
            SmokeMaker.ThrowFleckDef(moteProps.fleckDef, caster.PositionHeld.ToVector3Shifted(), caster.MapHeld, size, moteProps.Velocity, relAngle + moteProps.Angle, moteProps.Rotation);
        }

        return true;
      }

      return false;
    }
  }
}
