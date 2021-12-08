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
            var drop = new LordToil_Drop();
            var travelToDrop = new Transition(travel, drop);
            travelToDrop.AddTrigger(new Trigger_Memo("TravelArrived"));
            travelToDrop.AddTrigger(new Trigger_PawnHarmed());
            var dropToLeave = new Transition(drop, leave);
            dropToLeave.AddTrigger(new Trigger_Memo(LordToil_Drop.DROPPED_MEMO));
            graph.AddToil(drop);
            graph.AddToil(leave);
            graph.AddTransition(travelToDrop);
            graph.AddTransition(dropToLeave);
            return graph;
        }
    }

    public class LordToil_Drop : LordToil
    {
        public const string DROPPED_MEMO = "AllDropped";

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns) pawn.mindState.duty = new PawnDuty(Outposts_DefOf.VEF_DropAllInInventory);
        }

        public override void LordToilTick()
        {
            base.LordToilTick();
            if (lord.ownedPawns.All(pawn => !pawn.inventory.innerContainer.Any())) lord.ReceiveMemo(DROPPED_MEMO);
        }
    }

    public class JobGiver_DropAll : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn?.inventory is null) return null;
            pawn.inventory.UnloadEverything = true;
            pawn.inventory.DropAllNearPawn(pawn.Position, false, true);
            return null;
        }
    }
}