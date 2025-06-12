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
    public class Machine : Pawn, IRenameable
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

            if (this.Faction == Faction.OfPlayer && this.Name == null)
            {
                this.Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Numeric);
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

        public string MachineName { get; set; }

        public string RenamableLabel
        {
            get
            {
                return MachineName ?? BaseLabel;
            }
            set
            {
                MachineName = value;
            }
        }

        public string BaseLabel => this.def.label;

        public string InspectLabel => RenamableLabel;
    }
}
