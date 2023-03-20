using Verse;

namespace VFECore
{
    public class Dialog_RenameDoorTeleporter : Dialog_Rename
    {
        public DoorTeleporter DoorTeleporter;

        public Dialog_RenameDoorTeleporter(DoorTeleporter DoorTeleporter)
        {
            this.DoorTeleporter = DoorTeleporter;
            this.curName = DoorTeleporter.Name ?? DoorTeleporter.def.label + " #" + Rand.Range(1, 99).ToString("D2");
        }

        protected override void SetName(string name)
        {
            this.DoorTeleporter.Name = name;
        }
    }
}
