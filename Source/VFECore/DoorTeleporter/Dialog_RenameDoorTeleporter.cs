using Verse;

namespace VFECore
{
    public class Dialog_RenameDoorTeleporter : Dialog_Rename<DoorTeleporter>
    {
        public DoorTeleporter DoorTeleporter;

        public Dialog_RenameDoorTeleporter(DoorTeleporter doorTeleporter) : base(doorTeleporter)
        {
            this.DoorTeleporter = doorTeleporter;
            this.curName = doorTeleporter.Name ?? doorTeleporter.def.label + " #" + Rand.Range(1, 99).ToString("D2");
        }

    }
}
