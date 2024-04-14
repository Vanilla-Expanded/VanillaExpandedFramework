using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace AnimalBehaviours
{
    public class JobGiver_GetWeirdFood : ThinkNode_JobGiver
    {

        private HungerCategory minCategory;

        private float maxLevelPercentage = 1f;

        public bool forceScanWholeMap;

        private Effecter effecter;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GetWeirdFood jobGiver_GetFood = (JobGiver_GetWeirdFood)base.DeepCopy(resolve);
            jobGiver_GetFood.minCategory = this.minCategory;
            jobGiver_GetFood.maxLevelPercentage = this.maxLevelPercentage;
            jobGiver_GetFood.forceScanWholeMap = this.forceScanWholeMap;
            return jobGiver_GetFood;
        }

        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn?.needs?.food;
            if (food == null)
            {
                return 0f;
            }
            if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
            {
                return 0f;
            }
            if (food.CurCategory < this.minCategory)
            {
                return 0f;
            }
            if (food.CurLevelPercentage > this.maxLevelPercentage)
            {
                return 0f;
            }
            if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
            {
                return 9.5f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Food food = pawn?.needs?.food;
            if (food == null || food.CurCategory < this.minCategory || food.CurLevelPercentage > this.maxLevelPercentage)
            {
                return null;
            }

            //Log.Message("Eating");

            ThingDef thingDef = null;
            Thing thing = null;

            CompEatWeirdFood comp = pawn.TryGetComp<CompEatWeirdFood>();
            if (comp != null)
            {
                foreach (string customThingToEat in comp.Props.customThingToEat)
                {
                    thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(customThingToEat);
                    if (thingDef != null)
                    {
                        thing = FindWeirdFoodInMap(thingDef, pawn);

                        if (thing != null)
                        {

                            break;
                        }

                    }

                }

            }
            else return null;
            if (thing != null && pawn.Map.reservationManager.CanReserve(pawn, thing, 1))
            {
                //float nutrition = 1f;
                Job job3 = JobMaker.MakeJob(InternalDefOf.VEF_IngestWeird, thing);
                job3.count = 1;// FoodUtility.WillIngestStackCountOf(pawn, thingDef, nutrition);
                return job3;
            }
            else
            {
                if ((pawn.Map != null) && comp.Props.digThingIfMapEmpty && (pawn.needs?.food?.CurLevelPercentage < pawn.needs?.food?.PercentageThreshHungry) && (pawn.Awake()))
                {



                    ThingDef newThing = ThingDef.Named(comp.Props.thingToDigIfMapEmpty);
                    Thing newcorpse = null;
                    for (int i = 0; i < comp.Props.customAmountToDig; i++)
                    {
                        newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                    }

                    if (this.effecter == null)
                    {
                        this.effecter = EffecterDefOf.Mine.Spawn();
                    }
                    this.effecter.Trigger(pawn, newcorpse);



                }



            }
            return null;

        }


        public Thing FindWeirdFoodInMap(ThingDef thingDef, Pawn pawn)
        {
            ThingRequest thingReq = ThingRequest.ForDef(thingDef);
            bool ignoreEntirelyForbiddenRegions = ForbidUtility.CaresAboutForbidden(pawn, true) && pawn.playerSettings != null && pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null;
            Thing thingToEat = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, thingReq, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, null, null, 0, -1, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
            if (thingToEat != null && thingToEat.Position.InAllowedArea(pawn))
            {
                return thingToEat;
            }
            else
            {
                return null;
            }


        }


    }
}

