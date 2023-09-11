using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore
{
	public class GaussProjectile : ExpandableProjectile
	{

		public HashSet<AltitudeLayer> altitudeLayersBlackList = new HashSet<AltitudeLayer> 
		{
			AltitudeLayer.Item,
			AltitudeLayer.ItemImportant,
			AltitudeLayer.Conduits,
			AltitudeLayer.Floor,
			AltitudeLayer.FloorEmplacement
		};
		public override int DamageAmount
        {
			get
            {
				var baseDamage = def.projectile.GetDamageAmount(weaponDamageMultiplier);
				var damageMultiplier = 1f;
				damageMultiplier += ((float)hitThings.Count / 10f);
				var damageAmount = (int)(baseDamage / damageMultiplier);
				return damageAmount;
			}
        }

		public override void DoDamage(IntVec3 pos)
		{
			if (!stopped)
			{
                base.DoDamage(pos);
                if (pos != this.launcher.Position && this.launcher.Map != null && GenGrid.InBounds(pos, this.launcher.Map))
                {
                    var list = this.launcher.Map.thingGrid.ThingsListAt(pos);
                    for (int num = list.Count - 1; num >= 0; num--)
                    {
                        if (IsDamagable(list[num]) && !altitudeLayersBlackList.Contains(list[num].def.altitudeLayer))
                        {
                            this.customImpact = true;
                            base.Impact(list[num]);
                            this.customImpact = false;
                        }
                    }
                }
            }
        }

        public override bool IsDamagable(Thing t)
        {
            if (t is Pawn pawn)
            {
                if (launcher != null && pawn.Faction != null && launcher.Faction != null 
                    && !pawn.Faction.HostileTo(launcher.Faction))
                {
                    if (Rand.Chance(Find.Storyteller.difficulty.friendlyFireChanceFactor) is false)
                    {
                        return false;
                    }
                }
                if (pawn.GetPosture() != PawnPosture.Standing && this.intendedTarget.Thing != pawn)
                {
                    return false;
                }
            }
            return base.IsDamagable(t);
        }
    }
}
