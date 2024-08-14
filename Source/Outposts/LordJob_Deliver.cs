using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Outposts
{
    public class LordJob_Deliver : LordJob
    {
        private IntVec3 deliverLoc;

        public LordJob_Deliver()
        {
        }

        public LordJob_Deliver(IntVec3 deliverLoc) => this.deliverLoc = deliverLoc;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref deliverLoc, "deliverLoc");
        }

        public override StateGraph CreateGraph()
        {
            var graph = new StateGraph();
            var travel = new LordToil_Travel(deliverLoc) {maxDanger = Danger.Deadly, useAvoidGrid = true};
            graph.StartingToil = travel;
            var leave = new LordToil_ExitMap(canDig: true);
            graph.AddToil(leave);
            var drop = new LordToil_Drop();
            graph.AddToil(drop);
            var travelToDrop = new Transition(travel, drop);
            travelToDrop.AddTrigger(new Trigger_Memo("TravelArrived"));
            travelToDrop.AddTrigger(new Trigger_PawnHarmed());
            graph.AddTransition(travelToDrop);
            var dropToLeave = new Transition(drop, leave);
            dropToLeave.AddTrigger(new Trigger_Memo(LordToil_Drop.DROPPED_MEMO));
            graph.AddTransition(dropToLeave);
            var gotoDropLoc = new LordToil_GotoDropLoc();
            graph.AddToil(gotoDropLoc);
            var newDropLoc = new Transition(drop, gotoDropLoc);
            newDropLoc.AddTrigger(new Trigger_Memo(LordToil_Drop.DROPPED_MEMO));
            graph.AddTransition(newDropLoc);
            var atDropLoc = new Transition(gotoDropLoc, drop);
            atDropLoc.AddTrigger(new Trigger_Memo("TravelArrived"));
            graph.AddTransition(atDropLoc);
            return graph;
        }
    }

    public class LordToil_Drop : LordToil
    {
        public const string DROPPED_MEMO = "AllDropped";
        public const string AREAFULL_MEMO = "AreaFull";

        public LordToil_Drop() => data = new LordToilData_Drop {TicksPassed = 0};

        public LordToilData_Drop Data => data as LordToilData_Drop;

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns) pawn.mindState.duty = new PawnDuty(Outposts_DefOf.VEF_DropAllInInventory);
            Data.TicksPassed = 0;
        }

        public override void LordToilTick()
        {
            base.LordToilTick();
            if (lord.ownedPawns.All(pawn => !pawn.inventory.innerContainer.Any())) lord.ReceiveMemo(DROPPED_MEMO);
            Data.TicksPassed++;
            if (Data.TicksPassed > 60) lord.ReceiveMemo(AREAFULL_MEMO);
        }

        public class LordToilData_Drop : LordToilData
        {
            public int TicksPassed;

            public override void ExposeData()
            {
                Scribe_Values.Look(ref TicksPassed, "ticksPassed");
            }
        }
    }

    public class LordToil_GotoDropLoc : LordToil_Travel
    {
        public LordToil_GotoDropLoc() : base(IntVec3.Zero)
        {
        }

        public override void UpdateAllDuties()
        {
            SetDestination(FindDropSpot(lord.ownedPawns.First()));
            base.UpdateAllDuties();
        }

        private IntVec3 FindDropSpot(Pawn pawn)
        {
            if (CellFinder.TryFindRandomReachableCellNearPosition(pawn.Position, pawn.Position, pawn.Map, 12.9f * 2f, TraverseParms.For(pawn),
                x => x.Walkable(pawn.Map) &&
                     GenRadial.RadialCellsAround(x, 12.9f, true).Count(c =>
                         c.Walkable(pawn.Map) && !c.GetThingList(pawn.Map).Any(t => t.def.saveCompressible || t.def.category == ThingCategory.Item)) >=
                     GenRadial.NumCellsInRadius(12.9f) / 2, _ => true, out var dropLoc))
                return dropLoc;
            return CellFinder.RandomCell(pawn.Map);
        }
    }

    public class JobGiver_DropAll : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn?.inventory is null) return null;
            pawn.inventory.UnloadEverything = true;
            pawn.inventory.DropAllNearPawn(pawn.Position, false, true);
            return null;
        }
    }
}