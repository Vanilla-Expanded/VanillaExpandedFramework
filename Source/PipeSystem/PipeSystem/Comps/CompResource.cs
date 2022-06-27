using RimWorld;
using System.Text;
using Verse;
using Verse.Sound;

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
        public PipeNetManager PipeNetManager { get; internal set; }

        public Sustainer sustainer;

        private Graphic_LinkedOverlayPipe graphicLinkedOverlay;

        /// <summary>
        /// Remove under pipes. Get and set manager. Start sustainer.
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            RemovePipes();
            PipeNetManager = parent.Map.GetComponent<PipeNetManager>();
            PipeNetManager.RegisterConnector(this);
            PipeSystemDebug.Message($"Registering {this}");

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                StartSustainer();
            });

            graphicLinkedOverlay = LinkedPipes.GetOverlayFor(PipeNet.def);
        }

        /// <summary>
        /// Unregister comp
        /// </summary>
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

        /// <summary>
        /// Inspect infos
        /// </summary>
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (PipeNet == null)
            {
                sb.Append("PipeSystem_NotConnected".Translate(Resource.name));
                return sb.ToString().Trim();
            }

            if (!PipeNet.def.resource.onlyShowStored)
            {
                sb.Append($"{"PipeSystem_ExcessStored".Translate(Resource.name)} {((PipeNet.Production - PipeNet.Consumption) / 100) * GenDate.TicksPerDay:##0} {Resource.unit}/d ({PipeNet.Stored:##0} {Resource.unit})");
            }
            else
            {
                sb.Append($"{"PipeSystem_Stored".Translate(Resource.name)} {PipeNet.Stored:##0} {Resource.unit}");
            }
            sb.AppendInNewLine(base.CompInspectStringExtra());

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Try spawn sustainer
        /// </summary>
        public void StartSustainer()
        {
            if (!Props.soundAmbient.NullOrUndefined() && sustainer == null)
            {
                SoundInfo info = SoundInfo.InMap(parent);
                sustainer = Props.soundAmbient.TrySpawnSustainer(info);
            }
        }

        /// <summary>
        /// End and remove sustainer
        /// </summary>
        public void EndSustainer()
        {
            if (sustainer != null)
            {
                sustainer.End();
                sustainer = null;
            }
        }

        /// <summary>
        /// If have an ambient sound, and should be active now, maintain it
        /// If it is null or ended, recreate it.
        /// If it shouldn't make sound, end it and null it.
        /// </summary>
        /// <param name="check">Should make sound?</param>
        public void UpdateSustainer(bool check)
        {
            if (Props.soundAmbient == null)
            {
                return;
            }
            if (check)
            {
                if (sustainer == null || sustainer.Ended)
                {
                    sustainer = Props.soundAmbient.TrySpawnSustainer(SoundInfo.InMap(parent));
                }
                sustainer.Maintain();
            }
            else if (sustainer != null)
            {
                sustainer.End();
                sustainer = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CompPrintForResourceGrid(SectionLayer layer)
        {
            if (TransmitResourceNow)
            {
                graphicLinkedOverlay.Print(layer, parent, 0);
            }
        }
    }
}