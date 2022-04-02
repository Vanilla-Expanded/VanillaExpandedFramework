using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public class CompConvertToThing : CompResource
    {
        private CompBreakdownable compBreakdownable;
        private CompPowerTrader compPowerTrader;
        private CompFlickable compFlickable;

        private int maxHeldThingStackSize = 10;

        private Command_Action decrease10;
        private Command_Action decrease1;
        private Command_Action augment1;
        private Command_Action augment10;

        private static readonly Texture2D less = ContentFinder<Texture2D>.Get("UI/PS_Less");
        private static readonly Texture2D plus = ContentFinder<Texture2D>.Get("UI/PS_Plus");

        public new CompProperties_ConvertResourceToThing Props => (CompProperties_ConvertResourceToThing)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compBreakdownable = parent.GetComp<CompBreakdownable>();
            compPowerTrader = parent.GetComp<CompPowerTrader>();
            compFlickable = parent.GetComp<CompFlickable>();

            decrease10 = new Command_Action
            {
                action = delegate
                {
                    maxHeldThingStackSize -= 10;
                    if (maxHeldThingStackSize < 0)
                        maxHeldThingStackSize = 0;
                },
                defaultLabel = "PipeSystem_DecreaseStackB".Translate(),
                defaultDesc = "PipeSystem_DecreaseStackDescB".Translate(),
                icon = less
            };
            decrease1 = new Command_Action
            {
                action = delegate
                {
                    maxHeldThingStackSize--;
                    if (maxHeldThingStackSize < 0)
                        maxHeldThingStackSize = 0;
                },
                defaultLabel = "PipeSystem_DecreaseStack".Translate(),
                defaultDesc = "PipeSystem_DecreaseStackDesc".Translate(),
                icon = less
            };
            augment1 = new Command_Action
            {
                action = delegate
                {
                    maxHeldThingStackSize++;
                },
                defaultLabel = "PipeSystem_AugmentStack".Translate(),
                defaultDesc = "PipeSystem_AugmentStackDesc".Translate(),
                icon = plus
            };
            augment10 = new Command_Action
            {
                action = delegate
                {
                    maxHeldThingStackSize += 10;
                },
                defaultLabel = "PipeSystem_AugmentStackB".Translate(),
                defaultDesc = "PipeSystem_AugmentStackDescB".Translate(),
                icon = plus
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref maxHeldThingStackSize, "maxHeldThingStackSize", Props.maxOutputStackSize, true);
        }

        public bool CanOutputNow
        {
            get
            {
                return (compBreakdownable == null || !compBreakdownable.BrokenDown)
                       && (compPowerTrader == null || compPowerTrader.PowerOn)
                       && (compFlickable == null || compFlickable.SwitchIsOn);
            }
        }

        public int MaxCanOutput
        {
            get
            {
                Thing heldThing = HeldThing;
                if (heldThing == null)
                {
                    return maxHeldThingStackSize < Props.thing.stackLimit ? maxHeldThingStackSize : Props.thing.stackLimit;
                }
                if (heldThing.def == Props.thing)
                {
                    var max = maxHeldThingStackSize < heldThing.def.stackLimit ? maxHeldThingStackSize : heldThing.def.stackLimit;
                    var maxSubCount = max - heldThing.stackCount;
                    return 0 > maxSubCount ? 0 : maxSubCount;
                }
                return 0;
            }
        }

        public Thing HeldThing
        {
            get
            {
                List<Thing> thingList = parent.Map.thingGrid.ThingsListAt(parent.Position);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i].def == Props.thing)
                        return thingList[i];
                }
                return null;
            }
        }

        public void OutputResource(int amount)
        {
            if (CanOutputNow)
            {
                var heldThing = HeldThing;
                if (heldThing != null)
                {
                    heldThing.stackCount += amount;
                }
                else
                {
                    Thing createdThing = ThingMaker.MakeThing(Props.thing);
                    createdThing.stackCount = amount;
                    GenSpawn.Spawn(createdThing, parent.Position, parent.Map, WipeMode.VanishOrMoveAside);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("PipeSystem_AmountOut".Translate(maxHeldThingStackSize));
            sb.AppendLine(base.CompInspectStringExtra());
            return sb.ToString().Trim();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            yield return decrease10;
            yield return decrease1;
            yield return augment1;
            yield return augment10;
        }
    }
}