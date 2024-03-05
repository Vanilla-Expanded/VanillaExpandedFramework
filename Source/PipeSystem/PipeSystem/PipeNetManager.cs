using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
    public class PipeNetManager : MapComponent
    {
        // Cache wanting refill for performance
        public HashSet<Thing> wantRefill = new HashSet<Thing>();
        // All map nets
        public List<PipeNet> pipeNets = new List<PipeNet>();
        // To avoid getting List Count
        private int pipeNetsCount = 0;
        // Valve that link even when off
        public BoolGrid valveGrid;
        // Net needing the no storage alerts
        public List<PipeNet> noStorage = new List<PipeNet>();

        public PipeNetManager(Map map) : base(map)
        {
            valveGrid = new BoolGrid(map);
        }

        /// <summary>
        /// We draw resources grid if wanted.
        /// </summary>
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (PipeSystemDebug.drawResourcesNetGrid && Current.ProgramState == ProgramState.Playing && Find.CurrentMap == map)
            {
                int num = 0;
                foreach (var net in pipeNets)
                {
                    foreach (var cell in net.networkGrid.ActiveCells)
                    {
                        CellRenderer.RenderCell(cell, num * 0.44f);
                    }
                    ++num;
                }
            }
        }

        /// <summary>
        /// Ticking all net from here.
        /// </summary>
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            for (int i = 0; i < pipeNetsCount; i++)
            {
                var net = pipeNets[i];
                var tick = Find.TickManager.TicksGame;

                if (net.NextTick <= tick)
                {
                    net.PipeSystemTick();
                    net.NextTick = tick + 100;
                }
            }
        }

        /// <summary>
        /// Get all the things adjacent cardinal to the given thing.
        /// </summary>
        /// <param name="thing"></param>
        /// <returns>List of things adjacent</returns>
        private List<Thing> NeighbourThingsCardinal(Thing thing)
        {
            List<Thing> things = new List<Thing>();
            var adjCells = GenAdjFast.AdjacentCellsCardinal(thing);
            for (int i = 0; i < adjCells.Count; i++)
            {
                var adj = adjCells[i];
                if (adj.InBounds(map))
                {
                    var thingsAt = map.thingGrid.ThingsListAt(adj);
                    for (int o = 0; o < thingsAt.Count; o++)
                    {
                        var t = thingsAt[o];
                        if (!things.Contains(t))
                            things.Add(t);
                    }
                }
            }

            return things;
        }

        /// <summary>
        /// Register a comp. If no adjacent PipeNet are found, a new one is created.
        /// Else it's added to an existing one, and merging PipeNet if necessary.
        /// </summary>
        /// <param name="comp"></param>
        public void RegisterConnector(CompResource comp)
        {
            // Get all thing around
            List<Thing> things = NeighbourThingsCardinal(comp.parent);
            // Loop through all of them, trying to find PipeNet(s) of the right resource
            List<PipeNet> foundNets = new List<PipeNet>();
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is ThingWithComps thing)
                {
                    var comps = thing.GetComps<CompResource>();
                    foreach (var compR in comps)
                    {
                        if (compR != null
                            && compR.TransmitResourceNow
                            && compR.Props.pipeNet == comp.Props.pipeNet
                            && compR.PipeNet is PipeNet p
                            && p != null
                            && !foundNets.Contains(p))
                        {
                            foundNets.Add(p);
                        }
                    }
                }
            }

            // If no PipeNet are adjacent, we create a new one
            if (foundNets.Count == 0)
            {
                pipeNets.Add(PipeNetMaker.MakePipeNet(new List<CompResource> { comp }, map, comp.Props.pipeNet));
                pipeNetsCount++;
                PipeSystemDebug.Message($"Creating new net");
            }
            // We found only one, register the comp to it
            else if (foundNets.Count == 1)
            {
                foundNets[0].RegisterComp(comp);
                PipeSystemDebug.Message($"Adding comp to existing net");
            }
            // We found multiple, we keep the first add the comp to it and merge the others
            else if (foundNets.Count > 1)
            {
                foundNets[0].RegisterComp(comp);
                for (int i = 0; i < foundNets.Count - 1; i++)
                {
                    foundNets[0].Merge(foundNets[i + 1]);
                }
                PipeSystemDebug.Message($"Merged {foundNets.Count} networks");
                pipeNetsCount = pipeNets.Count;
            }
            PipeSystemDebug.Message($"Network(s) number: {pipeNets.Count}");
        }

        /// <summary>
        /// Unregister a comp. We remove it from it's net, and recreate PipeNet(s) if needed.
        /// </summary>
        /// <param name="comp"></param>
        public void UnregisterConnector(CompResource comp)
        {
            // Get all thing around
            List<Thing> things = NeighbourThingsCardinal(comp.parent);
            // Loop through all of them, trying to find PipeNet(s) of the right resource
            List<CompResource> foundConnectors = new List<CompResource>();
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is ThingWithComps thing)
                {
                    var comps = thing.GetComps<CompResource>();
                    foreach (var compR in comps)
                    {
                        if (compR != null
                            && compR.TransmitResourceNow
                            && compR.Props.pipeNet == comp.Props.pipeNet
                            && !foundConnectors.Contains(compR))
                        {
                            foundConnectors.Add(compR);
                        }
                    }
                }
            }

            // Destroy the PipeNet
            comp.PipeNet.Destroy();
            pipeNetsCount--;
            // Recreate PipeNet(s) based on neigbours connectors
            CreatePipeSystemNets(foundConnectors, comp.Props.pipeNet);

            PipeSystemDebug.Message($"Network(s) number: {pipeNets.Count}");
        }

        /// <summary>
        /// Create PipeNet(s) from a list of connectors
        /// </summary>
        /// <param name="connectors"></param>
        /// <param name="pipeNet"></param>
        public void CreatePipeSystemNets(List<CompResource> connectors, PipeNetDef pipeNet)
        {
            while (connectors.Any())
            {
                pipeNets.Add(CreatePipeNetFrom(connectors.First(), pipeNet, out var treated));
                pipeNetsCount++;
                // remove things we've seen from the list
                foreach (var connector in treated)
                    connectors.Remove(connector);
            }
        }

        /// <summary>
        /// Create PipeNet from a root connector
        /// </summary>
        /// <param name="rootComp">Root resouce comp</param>
        /// <param name="pipeNet">Pipe net definition</param>
        /// <param name="treated">Out net connectors</param>
        /// <returns>PipeNet</returns>
        public PipeNet CreatePipeNetFrom(CompResource rootComp, PipeNetDef pipeNet, out HashSet<CompResource> treated)
        {
            Queue<CompResource> queue = new Queue<CompResource>();
            HashSet<CompResource> newNet = new HashSet<CompResource>();
            treated = new HashSet<CompResource>();

            if (rootComp.TransmitResourceNow)
            {
                queue.Enqueue(rootComp);
                treated.Add(rootComp);
                // While queue isn't empty
                while (queue.Any())
                {
                    CompResource current = queue.Dequeue();
                    newNet.Add(current);
                    // Get neigbours things
                    var things = NeighbourThingsCardinal(current.parent);
                    for (int i = 0; i < things.Count; i++)
                    {
                        var thing = things[i];
                        if (thing is ThingWithComps tWC)
                        {
                            var comps = tWC.GetComps<CompResource>();
                            foreach (var comp in comps)
                            {
                                if (!treated.Contains(comp) && comp.Props.pipeNet == pipeNet)
                                {
                                    treated.Add(comp);
                                    if (comp.TransmitResourceNow)
                                    {
                                        queue.Enqueue(comp);
                                    }
                                }
                            }
                        }
                    }
                }
                // Return the new network
                return PipeNetMaker.MakePipeNet(newNet, map, rootComp.Props.pipeNet);
            }
            else
            {
                treated.Add(rootComp);
                return PipeNetMaker.MakePipeNet(new List<CompResource> { rootComp }, map, rootComp.Props.pipeNet);
            }
        }

        /// <summary>
        /// Search the registered nets to find one of the right resource at a cell.
        /// </summary>
        /// <param name="cell">Cell we are looking for</param>
        /// <param name="resource">Resource the net need to manage</param>
        /// <returns>The PipeNet found or null</returns>
        public PipeNet GetPipeNetAt(IntVec3 cell, PipeNetDef resource)
        {
            for (int i = 0; i < pipeNetsCount; i++)
            {
                PipeNet pipeNet = pipeNets[i];
                if (pipeNet.def == resource && (pipeNet.networkGrid[cell] || valveGrid[cell]))
                    return pipeNet;
            }
            return null;
        }

        /// <summary>
        /// Register a valve
        /// </summary>
        /// <param name="pipeValve"></param>
        public void RegisterValve(CompPipeValve pipeValve)
        {
            if (pipeValve.Props.alwaysLinkToPipes)
            {
                foreach (var c in pipeValve.parent.OccupiedRect())
                {
                    valveGrid.Set(c, true);
                }
            }
        }

        /// <summary>
        /// Unregister a valve
        /// </summary>
        /// <param name="pipeValve"></param>
        public void UnregisterValve(CompPipeValve pipeValve)
        {
            if (pipeValve.Props.alwaysLinkToPipes)
            {
                foreach (var c in pipeValve.parent.OccupiedRect())
                {
                    valveGrid.Set(c, false);
                }
            }
        }

        /// <summary>
        /// Cache storage that want to be refilled for quicker job scanner
        /// </summary>
        public void UpdateRefillableWith(Thing thing)
        {
            if (wantRefill.Contains(thing))
                wantRefill.Remove(thing);
            else
                wantRefill.Add(thing);
        }
    }
}