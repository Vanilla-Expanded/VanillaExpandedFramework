using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class Machine : Pawn
    {
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (!this.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !this.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                this.Kill(dinfo);
            }
        }
    }
}
