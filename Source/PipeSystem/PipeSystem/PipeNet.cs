using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public List<CompRefillWithPipes> refuelables = new List<CompRefillWithPipes>();
        public List<CompResourceProcessor> processors = new List<CompResourceProcessor>();

        public Map map;
        public BoolGrid networkGrid;
        public PipeNetDef def;
        public bool receiversDirty;
        public bool producersDirty;

        internal List<CompResourceStorage> markedForTransfer = new List<CompResourceStorage>();

        private readonly List<CompResourceTrader> receiversOn = new List<CompResourceTrader>();
        private readonly List<CompResourceTrader> receiversOff = new List<CompResourceTrader>();
        private readonly List<CompResourceTrader> producersOn = new List<CompResourceTrader>();
        private readonly List<CompResourceTrader> producersOff = new List<CompResourceTrader>();

        public int NextTick { get; set; }
        public float Consumption { get; private set; }
        public float Production { get; private set; }
        public float Stored { get; private set; }
        public float MaxGridStorageCapacity { get; internal set; }
        public float AvailableCapacity => MaxGridStorageCapacity - Stored;

        public PipeNet(IEnumerable<CompResource> connectors, Map map, PipeNetDef def)
        {
            this.map = map;
            this.def = def;
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
                if (storage.markedForTransfer)
                {
                    markedForTransfer.Add(storage);
                }
                else
                {
                    storages.Add(storage);
                    MaxGridStorageCapacity += storage.Props.storageCapacity;
                }
            }
            else if (comp is CompConvertToThing convertToThing)
            {
                converters.Add(convertToThing);
            }
            else if (comp is CompResourceProcessor processor)
            {
                processors.Add(processor);
            }
            else if (comp is CompRefillWithPipes refuelable)
            {
                refuelables.Add(refuelable);
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
            else if (comp is CompRefillWithPipes refuelable)
            {
                refuelables.Remove(refuelable);
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
            float usable = Production - Consumption;
            // Draw from storage if we use more than we produce
            if (usable < 0)
            {
                DrawAmongStorage(-usable, storages);
            }
            // If we produce resource, and there is storage
            else if (storages.Count > 0)
            {
                // Store it
                DistributeAmongStorage(usable);
                // Distribute using the whole storage
                DistributeAmongRefuelables(Stored);
                DistributeAmongProcessor(Stored);
                DistributeAmongConverter(Stored);
            }
            else
            {
                float rUsage = DistributeAmongRefuelables(usable);
                float leftAfter = usable - rUsage;
                float pUsage = DistributeAmongProcessor(leftAfter);
                DistributeAmongConverter(leftAfter - pUsage);
            }

            // Manage the tank marked for transfer
            if (markedForTransfer.Count > 0)
            {
                float canTransfer = 0f;
                // Get everything that need to be transfered
                for (int i = 0; i < markedForTransfer.Count; i++)
                {
                    var marked = markedForTransfer[i];
                    canTransfer += marked.AmountStored;
                }
                // Actual transfer we will do
                float availableCapacity = AvailableCapacity;
                float toTransfer = availableCapacity > canTransfer ? canTransfer : availableCapacity;
                float willTransfer = toTransfer > 100 ? 100 : toTransfer;
                // Draw from marked and distribute to others
                DrawAmongStorage(willTransfer, markedForTransfer);
                DistributeAmongStorage(willTransfer);
            }
        }

        /// <summary>
        /// Distribute resources stored into the processors
        /// </summary>
        private float DistributeAmongProcessor(float available)
        {
            float used = 0;
            if (processors.Count == 0 || available <= 0)
                return used;

            for (int i = 0; i < processors.Count; i++)
            {
                var temp = processors[i].PushTo(available);
                available -= temp;
                used += temp;

                if (available <= 0)
                    break;
            }

            DrawAmongStorage(used, storages);
            return used;
        }

        // <summary>
        /// Distribute resources stored into the converters
        /// </summary>
        internal float DistributeAmongRefuelables(float available)
        {
            float used = 0;
            if (refuelables.Count == 0 || available <= 0)
                return used;

            for (int i = 0; i < refuelables.Count; i++)
            {
                var refuelable = refuelables[i];
                var compRefuelable = refuelables[i].compRefuelable;

                var toAdd = compRefuelable.TargetFuelLevel - compRefuelable.Fuel; // The amount of fuel needed by compRefuelable
                var resourceNeeded = toAdd * refuelable.Props.ratio; // Converted to the amount of resource
                // Check if needed resource is more that available resource
                var resourceCanBeUsed = resourceNeeded < available ? resourceNeeded : available; // Can we spare all of it?

                compRefuelable.Refuel(resourceCanBeUsed / refuelable.Props.ratio);

                available -= resourceCanBeUsed;
                used += resourceCanBeUsed;

                if (available <= 0)
                    break;
            }

            DrawAmongStorage(used, storages);
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

            DrawAmongStorage(used, storages);
            return used;
        }

        /// <summary>
        /// Add resource to storage.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void DistributeAmongStorage(float amount)
        {
            if (amount <= 0 || !storages.Any())
                return;
            // Get all storage that can accept resources
            var resourceStorages = new List<CompResourceStorage>();
            for (int i = 0; i < storages.Count; i++)
            {
                var storage = storages[i];
                if (!storage.markedForTransfer && storage.AmountCanAccept > 0)
                    resourceStorages.Add(storage);
            }
            // If all full
            if (resourceStorages.Count == 0)
                return;
            // Get the amount that can be stored, max amount being the whole grid capacity
            float toBeStored = Mathf.Min(amount, MaxGridStorageCapacity - Stored);
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
                    float toStore = Mathf.Min(storage.AmountCanAccept, amountInEach);
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
        public void DrawAmongStorage(float amount, List<CompResourceStorage> storages)
        {
            if (amount <= 0 || storages.Count == 0)
                return;
            // Get all storage that can provide resources
            var resourceStorages = new List<CompResourceStorage>();
            for (int i = 0; i < storages.Count; i++)
            {
                var storage = storages[i];
                if (storage.AmountStored > 0)
                    resourceStorages.Add(storage);
            }
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
                    float toDraw = Mathf.Min(storage.AmountStored, amountInEach);
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