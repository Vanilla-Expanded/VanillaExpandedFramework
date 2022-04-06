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
        public CompProperties_Resource Props => props as CompProperties_Resource;

        public virtual PipeNet PipeNet { get; set; }
        public virtual bool TransmitResourceNow => true;
        public Resource Resource => Props.pipeNet.resource;
        public PipeNetManager PipeNetManager { get; private set; }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            RemovePipes();
            PipeNetManager = parent.Map.GetComponent<PipeNetManager>();
            PipeNetManager.RegisterConnector(this);
            PipeSystemDebug.Message($"Registering {this}");
        }

        public override void PostDeSpawn(Map map)
        {
            PipeNetManager.UnregisterConnector(this);
            PipeSystemDebug.Message($"Unregistering {this}");
            base.PostDeSpawn(map);
        }

        /// <summary>
        /// If we find any pipe related to it's resource under it, we remove it.
        /// Prevent multiple grid transmitter at the same position (for exemple building tank on existing pipe).
        /// </summary>
        internal void RemovePipes()
        {
            if (!Props.pipeNet.pipeDefs.Contains(parent.def))
            {
                Map map = parent.Map;
                foreach (var cell in GenAdj.CellsOccupiedBy(parent))
                {
                    var things = cell.GetThingList(map);
                    for (int i = 0; i < things.Count; i++)
                    {
                        var thing = things[i];
                        if (Props.pipeNet.pipeDefs.Contains(thing.def))
                        {
                            thing.Destroy(DestroyMode.Deconstruct);
                        }
                    }
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