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
            var pawns = new List<Pawn>();
            foreach (var p in pods.SelectMany(pod => pod.innerContainer).OfType<Pawn>())
            {
                pawns.Add(p);
                Messages.Message("Outposts.AddedFromTransportPods".Translate(p.LabelShortCap, outpost.LabelCap), outpost, MessageTypeDefOf.TaskCompletion);
            }

            foreach (var pawn in pawns) outpost.AddPawn(pawn);
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
                "Outposts.AddTo".Translate(outpost.LabelCap), representative, outpost.Tile, launch =>
                {
                    if (pods.SelectMany(pod => pod.GetDirectlyHeldThings()).Any(t => t is not Pawn))
                        Dialog_MessageBox.CreateConfirmation("Outposts.SendNonPawns".Translate(), launch);
                    else launch();
                });
        }
    }
}