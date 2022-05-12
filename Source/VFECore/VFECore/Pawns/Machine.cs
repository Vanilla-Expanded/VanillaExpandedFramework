using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using VFE.Mechanoids.Needs;

namespace VFEMech
{
    public class Machine : Pawn
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.mindState.lastJobTag = JobTag.Idle;
            this.guest = new Pawn_GuestTracker(this);
            if (this.drafter is null)
            {
                if (this.TryGetComp<CompMachine>().Props.violent)
                {
                    this.drafter = new Pawn_DraftController(this);
                }
            }
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (!this.health.Dead && (!this.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !this.health.capacities.CapableOf(PawnCapacityDefOf.Moving)))
            {
                this.Kill(dinfo);
            }
        }
    }
}
