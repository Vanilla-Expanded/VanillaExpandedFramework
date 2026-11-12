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

        public new CompProperties_PipeValve Props => (CompProperties_PipeValve)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            PipeNetManager.RegisterValve(this);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();

            InitializeComps();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            PipeNetManager.UnregisterValve(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitializeComps();
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
                var valveOpenedKey = Props.valveOpenedKey ?? Props.pipeNet.keyOverrides?.valveOpenedKey;
                if (valveOpenedKey.NullOrEmpty())
                    return $"{"PipeSystem_ValveOpened".Translate()}\n{base.CompInspectStringExtra()}".Trim();
                return valveOpenedKey.Translate([.. InspectStringArguments()]);
            }

            var valveClosedKey = Props.valveClosedKey ?? Props.pipeNet.keyOverrides?.valveClosedKey;
            if (valveClosedKey.NullOrEmpty())
                return "PipeSystem_ValveClosed".Translate();
            return valveClosedKey.Translate([.. InspectStringArguments()]);
        }

        public override void ReceiveCompSignal(string signal)
        {
            var map = parent.Map;
            var root = parent.Position;

            if (signal == CompFlickable.FlickedOffSignal)
            {
                // Unregister it, and make our MapMeshFlag dirty at this pos
                for (var i = 0; i < parent.AllComps.Count; i++)
                {
                    if (parent.AllComps[i] is CompResource comp && comp.Props.pipeNet == Props.pipeNet)
                        PipeNetManager.UnregisterConnector(comp);
                }
                map.mapDrawer.MapMeshDirty(root, MapMeshFlagDefOf.Things, true, false);
                map.mapDrawer.MapMeshDirty(root, MapMeshFlagDefOf.PowerGrid, true, false);
            }
            if (signal == CompFlickable.FlickedOnSignal)
            {
                // Register it, and make our MapMeshFlag dirty at this pos
                for (var i = 0; i < parent.AllComps.Count; i++)
                {
                    if (parent.AllComps[i] is CompResource comp && comp.Props.pipeNet == Props.pipeNet)
                        PipeNetManager.RegisterConnector(comp);
                }
                map.mapDrawer.MapMeshDirty(root, MapMeshFlagDefOf.Things, true, false);
                map.mapDrawer.MapMeshDirty(root, MapMeshFlagDefOf.PowerGrid, true, false);
            }
            base.ReceiveCompSignal(signal);
        }

        /// <summary>
        /// Get comps on PostPostMake and ExposeData (loading vars) only, matching vanilla behavior.
        /// </summary>
        private void InitializeComps()
        {
            compFlickable = parent.TryGetComp<CompFlickable>();
        }
    }
}