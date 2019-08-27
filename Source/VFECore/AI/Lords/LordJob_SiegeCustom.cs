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

        public LordJob_SiegeCustom(Faction faction, IntVec3 siegeSpot, float blueprintPoints)
        {
            this.faction = faction;
            this.siegeSpot = siegeSpot;
            this.blueprintPoints = blueprintPoints;
        }

        public override bool GuiltyOnDowned => true;

        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();

            // Travel to siege point
            var travelToil = stateGraph.AttachSubgraph(new LordJob_Travel(siegeSpot).CreateGraph()).StartingToil;
            var lordToil_Siege = new LordToil_SiegeCustom(siegeSpot, blueprintPoints);
            stateGraph.AddToil(lordToil_Siege);

            // Besiege colony
            var travelToSiegeTransition = new Transition(travelToil, lordToil_Siege, false, true);
            travelToSiegeTransition.AddTrigger(new Trigger_Memo("TravelArrived"));
            travelToSiegeTransition.AddTrigger(new Trigger_TicksPassed(5000));
            stateGraph.AddTransition(travelToSiegeTransition, false);

            // Assault colony
            var assaultToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
            var siegeToAssaultTransition = new Transition(lordToil_Siege, assaultToil, false, true);
            siegeToAssaultTransition.AddTrigger(new Trigger_Memo("NoBuilders"));
            siegeToAssaultTransition.AddTrigger(new Trigger_Memo("NoArtillery"));
            siegeToAssaultTransition.AddTrigger(new Trigger_PawnHarmed(0.08f, false, null));
            siegeToAssaultTransition.AddTrigger(new Trigger_FractionPawnsLost(0.3f));
            siegeToAssaultTransition.AddTrigger(new Trigger_TicksPassed((int)(60000f * Rand.Range(1.5f, 3f))));
            siegeToAssaultTransition.AddPreAction(new TransitionAction_Message("MessageSiegersAssaulting".Translate(faction.def.pawnsPlural, faction), MessageTypeDefOf.ThreatBig, null, 1f));
            siegeToAssaultTransition.AddPostAction(new TransitionAction_WakeAll());
            stateGraph.AddTransition(siegeToAssaultTransition, false);

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
