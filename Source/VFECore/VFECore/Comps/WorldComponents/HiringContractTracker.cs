using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using VFECore.Misc;

namespace VFECore
{
    using UnityEngine;

    public class HiringContractTracker : WorldComponent, ICommunicable
    {
        public Dictionary<Hireable, List<ExposablePair>>
            deadCount = new Dictionary<Hireable, List<ExposablePair>>(); //the pair being amount of dead people and at what tick it expires

        public int                endTicks;
        public HireableFactionDef factionDef;
        public Hireable           hireable;
        public List<Pawn>         pawns = new List<Pawn>();
        public float              price;

        public HiringContractTracker(World world) : base(world)
        {
        }

        public string GetCallLabel() => "VEF.ContractInfo".Translate((factionDef?.label ?? hireable.Key).CapitalizeFirst());

        public string GetInfoText() => "";

        public void TryOpenComms(Pawn negotiator)
        {
            Find.WindowStack.Add(new Dialog_ContractInfo(this));
        }

        public Faction GetFaction() => null;

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator) => FloatMenuUtility.DecoratePrioritizedTask(
         new FloatMenuOption(GetCallLabel(), () => console.GiveUseCommsJob(negotiator, this), MenuOptionPriority.InitiateSocial), negotiator, console);

        public bool IsHired(Pawn pawn) => this.pawns.Contains(pawn);

        public void SetNewContract(int days, List<Pawn> pawns, Hireable hireable, HireableFactionDef faction = null, float price = 0)
        {
            endTicks      = Find.TickManager.TicksAbs + days * GenDate.TicksPerDay;
            this.pawns    = pawns;
            this.hireable = hireable;
            factionDef    = faction;
            this.price    = price;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (Find.TickManager.TicksAbs % 150 == 0 && Find.TickManager.TicksAbs > endTicks && this.pawns.Any())
                this.EndContract();
        }

        public void EndContract()
        {
            var deadPeople = 0;

            for (int index = pawns.Count - 1; index >= 0; index--)
            {
                Pawn pawn = pawns[index];
                
                if (pawn == null || pawn.Dead || Find.FactionManager.AllFactionsListForReading.Any(f => f.kidnapped.KidnappedPawnsListForReading.Contains(pawn)))
                {
                    deadPeople++;
                    this.pawns.Remove(pawn);
                }
                else if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                {
                    if (pawn.Map != null && pawn.CurJobDef != VFEDefOf.VFEC_LeaveMap)
                    {
                        pawn.jobs.StopAll();
                        if (!CellFinder.TryFindRandomPawnExitCell(pawn, out IntVec3 exit))
                            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !pawn.Map.roofGrid.Roofed(c) && c.WalkableBy(pawn.Map, pawn) &&
                                                                                     pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, canBashDoors: true, canBashFences: true,
                                                                                                   TraverseMode.PassDoors), pawn.Map, 0f, out exit))
                            {
                                this.BreakContract();
                                return;
                            }

                        pawn.jobs.TryTakeOrderedJob(new Job(VFEDefOf.VFEC_LeaveMap, exit));
                    }
                    else if (pawn.GetCaravan() != null)
                    {
                        pawn.GetCaravan().RemovePawn(pawn);
                        this.pawns.Remove(pawn);
                    }
                }

                if (deadPeople > 0)
                {
                    if (!deadCount.ContainsKey(hireable))
                        deadCount.Add(hireable, new List<ExposablePair>());

                    deadCount[hireable].Add(new ExposablePair(deadPeople, Find.TickManager.TicksAbs + GenDate.TicksPerYear));
                }
            }

            if (this.pawns.Count <= 0)
                this.hireable = null;
        }

        public void BreakContract()
        {
            if (this.pawns.Count > 0)
            {
                if (!deadCount.ContainsKey(hireable))
                    deadCount.Add(hireable, new List<ExposablePair>());

                deadCount[hireable].Add(new ExposablePair(this.pawns.Count, Find.TickManager.TicksAbs + GenDate.TicksPerYear));

                foreach (Pawn pawn in this.pawns)
                {
                    if (!pawn.Dead)
                    {
                        if (pawn.Map != null)
                        {
                            pawn.jobs.StopAll();
                            pawn.SetFaction(Faction.OfAncientsHostile);
                            RaidStrategyDefOf.ImmediateAttack.Worker.MakeLords(new IncidentParms() { target = pawn.Map, faction = Faction.OfAncientsHostile, canTimeoutOrFlee = false },
                                                                               new List<Pawn>() { pawn });
                        }
                        else if (pawn.GetCaravan() != null)
                        {
                            pawn.GetCaravan().RemovePawn(pawn);
                        }
                    }
                }
            }

            this.hireable = null;
            this.pawns.Clear();
        }

        public float GetFactorForHireable(Hireable hireable)
        {
            if (!deadCount.ContainsKey(hireable))
                deadCount.Add(hireable, new List<ExposablePair>());

            var pairs = deadCount[hireable];

            float factor = 0;

            for (var i = pairs.Count - 1; i >= 0; i--)
                if (Find.TickManager.TicksAbs > (int)pairs[i].value)
                    pairs.RemoveAt(i);
                else
                    factor += 0.05f * (int)pairs[i].key;

            return factor;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref endTicks, nameof(endTicks));

            Scribe_Collections.Look(ref this.pawns, nameof(this.pawns), LookMode.Reference);

            Scribe_References.Look(ref hireable, nameof(hireable));
            var deadCountKey = new List<Hireable>(deadCount.Keys);
            Scribe_Collections.Look(ref deadCountKey, nameof(deadCountKey), LookMode.Reference);
            var deadCountValue = new List<List<ExposablePair>>(deadCount.Values);
            for (var i = 0; i < deadCountKey.Count; i++)
            {
                var exposablePairs = deadCountValue.Count > i ? deadCountValue[i] : new List<ExposablePair>();
                Scribe_Collections.Look(ref exposablePairs, nameof(exposablePairs) + i, LookMode.Deep);

                if (deadCountValue.Count > i)
                    deadCountValue[i] = exposablePairs;
                else
                    deadCountValue.Add(exposablePairs);
            }


            deadCount.Clear();
            for (var index = 0; index < deadCountKey.Count; index++)
                deadCount.Add(deadCountKey[index], deadCountValue[index]);

            Scribe_Values.Look(ref price, "price");
            Scribe_Defs.Look(ref factionDef, "faction");
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
            Scribe_Values.Look(ref key,   nameof(key));
            Scribe_Values.Look(ref value, nameof(value));
        }
    }
}