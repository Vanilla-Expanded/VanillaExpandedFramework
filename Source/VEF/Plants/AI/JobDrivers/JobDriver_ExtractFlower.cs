
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
namespace VEF.Plants
{
    public class JobDriver_ExtractFlower : JobDriver
    {
        private float workLeft;

        private float totalNeededWork;

        public const TargetIndex FlowerInd = TargetIndex.A;

        protected Thing Target => job.GetTarget(TargetIndex.A).Thing;

        protected Plant_Blooming Flower => (Plant_Blooming)Target.GetInnerIfMinified();
       
        protected float TotalNeededWork => Flower.def.plant.harvestWork;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workLeft, "workLeft", 0f);
            Scribe_Values.Look(ref totalNeededWork, "totalNeededWork", 0f);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
          
            this.FailOnForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = ToilMaker.MakeToil("MakeNewToils").FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            doWork.initAction = delegate
            {
                totalNeededWork = TotalNeededWork;
                workLeft = totalNeededWork;
            };
            doWork.tickIntervalAction = delegate (int delta)
            {
                workLeft -= JobDriver_PlantWork.WorkDonePerTick(pawn, Flower) * (float)delta;
                if (pawn.skills != null)
                {
                    pawn.skills.Learn(SkillDefOf.Plants, 0.085f * (float)delta);
                }
                if (workLeft <= 0f)
                {
                    SoundDefOf.Finish_Wood.PlayOneShot(SoundInfo.InMap(Flower));
                    doWork.actor.jobs.curDriver.ReadyForNextToil();
                }
            };
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.WithProgressBar(TargetIndex.A, () => 1f - workLeft / totalNeededWork);
            doWork.WithEffect(EffecterDefOf.Harvest_Plant, TargetIndex.A);
            doWork.PlaySustainerOrSound(() => SoundDefOf.Interact_ConstructDirt);
            doWork.activeSkill = () => SkillDefOf.Plants;
            yield return doWork;
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                IntVec3 position = Flower.Position;
                bool num = Find.Selector.IsSelected(Flower);
                Thing thing = GenSpawn.Spawn(Flower.MakeMinified(), position, pawn.Map);
                if (num && thing != null)
                {
                    Find.Selector.Select(thing, playSound: false, forceDesignatorDeselect: false);
                }
                base.Map.designationManager.RemoveAllDesignationsOn(Target);

                Flower.plantAwaitingExtraction = false;
                MapComponent_BloomingPlants mapComp = Map.GetComponent<MapComponent_BloomingPlants>();
                if (mapComp != null)
                {
                    mapComp.RemoveObjectFromMap(Flower);
                }

            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }
    }
}