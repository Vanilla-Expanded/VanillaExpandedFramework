using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HeavyWeapons
{
	public class Verb_Shoot : Verb_LaunchProjectile
	{
		protected override int ShotsPerBurst => verbProps.burstShotCount;
		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Pawn pawn = currentTarget.Thing as Pawn;
			if (pawn != null && !pawn.Downed && CasterIsPawn && CasterPawn.skills != null)
			{
				float num = pawn.HostileTo(caster) ? 170f : 20f;
				float num2 = verbProps.AdjustedFullCycleTime(this, CasterPawn);
				CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
			}
		}

		protected override bool TryCastShot()
		{
			bool num = base.TryCastShot();
			if (num && CasterIsPawn)
			{
				CasterPawn.records.Increment(RecordDefOf.ShotsFired);
			}
			if (num && this.EquipmentSource.def.HasModExtension<HeavyWeapon>())
            {
				var options = this.EquipmentSource.def.GetModExtension<HeavyWeapon>();
				if (options.weaponHitPointsDeductionOnShot > 0)
                {
					this.EquipmentSource.HitPoints -= options.weaponHitPointsDeductionOnShot;
					if (this.EquipmentSource.HitPoints <= 0)
                    {
						this.EquipmentSource.Destroy();
						if (CasterIsPawn)
						{
                            CasterPawn.jobs.StopAll();
                        }
                    }
                }
			}
			return num;
		}
	}
}
