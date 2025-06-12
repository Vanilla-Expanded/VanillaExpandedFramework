using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Outposts
{
    public class TransportPodsArrivalAction_AddToOutpost : TransportersArrivalAction
    {
        private Outpost outpost;

        public override bool GeneratesMap => true;

        public TransportPodsArrivalAction_AddToOutpost()
        {
        }

        public TransportPodsArrivalAction_AddToOutpost(Outpost addTo) => outpost = addTo;

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            var things = new List<Thing>();
            foreach (var t in transporters.SelectMany(pod => pod.innerContainer).OfType<Thing>())
            {
                things.Add(t);
                if(t is Pawn){
                	Messages.Message("Outposts.AddedFromTransportPods".Translate(t.LabelShortCap, outpost.LabelCap), outpost, MessageTypeDefOf.TaskCompletion);
            	}
            }

            foreach (var t in things){
            	if(t is Pawn) outpost.AddPawn(t as Pawn);
            	else outpost.AddItem(t);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref outpost, "outpost");
        }

        public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile) => outpost.Tile == destinationTile;

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(IEnumerable<IThingHolder> pods,Action<PlanetTile, TransportersArrivalAction> launchAction, Outpost outpost)
        {
            return TransportersArrivalActionUtility.GetFloatMenuOptions(
                () => true, () => new TransportPodsArrivalAction_AddToOutpost(outpost),
                "Outposts.AddTo".Translate(outpost.LabelCap), launchAction, outpost.Tile, launch =>{launch();});
        }




    }
}
