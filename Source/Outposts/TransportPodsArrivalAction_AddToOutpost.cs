using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Outposts
{
    public class TransportPodsArrivalAction_AddToOutpost : TransportPodsArrivalAction
    {
        private Outpost outpost;

        public TransportPodsArrivalAction_AddToOutpost()
        {
        }

        public TransportPodsArrivalAction_AddToOutpost(Outpost addTo) => outpost = addTo;

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            var things = new List<Thing>();
            foreach (var t in pods.SelectMany(pod => pod.innerContainer).OfType<Thing>())
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

        public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile) => outpost.Tile == destinationTile;

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, Outpost outpost)
        {
            return TransportPodsArrivalActionUtility.GetFloatMenuOptions(
                () => true, () => new TransportPodsArrivalAction_AddToOutpost(outpost),
                "Outposts.AddTo".Translate(outpost.LabelCap), representative, outpost.Tile, launch =>{launch();});
        }
    }
}
