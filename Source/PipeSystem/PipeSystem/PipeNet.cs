using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class PipeNet
    {
        // Everything
        public List<CompResource> connectors = new List<CompResource>();
        // Producing traders
        public List<CompResourceTrader> producers = new List<CompResourceTrader>();
        // Consuming traders
        public List<CompResourceTrader> receivers = new List<CompResourceTrader>();
        // Storages
        public List<CompResourceStorage> storages = new List<CompResourceStorage>();
        // Converters to things
        public List<CompConvertToThing> thingConverters = new List<CompConvertToThing>();
        // Converters to resource
        public List<CompConvertToResource> resourceConverters = new List<CompConvertToResource>();
        // All refillables compRefuelable/ItemProcessor
        public List<CompRefillWithPipes> refillables = new List<CompRefillWithPipes>();
        // Processor
        public List<CompResourceProcessor> processors = new List<CompResourceProcessor>();
        // Extractors
        public List<CompDeepExtractor> extractors = new List<CompDeepExtractor>();

        public Map map;
        public BoolGrid networkGrid;
        public PipeNetDef def;
        public bool receiversDirty;
        public bool nextTickRDirty;
        public bool producersDirty;

        internal List<CompResourceStorage> markedForTransfer = new List<CompResourceStorage>();

        protected readonly List<CompResourceTrader> receiversOn = new List<CompResourceTrader>();
        protected readonly List<CompResourceTrader> receiversOff = new List<CompResourceTrader>();
        protected readonly List<CompResourceTrader> producersOn = new List<CompResourceTrader>();
        protected readonly List<CompResourceTrader> producersOff = new List<CompResourceTrader>();

        public int NextTick { get; set; }

        public float Consumption { get; private set; }

        public float Production { get; private set; }

        public float ExtractorRawProduction
        {
            get
            {
                var prod = 0f;
                for (int i = 0; i < extractors.Count; i++)
                    prod += extractors[i].RawProduction;

                return prod;
            }
        }

        public float Stored { get; private set; }

        public float AvailableCapacity
        {
            get
            {
                var available = 0f;
                for (int i = 0; i < storages.Count; i++)
                {
                    available += storages[i].AmountCanAccept;
                }

                return available;
            }
        }

        public float ThingConvertersMaxOutput
        {
            get
            {
                var convertersCapacity = 0;
                for (int i = 0; i < thingConverters.Count; i++)
                {
                    var converter = thingConverters[i];
                    if (converter.CanOutputNow)
                        convertersCapacity += converter.MaxCanOutput;
                }
                return convertersCapacity;
            }
        }

        public float RefillableAmount
        {
            get
            {
                var refillableAmount = 0f;
                for (int i = 0; i < refillables.Count; i++)
                {
                    var refillable = refillables[i];
                    refillableAmount += (refillable.compRefuelable.TargetFuelLevel - refillable.compRefuelable.Fuel) * refillable.Props.ratio;
                }
                return refillableAmount;
            }
        }

        /// <summary>
        /// Loop on all receivers, sort them in receiversOn and receiversOff.
        /// Update Consumption. This is called when receiversDirty is true.
        /// </summary>
        protected bool ReceiversDirty()
        {
            var nextTickDirty = false;
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

                    if (trader.UsedLastTick)
                    {
                        if (!map.reservationManager.IsReservedByAnyoneOf(trader.parent, Faction.OfPlayer))
                        {
                            trader.UsedLastTick = false;
                            PipeSystemDebug.Message("setting UsedLastTick to false");
                        }

                        nextTickDirty = true;
                    }
                }
                else
                {
                    receiversOff.Add(trader);
                }
            }

            Consumption = consumption;
            receiversDirty = false;

            return nextTickDirty;
        }

        /// <summary>
        /// Loop on all producers, sort them in producersOn and producersOff.
        /// Update Production. This is called when producersDirty is true.
        /// </summary>
        protected void ProducersDirty()
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
                else if (trader.Consumption >= 0f)
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
                }
            }
            else if (comp is CompConvertToThing convertToThing)
            {
                thingConverters.Add(convertToThing);
            }
            else if (comp is CompConvertToResource toResource)
            {
                resourceConverters.Add(toResource);
            }
            else if (comp is CompResourceProcessor processor)
            {
                processors.Add(processor);
            }
            else if (comp is CompRefillWithPipes refuelable)
            {
                refillables.Add(refuelable);
            }
            else if (comp is CompDeepExtractor extractor)
            {
                extractors.Add(extractor);
            }
            comp.PipeNet = this;

            foreach (var c in comp.parent.OccupiedRect())
            {
                networkGrid.Set(c, true);
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
            }
            else if (comp is CompConvertToThing convertToThing)
            {
                thingConverters.Remove(convertToThing);
            }
            else if (comp is CompConvertToResource toResource)
            {
                resourceConverters.Remove(toResource);
            }
            else if (comp is CompResourceProcessor processor)
            {
                processors.Remove(processor);
            }
            else if (comp is CompRefillWithPipes refuelable)
            {
                refillables.Remove(refuelable);
            }
            else if (comp is CompDeepExtractor extractor)
            {
                extractors.Remove(extractor);
            }

            foreach (var c in comp.parent.OccupiedRect())
            {
                networkGrid.Set(c, false);
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
            if (receiversDirty || nextTickRDirty)
                nextTickRDirty = ReceiversDirty();
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
            var wantToBeOn = new List<CompResourceTrader>();
            for (int w = 0; w < receiversOff.Count; w++)
            {
                var r = receiversOff[w];
                if (r.CanBeOn() && (available - (Consumption + r.Consumption)) > 0f)
                    wantToBeOn.Add(r);
            }

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
                // Distribute using the whole storage
                DistributeAmongRefillables(Stored);
                DistributeAmongProcessors(Stored);
                DistributeAmongConverters(Stored);
            }
            // If we produce resource, and there is storage
            else if (storages.Count > 0)
            {
                // Store it
                DistributeAmongStorage(usable, out _);
                // Get other inputs
                GetFromConverters();
                // Distribute using the whole storage
                DistributeAmongRefillables(Stored);
                DistributeAmongProcessors(Stored);
                DistributeAmongConverters(Stored);
            }
            else
            {
                float rUsage = DistributeAmongRefillables(usable);
                float leftAfter = usable - rUsage;
                float pUsage = DistributeAmongProcessors(leftAfter);
                DistributeAmongConverters(leftAfter - pUsage);
            }

            // Manage the tank marked for transfer
            if (markedForTransfer.Count > 0)
            {
                float canTransfer = 0f;
                // Get everything that need to be transfered
                for (int i = 0; i < markedForTransfer.Count; i++)
                {
                    canTransfer += markedForTransfer[i].AmountStored;
                }
                // Actual transfer we will do
                float availableCapacity = AvailableCapacity;
                float toTransfer = availableCapacity > canTransfer ? canTransfer : availableCapacity;
                float willTransfer = toTransfer > def.transferAmount ? def.transferAmount : toTransfer;
                // Draw from marked and distribute to others
                DrawAmongStorage(willTransfer, markedForTransfer);
                DistributeAmongStorage(willTransfer, out _);
            }
        }

        /// <summary>
        /// Distribute resources stored into the processors
        /// </summary>
        private float DistributeAmongProcessors(float available)
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

        /// <summary>
        /// Distribute resources stored into the converters
        /// </summary>
        internal float DistributeAmongRefillables(float available)
        {
            float used = 0;
            if (refillables.Count == 0 || available <= 0)
                return used;

            for (int i = 0; i < refillables.Count; i++)
            {
                var refillable = refillables[i];
                float resourceUsed = refillable.Refill(available);

                available -= resourceUsed;
                used += resourceUsed;

                if (available <= 0)
                    break;
            }

            DrawAmongStorage(used, storages);
            return used;
        }

        /// <summary>
        /// Distribute resources stored into the converters
        /// </summary>
        internal float DistributeAmongConverters(float available)
        {
            float used = 0;
            if (!thingConverters.Any() || available <= 0)
                return used;

            // Get all converter ready
            var convertersReady = new List<CompConvertToThing>();
            for (int i = 0; i < thingConverters.Count; i++)
            {
                var converter = thingConverters[i];
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
                if (converter.CanOutputNow)
                {
                    // Cap to max amount storage can accept
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
                    else
                    {
                        break;
                    }
                }
            }

            DrawAmongStorage(used, storages);
            return used;
        }

        /// <summary>
        /// Take resources away from converters
        /// </summary>
        internal void GetFromConverters()
        {
            for (int i = 0; i < resourceConverters.Count; i++)
            {
                var converter = resourceConverters[i];
                // Verify others comps
                if (converter.CanInputNow)
                {
                    var heldThing = converter.HeldThing;
                    // Anything stored on the converter?
                    if (heldThing != null)
                    {
                        // Get the resource we can add to the net
                        var resourceToAdd = heldThing.stackCount * converter.Props.ratio;

                        float resourceCanAdd;
                        if (def.convertAmount > 0)
                            resourceCanAdd = Mathf.Min(resourceToAdd, AvailableCapacity, def.convertAmount);
                        else
                            resourceCanAdd = Mathf.Min(resourceToAdd, AvailableCapacity);

                        if (resourceCanAdd > 0)
                        {
                            // Add it
                            DistributeAmongStorage(resourceCanAdd, out _);
                            // Change heldthing stacksize, or despawn it
                            if (resourceToAdd == resourceCanAdd)
                            {
                                heldThing.DeSpawn();
                            }
                            else
                            {
                                heldThing.stackCount -= (int)(resourceCanAdd / converter.Props.ratio);
                                // We did not use all converter thing, no need to iterate over the other ones
                                return;
                            }
                        }
                    }
                }
            }
        }

        [Obsolete]
        public void DistributeAmongStorage(float amount) => DistributeAmongStorage(amount, out _);

        /// <summary>
        /// Add resource to storage.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void DistributeAmongStorage(float amount, out float stored)
        {
            stored = 0;
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
            float toBeStored = Mathf.Min(amount, AvailableCapacity);
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
                    stored += toStore;
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
                    PipeSystemDebug.Message($"To many iteration in DrawAmongStorage. Amount left: {amount}");
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

        public override string ToString()
        {
            return $"PipeNet: {def.resource.name} Stored: {Stored} AvailableCapacity: {AvailableCapacity} Consumption: {Consumption} Production: {Production}";
        }
    }
}