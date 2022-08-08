using RimWorld;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Unregister/register "pipe" (valve) based on CompFlickable
    /// </summary>
    public class CompPipeValve : CompResource
    {
        private CompFlickable compFlickable;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compFlickable = parent.GetComp<CompFlickable>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override bool TransmitResourceNow
        {
            get
            {
                return compFlickable.SwitchIsOn;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (TransmitResourceNow)
            {
                return $"{"PipeSystem_ValveOpened".Translate()}\n{base.CompInspectStringExtra()}".Trim();
            }
            return "PipeSystem_ValveClosed".Translate();
        }

        public override void ReceiveCompSignal(string signal)
        {
            var map = parent.Map;
            var root = parent.Position;

            if (signal == CompFlickable.FlickedOffSignal)
            {
                // Unregister it, and make our MapMeshFlag dirty at this pos
                PipeNetManager.UnregisterConnector(this);
                map.mapDrawer.MapMeshDirty(root, MapMeshFlag.Things, true, false);
                map.mapDrawer.MapMeshDirty(root, (MapMeshFlag)455, true, false);
            }
            if (signal == CompFlickable.FlickedOnSignal)
            {
                // Register it, and make our MapMeshFlag dirty at this pos
                PipeNetManager.RegisterConnector(this);
                map.mapDrawer.MapMeshDirty(root, MapMeshFlag.Things, true, false);
                map.mapDrawer.MapMeshDirty(root, (MapMeshFlag)455, true, false);
            }
            base.ReceiveCompSignal(signal);
        }
    }
}