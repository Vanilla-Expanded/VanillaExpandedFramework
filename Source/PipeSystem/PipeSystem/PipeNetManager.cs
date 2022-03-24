using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
    public class PipeNetManager : MapComponent
    {
        public PipeNetManager(Map map) : base(map)
        {
        }

        public List<PipeNet> pipeNets = new List<PipeNet>();

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
            for (int i = 0; i < pipeNets.Count; i++)
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
                var thingsAt = map.thingGrid.ThingsListAt(adjCells[i]);
                for (int o = 0; o < thingsAt.Count; o++)
                {
                    var t = thingsAt[o];
                    if (!things.Contains(t))
                        things.Add(t);
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
                var thing = (ThingWithComps)things[i];
                var comps = thing.GetComps<CompResource>();

                for (int o = 0; o < comps.Count(); o++)
                {
                    var rComp = comps.ElementAt(o);
                    if (rComp != null
                        && rComp.Props.pipeNet == comp.Props.pipeNet
                        && rComp.PipeNet is PipeNet p
                        && p != null
                        && !foundNets.Contains(p))
                    {
                        foundNets.Add(p);
                    }
                }
            }

            // If no PipeNet are adjacent, we create a new one
            if (foundNets.Count == 0)
            {
                pipeNets.Add(new PipeNet(new List<CompResource> { comp }, map, comp.Props.pipeNet));
            }
            // We found only one, register the comp to it
            else if (foundNets.Count == 1)
            {
                foundNets[0].RegisterComp(comp);
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
                if (things[i].TryGetComp<CompResource>() is CompResource cR && cR.Props.pipeNet == comp.Props.pipeNet && !foundConnectors.Contains(cR))
                    foundConnectors.Add(cR);
            }

            // Destroy the PipeNet
            comp.PipeNet.Destroy();
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

                // remove things we've seen from the list
                foreach (var connector in treated)
                    connectors.Remove(connector);
            }
        }

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
                            for (int o = 0; o < comps.Count(); o++)
                            {
                                var comp = comps.ElementAt(o);
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
                return new PipeNet(newNet, map, rootComp.Props.pipeNet);
            }
            else
            {
                treated.Add(rootComp);
                return new PipeNet(new List<CompResource> { rootComp }, map, rootComp.Props.pipeNet);
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
            return pipeNets.Find(n => n.resource == resource && n.networkGrid[cell]);
        }
    }
}