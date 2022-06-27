using RimWorld;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class LordJob_DefendBaseNoEat : LordJob
    {
        private IntVec3 baseCenter;
        private Faction faction;

        public LordJob_DefendBaseNoEat()
        {
        }

        public LordJob_DefendBaseNoEat(Faction faction, IntVec3 baseCenter)
        {
            this.faction = faction;
            this.baseCenter = baseCenter;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            LordToil_DefendBase lordToilDefendBase1 = new LordToil_DefendBase(baseCenter);
            stateGraph.StartingToil = lordToilDefendBase1;

            LordToil_DefendBase lordToilDefendBase2 = new LordToil_DefendBase(baseCenter);
            stateGraph.AddToil(lordToilDefendBase2);

            LordToil_AssaultColony toilAssaultColony = new LordToil_AssaultColony(true)
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(toilAssaultColony);

            Transition transition1 = new Transition(lordToilDefendBase1, lordToilDefendBase2);
            transition1.AddSource(toilAssaultColony);
            transition1.AddTrigger(new Trigger_BecameNonHostileToPlayer());
            stateGraph.AddTransition(transition1);

            Transition transition3 = new Transition(lordToilDefendBase1, toilAssaultColony);
            transition3.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
            transition3.AddTrigger(new Trigger_PawnHarmed(0.4f));
            transition3.AddTrigger(new Trigger_ChanceOnTickInterval(2500, 0.03f));
            transition3.AddTrigger(new Trigger_TicksPassed(251999));
            transition3.AddTrigger(new Trigger_ChanceOnPlayerHarmNPCBuilding(0.4f));
            transition3.AddTrigger(new Trigger_OnClamor(ClamorDefOf.Ability));
            transition3.AddPostAction(new TransitionAction_WakeAll());
            TaggedString taggedString = "MessageDefendersAttacking".Translate(faction.def.pawnsPlural, faction.Name, Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
            transition3.AddPreAction(new TransitionAction_Message(taggedString, MessageTypeDefOf.ThreatBig));
            stateGraph.AddTransition(transition3);

            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref baseCenter, "baseCenter");
        }
    }
}