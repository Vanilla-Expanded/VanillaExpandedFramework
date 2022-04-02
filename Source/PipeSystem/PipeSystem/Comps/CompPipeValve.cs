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
        private PipeNetManager netManager;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compFlickable = parent.GetComp<CompFlickable>();
            RemovePipes();
            netManager = parent.Map.GetComponent<PipeNetManager>();

            if (TransmitResourceNow)
                netManager.RegisterConnector(this);
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
            if (signal == CompFlickable.FlickedOffSignal)
            {
                // Unregister it, and make our MapMeshFlag dirty at this pos
                netManager.UnregisterConnector(this);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, (MapMeshFlag)455, true, false);
            }
            if (signal == CompFlickable.FlickedOnSignal)
            {
                // Register it, and make our MapMeshFlag dirty at this pos
                netManager.RegisterConnector(this);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, (MapMeshFlag)455, true, false);
            }
            base.ReceiveCompSignal(signal);
        }
    }
}