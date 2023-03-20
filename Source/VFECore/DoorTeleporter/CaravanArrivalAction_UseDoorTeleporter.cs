using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VFECore
{
    public class CaravanArrivalAction_UseDoorTeleporter : CaravanArrivalAction
    {
        public DoorTeleporter Target;
        public DoorTeleporter Use;
        public CaravanArrivalAction_UseDoorTeleporter(DoorTeleporter origin, DoorTeleporter dest)
        {
            this.Use = origin;
            this.Target = dest;
        }

        public override string Label => "VEF.TeleportTo".Translate(this.Target.Name);
        public override string ReportString =>
            JobUtility.GetResolvedJobReportRaw(VFEDefOf.VEF_UseDoorTeleporter.reportString, this.Use.Name, this.Use, this.Target.Name, this.Target, null, null);

        public override void Arrived(Caravan caravan)
        {
            caravan.Tile = this.Target.Map.Tile;
            caravan.Notify_Teleported();
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile) =>
            this.Target is { Spawned: true } && this.Use is { Spawned: true };

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, DoorTeleporter origin, DoorTeleporter dest)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(
                () => true, () => new CaravanArrivalAction_UseDoorTeleporter(origin, dest),
                "VEF.TeleportTo".Translate(dest.Name), caravan, origin.Map.Tile, origin.Map.Parent);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.Target, "target");
            Scribe_References.Look(ref this.Use, "use");
        }
    }
}
