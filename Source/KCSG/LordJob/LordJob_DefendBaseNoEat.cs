using System;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    public class LordJob_DefendBaseNoEat : LordJob
    {
        private IntVec3 baseCenter;
        private Faction faction;
        private int delayBeforeAssault;
        private bool attackWhenPlayerBecameEnemy;

        public LordJob_DefendBaseNoEat()
        {
        }

        [Obsolete("Will be removed in the future, use the other constructor. This constructor is kept for now for compatibility reasons.")]
        public LordJob_DefendBaseNoEat(Faction faction, IntVec3 baseCenter) : this(faction, baseCenter, 180000)
        {
        }

        public LordJob_DefendBaseNoEat(Faction faction, IntVec3 baseCenter, int delayBeforeAssault, bool attackWhenPlayerBecameEnemy = false)
        {
            this.faction = faction;
            this.baseCenter = baseCenter;
            this.delayBeforeAssault = delayBeforeAssault;
            this.attackWhenPlayerBecameEnemy = attackWhenPlayerBecameEnemy;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            // Hostiles defending base (can assault the player)
            LordToil_DefendBase lordToilDefendBase1 = new LordToil_DefendBase(baseCenter);
            stateGraph.StartingToil = lordToilDefendBase1;

            // Non-hostiles defending their base (can't assault the player)
            LordToil_DefendBase lordToilDefendBase2 = new LordToil_DefendBase(baseCenter);
            stateGraph.AddToil(lordToilDefendBase2);

            // Assault the player after delay or RNG-based
            LordToil_AssaultColony toilAssaultColony = new LordToil_AssaultColony(true)
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(toilAssaultColony);

            // If faction stops being hostile, transition to the non-hostile defence so they can't assault the player
            Transition transition1 = new Transition(lordToilDefendBase1, lordToilDefendBase2);
            transition1.AddSource(toilAssaultColony);
            transition1.AddTrigger(new Trigger_BecameNonHostileToPlayer());
            stateGraph.AddTransition(transition1);

            // If faction becomes hostile, either transition to hostile defence so they can assault or assault immediately (depends on attackWhenPlayerBecameEnemy)
            Transition transition2 = new Transition(lordToilDefendBase2, this.attackWhenPlayerBecameEnemy ? (LordToil)toilAssaultColony : lordToilDefendBase1, false, true);
            if (this.attackWhenPlayerBecameEnemy)
            {
                transition2.AddSource(lordToilDefendBase1);
            }
            transition2.AddTrigger(new Trigger_BecamePlayerEnemy());
            stateGraph.AddTransition(transition2, false);

            // Transition from hostile defence to assaulting the player, based on specific triggers.
            Transition transition3 = new Transition(lordToilDefendBase1, toilAssaultColony);
            transition3.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
            transition3.AddTrigger(new Trigger_PawnHarmed(0.4f));
            transition3.AddTrigger(new Trigger_ChanceOnTickInterval(2500, 0.03f));
            transition3.AddTrigger(new Trigger_TicksPassed(delayBeforeAssault));
            transition3.AddTrigger(new Trigger_ChanceOnPlayerHarmNPCBuilding(0.4f));
            transition3.AddTrigger(new Trigger_OnClamor(ClamorDefOf.Ability));
            transition3.AddPostAction(new TransitionAction_WakeAll());
            TaggedString taggedString = this.faction.def.messageDefendersAttacking.Formatted(this.faction.def.pawnsPlural, this.faction.Name, Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
            transition3.AddPreAction(new TransitionAction_Message(taggedString, MessageTypeDefOf.ThreatBig));
            stateGraph.AddTransition(transition3);

            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref baseCenter, "baseCenter");
            Scribe_Values.Look<bool>(ref this.attackWhenPlayerBecameEnemy, "attackWhenPlayerBecameEnemy", false);
            Scribe_Values.Look<int>(ref this.delayBeforeAssault, "delayBeforeAssault", 25000);
        }
    }
}