using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFEMech
{
    public class Machine : Pawn
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.mindState.lastJobTag = JobTag.Idle;
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (!this.health.Dead && (!this.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !this.health.capacities.CapableOf(PawnCapacityDefOf.Moving)))
            {
                this.Kill(dinfo);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            foreach (var comp in this.AllComps)
            {
                foreach (var g in comp.CompGetGizmosExtra())
                {
                    yield return g;
                }
            }
        }
    }
}
