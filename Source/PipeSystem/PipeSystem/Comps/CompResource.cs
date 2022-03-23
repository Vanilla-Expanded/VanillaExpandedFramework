using RimWorld;
using System.Text;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Most basic comp, used as base or with thing that use CompProperties_Resource.
    /// </summary>
    public class CompResource : ThingComp
    {
        public virtual PipeNet PipeNet { get; set; }
        public virtual bool TransmitResourceNow => true;

        public CompProperties_Resource Props => props as CompProperties_Resource;
        public Resource Resource => Props.pipeNet.resource;

        public PipeNetManager pipeNetManager;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            RemovePipes();
            pipeNetManager = parent.Map.GetComponent<PipeNetManager>();
            pipeNetManager.RegisterConnector(this);
        }

        public override void PostDeSpawn(Map map)
        {
            PipeSystemDebug.Message($"Unregistering {parent.ThingID}");
            pipeNetManager.UnregisterConnector(this);
            base.PostDeSpawn(map);
        }

        /// <summary>
        /// If we find any pipe related to it's resource under it, we remove it.
        /// Prevent multiple grid transmitter at the same position (for exemple building tank on existing pipe).
        /// </summary>
        internal void RemovePipes()
        {
            if (parent.def != Props.pipeNet.pipeDef)
            {
                foreach (var cell in GenAdj.CellsOccupiedBy(parent))
                {
                    cell.GetFirstThing(parent.Map, Props.pipeNet.pipeDef)?.Destroy(DestroyMode.Deconstruct);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (PipeNet == null)
            {
                sb.Append("PipeSystem_NotConnected".Translate(Resource.name));
                return sb.ToString().Trim();
            }

            sb.Append($"{"PipeSystem_ExcessStored".Translate(Resource.name)} {((PipeNet.Production - PipeNet.Consumption) / 100) * GenDate.TicksPerDay:##0} {Resource.unit}/d ({PipeNet.Stored:##0} {Resource.unit})");
            sb.AppendInNewLine(base.CompInspectStringExtra());

            return sb.ToString().Trim();
        }
    }
}