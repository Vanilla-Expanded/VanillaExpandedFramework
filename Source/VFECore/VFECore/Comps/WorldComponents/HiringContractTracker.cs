using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore
{
    using Misc;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine.Networking;
    using Verse;
    using Verse.AI;

    public class HiringContractTracker : WorldComponent
    {
        public int        endTicks;
        public List<Pawn> pawns = new List<Pawn>();
        public Hireable   hireable;

        public Dictionary<Hireable, List<ExposablePair>> deadCount = new Dictionary<Hireable, List<ExposablePair>>(); //the pair being amount of dead people and at what tick it expires

        public HiringContractTracker(World world) : base(world)
        {
        }

        public void SetNewContract(int days, List<Pawn> pawns, Hireable hireable)
        {
            this.endTicks = Find.TickManager.TicksAbs + days * GenDate.TicksPerDay;
            this.pawns    = pawns;
            this.hireable = hireable;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (Find.TickManager.TicksAbs % 150 == 0 && Find.TickManager.TicksAbs > this.endTicks && !this.pawns.NullOrEmpty())
            {
                //Let's send them home

                int deadPeople = 0;

                for (int index = this.pawns.Count - 1; index >= 0; index--)
                {
                    Pawn pawn = this.pawns[index];
                    if (pawn.Dead)
                    {
                        deadPeople++;
                        this.pawns.Remove(pawn);
                    }
                    else if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                    {
                        if (pawn.Map != null && pawn.CurJobDef != VFEDefOf.VFEC_LeaveMap)
                        {
                            pawn.jobs.StopAll();
                            CellFinder.TryFindRandomPawnExitCell(pawn, out IntVec3 exit);
                            pawn.jobs.TryTakeOrderedJob(new Job(VFEDefOf.VFEC_LeaveMap, exit));
                        }
                        else if (pawn.GetCaravan() != null)
                        {
                            pawn.GetCaravan().RemovePawn(pawn);
                            this.pawns.Remove(pawn);
                        }
                    }
                }

                if (deadPeople > 0)
                {
                    if (!this.deadCount.ContainsKey(this.hireable))
                        this.deadCount.Add(this.hireable, new List<ExposablePair>());

                    this.deadCount[this.hireable].Add(new ExposablePair(deadPeople, Find.TickManager.TicksAbs + GenDate.TicksPerYear));
                }

                this.hireable = null;
            }
        }

        public float GetFactorForHireable(Hireable hireable)
        {
            if (!this.deadCount.ContainsKey(hireable))
                this.deadCount.Add(hireable, new List<ExposablePair>());

            List<ExposablePair> pairs = this.deadCount[hireable];

            float factor = 0;

            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                if (Find.TickManager.TicksAbs > (int)pairs[i].value)
                    pairs.RemoveAt(i);
                else
                    factor += 0.05f * (int)pairs[i].key;
            }

            return factor;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref this.endTicks, nameof(this.endTicks));
            Scribe_Collections.Look(ref this.pawns, nameof(this.pawns), LookMode.Reference);
            Scribe_References.Look(ref this.hireable, nameof(this.hireable));
            List<Hireable>            deadCountKey   = new List<Hireable>();
            List<List<ExposablePair>> deadCountValue = new List<List<ExposablePair>>();
            Scribe_Collections.Look(ref this.deadCount, nameof(this.deadCount), LookMode.Reference, LookMode.Deep, ref deadCountKey, ref deadCountValue);
        }
    }

    public class ExposablePair : IExposable
    {
        public object key;
        public object value;


        public ExposablePair(object key, object value)
        {
            this.key   = key;
            this.value = value;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.key,   nameof(this.key));
            Scribe_Values.Look(ref this.value, nameof(this.value));
        }
    }
}