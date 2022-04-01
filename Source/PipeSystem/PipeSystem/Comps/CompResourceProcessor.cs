using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace PipeSystem
{
    public class CompResourceProcessor : CompResource
    {
        public new CompProperties_ResourceProcessor Props => (CompProperties_ResourceProcessor)props;

        public float Storage;

        private CompResource otherComp;
        private CompFlickable flickable;
        private CompPowerTrader compPower;

        private int nextProcessTick;
        private List<IntVec3> adjCells;

        private bool canPushToNet;
        private bool canCreateItems;

        private bool cantRefine = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (Props.result.net != null)
            {
                var comps = parent.GetComps<CompResource>();
                for (int i = 0; i < comps.Count(); i++)
                {
                    var comp = comps.ElementAt(i);
                    if (comp.Props.pipeNet == Props.result.net)
                    {
                        otherComp = comp;
                        break;
                    }
                }
            }

            canPushToNet = Props.result.net != null && otherComp != null;
            canCreateItems = Props.result.thing != null;

            flickable = parent.GetComp<CompFlickable>();
            compPower = parent.GetComp<CompPowerTrader>();

            adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();
            nextProcessTick = Find.TickManager.TicksGame + Props.eachTicks;
        }

        public override void CompTick()
        {
            int tick = Find.TickManager.TicksGame;
            if (tick >= nextProcessTick)
            {
                if (Storage == Props.bufferSize
                    && (flickable == null || flickable.SwitchIsOn)
                    && (compPower == null || compPower.PowerOn))
                {
                    SpawnOrCreateResource();
                }
                nextProcessTick = tick + Props.eachTicks;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref Storage, "storage");
            Scribe_Values.Look(ref nextProcessTick, "nextProcessTick");
            Scribe_Values.Look(ref cantRefine, "cantRefine", false, true);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendInNewLine(base.CompInspectStringExtra());
            if (Props.showBufferInfo)
                sb.AppendInNewLine("PipeSystem_ProcessorBuffer".Translate((Storage / Props.bufferSize).ToStringPercent()));
            if (cantRefine && Props.notWorkingKey != null)
                sb.AppendInNewLine(Props.notWorkingKey.Translate());
            return sb.ToString().Trim();
        }

        private void SpawnOrCreateResource()
        {
            // If it can directly go into the net
            if (canPushToNet && otherComp.PipeNet is PipeNet net
                && (net.AvailableCapacity is float a && a > 0
                    || net.converters.Count > 0))
            {
                var count = Props.result.netCount;
                // Available storage?
                if (a > count)
                {
                    // Store it
                    net.DistributeAmongStorage(count);
                    Storage = 0;
                }
                // No storage but converters?
                else if (net.converters.Count > 0)
                {
                    // Convert it, if some left keep it inside here
                    Storage -= net.DistributeAmongConverter(count);
                }
                // We shouldn't have anymore resource, if we do -> storage full or converter full
                cantRefine = Storage > 0;
            }
            // If can't go into net
            else if (canCreateItems)
            {
                CreateItem();
            }
        }

        private void CreateItem()
        {
            var map = parent.Map;
            for (int i = 0; i < adjCells.Count; i++)
            {
                // Find an output cell
                var cell = adjCells[i];
                if (cell.Walkable(map))
                {
                    // Try find thing of the same def
                    var thing = cell.GetFirstThing(map, Props.result.thing);
                    if (thing != null)
                    {
                        // We found some, modifying stack size
                        thing.stackCount += Props.result.thingCount;
                    }
                    else
                    {
                        // We didn't find any, creating thing
                        thing = ThingMaker.MakeThing(Props.result.thing);
                        thing.stackCount = Props.result.thingCount;
                        GenSpawn.Spawn(thing, cell, map, WipeMode.FullRefund);
                    }
                    break;
                }
            }
            // Reset buffer
            Storage = 0;
        }
    }
}