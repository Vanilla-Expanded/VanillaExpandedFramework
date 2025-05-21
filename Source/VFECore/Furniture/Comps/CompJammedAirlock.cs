
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaFurnitureExpanded
{
    public class CompJammedAirlock : CompInteractable
    {
        public Building Door => (Building)parent;

        public new CompProperties_JammedAirlock Props => (CompProperties_JammedAirlock)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (this.parent.Position.GetThingList(this.parent.Map)?.ContainsAny(x => x!= this.parent && x.def.passability == Traversability.Impassable) == true)
            {
                List<Thing> thingsToDespawn = new List<Thing>();

                foreach (IntVec3 tile in this.parent.OccupiedRect().Cells)
                {
                    foreach (Thing thing in tile.GetThingList(this.parent.Map))
                    {
                        if (thing.def.passability == Traversability.Impassable)
                        {
                            thingsToDespawn.Add(thing);
                        }
                    }
                }

                if (thingsToDespawn.Count > 0)
                {
                    foreach (Thing thing in thingsToDespawn)
                    {
                        thing.DeSpawn();
                    }

                }

            }

        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            OrderActivation(target.Pawn);
        }

        public override string CompInspectStringExtra()
        {
            if (Props.stringExtra != "")
            {
                return Props.stringExtra.Translate();
            }
            else return null;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {

            AcceptanceReport acceptanceReport = CanInteract(selPawn);
            FloatMenuOption floatMenuOption = new FloatMenuOption(Props.jobString.CapitalizeFirst(), delegate
            {
                OrderActivation(selPawn);
            });
            if (!acceptanceReport.Accepted)
            {
                floatMenuOption.Disabled = true;
                floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
            }
            yield return floatMenuOption;

        }
       

        protected override void OnInteracted(Pawn caster)
        {

            if (caster.IsColonist)
            {
                parent.Map.fogGrid.FloodUnfogAdjacent(parent.Position, sendLetters: false);
            }
            IntVec3 pos = this.parent.PositionHeld;
            Map map = this.parent.Map;
            Rot4 rot = this.parent.Rotation;
            if (this.parent.Spawned)
            {

                this.parent.DeSpawn();
            }

            GenSpawn.Spawn(ThingMaker.MakeThing(Props.doorToConvertTo), pos, map, rot);

        }

        private void OrderActivation(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
            job.count = 1;
            job.playerForced = true;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }


    }
}