using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PipeSystem
{
    public class CompSpawnerOrNet : CompResource
    {
        private CompCanBeDormant compCanBeDormant;
        private CompPowerTrader compPower;
        private CompResource compResource;
        private int ticksUntilSpawn;

        public new CompProperties_SpawnerOrNet Props => (CompProperties_SpawnerOrNet)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            compPower = parent.GetComp<CompPowerTrader>();
            compResource = parent.GetComp<CompResource>();
            compCanBeDormant = parent.GetComp<CompCanBeDormant>();

            if (!respawningAfterLoad)
                ticksUntilSpawn = Props.spawnIntervalRange.RandomInRange;
        }

        public override string CompInspectStringExtra()
        {
            if (!parent.Spawned)
                return null;

            if (!Props.writeTimeLeftToSpawn || (Props.requiresPower && (compPower == null || !compPower.PowerOn)))
                return base.CompInspectStringExtra();

            return base.CompInspectStringExtra() + "\n" + "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(Props.thingToSpawn, null, Props.spawnCount)).Resolve() + ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksUntilSpawn, (Props.saveKeysPrefix.NullOrEmpty() ? null : Props.saveKeysPrefix + "_") + "ticksUntilSpawn");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    action = delegate
                    {
                        ticksUntilSpawn = 50;
                    },
                    defaultLabel = "Spawn now",
                    defaultDesc = "Spawn now",
                };
            }

            yield break;
        }

        public override void CompTick() => TickInterval(1);

        public override void CompTickRare() => TickInterval(250);

        private void TickInterval(int interval)
        {
            if (!parent.Spawned
                || (compCanBeDormant != null && !compCanBeDormant.Awake)
                || parent.Position.Fogged(parent.Map)
                || (compPower != null && !compPower.PowerOn))
            {
                return;
            }

            ticksUntilSpawn -= interval;
            if (ticksUntilSpawn <= 0)
            {
                ticksUntilSpawn = Props.spawnIntervalRange.RandomInRange;
                TryDoSpawn();
            }
        }

        private bool TryDoSpawn()
        {
            if (!parent.Spawned)
                return false;

            if (Props.spawnMaxAdjacent >= 0)
            {
                int num = 0;
                for (int i = 0; i < 9; ++i)
                {
                    IntVec3 c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
                    if (c.InBounds(parent.Map))
                    {
                        List<Thing> thingList = c.GetThingList(parent.Map);
                        for (int index2 = 0; index2 < thingList.Count; ++index2)
                        {
                            if (thingList[index2].def == Props.thingToSpawn)
                            {
                                num += thingList[index2].stackCount;
                                if (num >= Props.spawnMaxAdjacent)
                                    return false;
                            }
                        }
                    }
                }
            }

            var net = compResource.PipeNet;
            if (net.AvailableCapacity >= Props.spawnCount)
            {
                net.DistributeAmongStorage(Props.spawnCount, out _);

                if (Props.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(Props.thingToSpawn.LabelCap), parent, MessageTypeDefOf.PositiveEvent);

                return true;
            }

            if (!CompSpawner.TryFindSpawnCell(parent, Props.thingToSpawn, Props.spawnCount, out IntVec3 result))
                return false;

            Thing thing = ThingMaker.MakeThing(Props.thingToSpawn);
            thing.stackCount = Props.spawnCount;

            if (thing == null)
                Log.Error("Could not spawn anything for " + parent);

            if (Props.inheritFaction && thing.Faction != parent.Faction)
                thing.SetFaction(parent.Faction);

            GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out Thing lastResultingThing);

            if (Props.spawnForbidden)
                lastResultingThing.SetForbidden(true);

            if (Props.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
                Messages.Message("MessageCompSpawnerSpawnedItem".Translate(Props.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);

            return true;
        }
    }
}