using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
    public class PipeNet
    {
        public List<CompResource> connectors = new List<CompResource>();
        public List<CompResourceTrader> producers = new List<CompResourceTrader>();
        public List<CompResourceTrader> receivers = new List<CompResourceTrader>();
        public List<CompResourceStorage> storages = new List<CompResourceStorage>();
        public List<CompConvertToThing> converters = new List<CompConvertToThing>();
        public List<CompResourceProcessor> processors = new List<CompResourceProcessor>();

        public Map map;
        public BoolGrid networkGrid;
        public PipeNetDef resource;
        public bool receiversDirty;
        public bool producersDirty;

        private readonly List<CompResourceTrader> receiversOn = new List<CompResourceTrader>();
        private readonly List<CompResourceTrader> receiversOff = new List<CompResourceTrader>();
        private readonly List<CompResourceTrader> producersOn = new List<CompResourceTrader>();
        private readonly List<CompResourceTrader> producersOff = new List<CompResourceTrader>();

        public int NextTick { get; set; }
        public float Consumption { get; private set; }
        public float Production { get; private set; }
        public float Stored { get; private set; }
        public float MaxGridStorageCapacity { get; private set; }
        public float AvailableCapacity => MaxGridStorageCapacity - Stored;

        public PipeNet(IEnumerable<CompResource> connectors, Map map, PipeNetDef resource)
        {
            this.map = map;
            this.resource = resource;
            networkGrid = new BoolGrid(map);

            NextTick = Find.TickManager.TicksGame;
            MaxGridStorageCapacity = 0;

            // Register all
            for (int i = 0; i < connectors.Count(); i++)
            {
                RegisterComp(connectors.ElementAt(i));
            }
        }

        /// <summary>
        /// Loop on all receivers, sort them in receiversOn and receiversOff.
        /// Update Consumption. This is called when receiversDirty is true.
        /// </summary>
        private void ReceiversDirty()
        {
            PipeSystemDebug.Message("Receivers dirty");
            receiversOn.Clear();
            receiversOff.Clear();

            float consumption = 0f;
            for (int i = 0; i < receivers.Count; i++)
            {
                var trader = receivers[i];
                if (trader.ResourceOn)
                {
                    receiversOn.Add(trader);
                    consumption += trader.Consumption;
                }
                else
                {
                    receiversOff.Add(trader);
                }
            }

            Consumption = consumption;
            receiversDirty = false;
        }

        /// <summary>
        /// Loop on all producers, sort them in producersOn and producersOff.
        /// Update Production. This is called when producersDirty is true.
        /// </summary>
        private void ProducersDirty()
        {
            PipeSystemDebug.Message("Producers dirty");
            producersOn.Clear();
            producersOff.Clear();

            float production = 0f;
            for (int i = 0; i < producers.Count; i++)
            {
                var trader = producers[i];
                if (trader.ResourceOn)
                {
                    producersOn.Add(trader);
                    production += -trader.Consumption;
                }
                else
                {
                    producersOff.Add(trader);
                }
            }

            Production = production;
            producersDirty = false;
        }

        /// <summary>
        /// Unregister all comp, remove the class from manager list
        /// </summary>
        public virtual void Destroy()
        {
            PipeSystemDebug.Message("Removing pipe net");
            map.GetComponent<PipeNetManager>().pipeNets.Remove(this);
            for (int i = 0; i < connectors.Count; i++)
            {
                UnregisterComp(connectors[i]);
            }
        }

        /// <summary>
        /// Merge two PipeNet together.
        /// </summary>
        /// <param name="otherNet">Other PipeNet to merge</param>
        public virtual void Merge(PipeNet otherNet)
        {
            for (int i = 0; i < otherNet.connectors.Count; i++)
            {
                RegisterComp(otherNet.connectors[i]);
            }
            map.GetComponent<PipeNetManager>().pipeNets.Remove(otherNet);
        }

        /// <summary>
        /// This is registering a comp into this net. All comps are added to connectors.
        /// Then sorted into traders or storages. Then setting the comp pipeNet to this one.
        /// Finnaly setting the networkGrid cell(s) to true.
        /// </summary>
        /// <param name="comp">The comp of the thing we register</param>
        public virtual void RegisterComp(CompResource comp)
        {
            if (connectors.Contains(comp))
                return;

            connectors.Add(comp);
            if (comp is CompResourceTrader trader)
            {
                if (trader.Consumption < 0f)
                {
                    producers.Add(trader);
                    producersDirty = true;
                }
                else if (trader.Consumption > 0f)
                {
                    receivers.Add(trader);
                    receiversDirty = true;
                }
            }
            else if (comp is CompResourceStorage storage)
            {
                storages.Add(storage);
                MaxGridStorageCapacity += storage.Props.storageCapacity;
            }
            else if (comp is CompConvertToThing convertToThing)
            {
                converters.Add(convertToThing);
            }
            else if (comp is CompResourceProcessor processor)
            {
                processors.Add(processor);
            }
            comp.PipeNet = this;

            var cells = comp.parent.OccupiedRect().Cells;
            for (int i = 0; i < cells.Count(); i++)
            {
                networkGrid.Set(cells.ElementAt(i), true);
            }
        }

        /// <summary>
        /// Remove a comp from the net.
        /// </summary>
        /// <param name="comp"></param>
        public virtual void UnregisterComp(CompResource comp)
        {
            connectors.Remove(comp);
            if (comp is CompResourceTrader trader)
            {
                if (trader.Consumption < 0f)
                {
                    producers.Remove(trader);
                    producersDirty = true;
                }
                else if (trader.Consumption > 0f)
                {
                    receivers.Remove(trader);
                    receiversDirty = true;
                }
            }
            else if (comp is CompResourceStorage storage)
            {
                storages.Remove(storage);
                MaxGridStorageCapacity -= storage.AmountCanAccept;
            }
            else if (comp is CompConvertToThing convertToThing)
            {
                converters.Remove(convertToThing);
            }
            else if (comp is CompResourceProcessor processor)
            {
                processors.Remove(processor);
            }

            var cells = comp.parent.OccupiedRect().Cells;
            for (int i = 0; i < cells.Count(); i++)
            {
                networkGrid.Set(cells.ElementAt(i), false);
            }

            if (connectors.NullOrEmpty())
                Destroy();
        }

        /// <summary>
        /// Loop through each storage and add up the amount stored.
        /// </summary>
        /// <returns>Current pipe net storage</returns>
        public float CurrentStored()
        {
            float stored = 0f;
            for (int i = 0; i < storages.Count; i++)
            {
                stored += storages[i].AmountStored;
            }
            return stored;
        }

        /// <summary>
        /// This is where we manage production/consumption/storage of one tick.
        /// </summary>
        public virtual void PipeSystemTick()
        {
            if (receiversDirty)
                ReceiversDirty();
            if (producersDirty)
                ProducersDirty();

            // Turn on producers that want and can be on
            for (int i = 0; i < producersOff.Count; i++)
            {
                var trader = producersOff[i];
                if (trader.CanBeOn())
                    trader.ResourceOn = true;
            }

            // Update current storage
            Stored = CurrentStored();

            // Available resource this tick
            float available = Production + Stored;
            // If not enough
            if (available < Consumption && receiversOn.Any())
            {
                PipeSystemDebug.Message("Turning off random building");
                // Turn off random building
                CompResourceTrader comp = receiversOn.RandomElement();
                Consumption = Math.Max(0f, Consumption - comp.Consumption);
                comp.ResourceOn = false;
                receiversDirty = true;
            }
            // Get all buildings that can potentially turn on
            var wantToBeOn = receiversOff.FindAll(t => t.CanBeOn() && (available - (Consumption + t.Consumption)) > 0f);
            if (wantToBeOn.Any())
            {
                PipeSystemDebug.Message("Turning on random building");
                // Turn random building on
                CompResourceTrader comp = wantToBeOn.RandomElement();
                Consumption += comp.Consumption;
                comp.ResourceOn = true;
                receiversDirty = true;
            }

            // Get the usable resource
            var usable = Production - Consumption;
            // Draw from storage if we use more than we produce
            if (usable < 0)
            {
                DrawAmongStorage(-usable);
            }
            // If we produce resource, and there is storage
            else if (storages.Count > 0)
            {
                // Store it
                DistributeAmongStorage(usable);
                // Distribute using the whole storage
                DistributeAmongProcessor(Stored);
                DistributeAmongConverter(Stored);
            }
            else
            {
                var pUsage = DistributeAmongProcessor(usable);
                DistributeAmongConverter(usable - pUsage);
            }
        }

        /// <summary>
        /// Distribute resources stored into the converters
        /// </summary>
        private float DistributeAmongProcessor(float available)
        {
            float used = 0;
            if (processors.Count == 0 || available <= 0)
                return used;

            for (int i = 0; i < processors.Count; i++)
            {
                var processor = processors[i];
                var sub = processor.Props.bufferSize - processor.Storage;

                if (sub > 0)
                {
                    var toStore = sub > available ? available : sub;
                    processor.Storage += toStore;
                    available -= toStore;
                    used += toStore;
                }

                if (available <= 0)
                    break;
            }

            DrawAmongStorage(used);
            return used;
        }

        /// <summary>
        /// Distribute resources stored into the converters
        /// </summary>
        internal float DistributeAmongConverter(float available)
        {
            float used = 0;
            if (!converters.Any() || available <= 0)
                return used;

            // Get all converter ready
            var convertersReady = new List<CompConvertToThing>();
            for (int i = 0; i < converters.Count; i++)
            {
                var converter = converters[i];
                if (converter.CanOutputNow && converter.MaxCanOutput > 0)
                {
                    convertersReady.Add(converter);
                }
            }
            // If no converters are ready
            if (convertersReady.Count == 0)
                return used;

            // Convert it
            for (int i = 0; i < convertersReady.Count; i++)
            {
                var converter = convertersReady[i];
                // cap to max amount storage can accept
                int availableWRatio = (int)(available / converter.Props.ratio);
                int max = converter.MaxCanOutput;
                int toConvert = max > availableWRatio ? availableWRatio : max;
                if (toConvert > 0)
                {
                    converter.OutputResource(toConvert);
                    var toDraw = toConvert * converter.Props.ratio;
                    used += toDraw;
                    available -= toDraw;
                    PipeSystemDebug.Message($"Converted {toDraw} resource for {toConvert}");
                }
                // Don't iterate if nothing more is available
                else break;
            }

            DrawAmongStorage(used);
            return used;
        }

        /// <summary>
        /// Add resource to storage.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        internal void DistributeAmongStorage(float amount)
        {
            if (amount <= 0 || !storages.Any())
                return;
            // Get all storage that can accept resources
            List<CompResourceStorage> resourceStorages = storages.FindAll(s => s.AmountCanAccept > 0).ToList();
            // If all full
            if (resourceStorages.Count == 0)
                return;
            // Get the amount that can be stored, max amount being the whole grid capacity
            float toBeStored = Math.Min(amount, MaxGridStorageCapacity - Stored);
            // Store it
            int iteration = 0;
            while (toBeStored > 0)
            {
                ++iteration;
                if (iteration > 1000)
                {
                    PipeSystemDebug.Message("To many iteration in DistributeAmongStorage");
                    break;
                }
                // Amount to store in each
                float amountInEach = toBeStored / resourceStorages.Count;
                for (int i = 0; i < resourceStorages.Count; i++)
                {
                    var storage = resourceStorages[i];
                    // cap amountInEach to amount storage can accept
                    float toStore = Math.Min(storage.AmountCanAccept, amountInEach);
                    storage.AddResource(toStore);
                    // If full delete for the next iteration
                    if (storage.AmountCanAccept == 0) resourceStorages.Remove(storage);
                    // update toBeStored
                    toBeStored -= toStore;
                }
            }
        }

        /// <summary>
        /// Withdraw resource from storage.
        /// </summary>
        /// <param name="amount">Amount to draw</param>
        internal void DrawAmongStorage(float amount)
        {
            Log.Message($"Drawing {amount} from storage");
            if (amount <= 0 || !storages.Any())
                return;
            // Get all storage that can provide resources
            List<CompResourceStorage> resourceStorages = storages.FindAll(s => s.AmountStored > 0).ToList();
            // If all empty
            if (resourceStorages.Count == 0)
                return;
            // Draw it
            int iteration = 0;
            while (amount > 0)
            {
                ++iteration;
                if (iteration > 1000)
                {
                    PipeSystemDebug.Message("To many iteration in DrawAmongStorage");
                    break;
                }
                // Amount to draw in each
                float amountInEach = amount / resourceStorages.Count;
                for (int i = 0; i < resourceStorages.Count; i++)
                {
                    var storage = resourceStorages[i];
                    // cap amountInEach to amount that can be draw
                    float toDraw = Math.Min(storage.AmountStored, amountInEach);
                    storage.DrawResource(toDraw);
                    // If empty delete for the next iteration
                    if (storage.AmountStored == 0) resourceStorages.Remove(storage);
                    // update toBeStored
                    amount -= toDraw;
                }
            }
        }
    }
}