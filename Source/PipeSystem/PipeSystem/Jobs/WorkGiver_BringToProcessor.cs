using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    public class WorkGiver_BringToProcessor : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => CachedAdvancedProcessorsManager.GetFor(pawn.Map).AwaitingIngredients;

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return CachedAdvancedProcessorsManager.GetFor(pawn.Map).AwaitingIngredients.Count == 0;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // TODO: Multiple pawns reservation
            if (t.IsBurning() || t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced) || t.Faction != pawn.Faction || pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                return false;

            var comp = CachedCompAdvancedProcessor.GetFor(t);
            if (comp == null || comp.Process == null)
                return false;

            var firstMissing = comp.FirstIngredientMissing;
            if (firstMissing != null && FindIngredient(pawn, comp, firstMissing) != null)
                return true;

            var firstCategoryMissing = comp.FirstCategoryMissing;
            if (firstCategoryMissing != null && FindCategoryIngredient(pawn, comp, firstCategoryMissing) != null)
                return true;
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var comp = CachedCompAdvancedProcessor.GetFor(t);

            if (comp.FirstIngredientMissing != null)
            {
                Thing ingredient = FindIngredient(pawn, comp, comp.FirstIngredientMissing);
                if(ingredient != null)
                {
                    return JobMaker.MakeJob(PSDefOf.PS_BringToProcessor, t, ingredient);
                }
                
            }
            if (comp.FirstCategoryMissing != null)
            {
                Thing ingredient = FindCategoryIngredient(pawn, comp, comp.FirstCategoryMissing);
                if (ingredient != null)
                {
                    return JobMaker.MakeJob(PSDefOf.PS_BringToProcessor, t, ingredient);
                }

               
            }
            return null;
        }

        private Thing FindIngredient(Pawn pawn, CompAdvancedResourceProcessor comp, ThingDef firstMissing)
        {
            if (firstMissing == null)
            {
                Log.Warning($"Tried to find ingredient for {comp.parent} but none is required.");
                return null;
            }

            bool validator(Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(firstMissing), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }

        private Thing FindCategoryIngredient(Pawn pawn, CompAdvancedResourceProcessor comp, ThingCategoryDef firstCategoryMissing)
        {
            if (firstCategoryMissing == null)
            {
                Log.Warning($"Tried to find category ingredient for {comp.parent} but none is required.");
                return null;
            }

            Predicate<Thing> validator = delegate (Thing x)
            {
                if (comp.ProcessDef.disallowMixing && comp.Process.GetLastStoredIngredient()
                    is ThingDef stored && x.def != stored)
                {
                    return false;
                }
                return !x.IsForbidden(pawn) && pawn.CanReserve(x);
            };

            List<Thing> searchSet = new List<Thing>();
            List<ThingDef> validThingDefs = firstCategoryMissing.childThingDefs.Where(x => comp.Process.Def.ingredients.Where(y => y.thingCategory == firstCategoryMissing).FirstOrFallback()?.disallowedThingDefs?.Contains(x) == false)?.ToList();
            foreach (ThingDef thingDef in validThingDefs)
            {                
                    searchSet.AddRange(pawn.Map.listerThings.ThingsOfDef(thingDef));
            }
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchSet, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }
    }
}