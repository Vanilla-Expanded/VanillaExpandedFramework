using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace VFECore
{

    public class LordJob_SiegeCustom : LordJob
    {
        public LordJob_SiegeCustom() { }
        public LordJob_SiegeCustom(Faction faction, IntVec3 siegeSpot, float blueprintPoints)
        {
            this.faction = faction;
            this.siegeSpot = siegeSpot;
            this.blueprintPoints = blueprintPoints;
        }

        public override bool GuiltyOnDowned => true;

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(this.siegeSpot).CreateGraph()).StartingToil;
            var lordToil_Siege = new LordToil_SiegeCustom(this.siegeSpot, this.blueprintPoints);
            stateGraph.AddToil(lordToil_Siege);
            LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_AssaultColony(this.faction, true, true, false, false, true).CreateGraph()).StartingToil;
            Transition transition = new Transition(startingToil, lordToil_Siege, false, true);
            transition.AddTrigger(new Trigger_Memo("TravelArrived"));
            transition.AddTrigger(new Trigger_TicksPassed(5000));
            stateGraph.AddTransition(transition, false);
            Transition transition2 = new Transition(lordToil_Siege, startingToil2, false, true);
            transition2.AddTrigger(new Trigger_Memo("NoBuilders"));
            transition2.AddTrigger(new Trigger_Memo("NoArtillery"));
            transition2.AddTrigger(new Trigger_PawnHarmed(0.08f, false, null));
            transition2.AddTrigger(new Trigger_FractionPawnsLost(0.3f));
            transition2.AddTrigger(new Trigger_TicksPassed((int)(60000f * Rand.Range(1.5f, 3f))));
            transition2.AddPreAction(new TransitionAction_Message("MessageSiegersAssaulting".Translate(this.faction.def.pawnsPlural, this.faction), MessageTypeDefOf.ThreatBig, null, 1f));
            transition2.AddPostAction(new TransitionAction_WakeAll());
            stateGraph.AddTransition(transition2, false);
            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Faction>(ref this.faction, "faction", false);
            Scribe_Values.Look<IntVec3>(ref this.siegeSpot, "siegeSpot", default(IntVec3), false);
            Scribe_Values.Look<float>(ref this.blueprintPoints, "blueprintPoints", 0f, false);
        }

        private Faction faction;

        private IntVec3 siegeSpot;

        private float blueprintPoints;
    }

}
