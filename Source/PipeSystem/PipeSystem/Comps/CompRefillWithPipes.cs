using ItemProcessor;
using RimWorld;
using System.Linq;
using Verse;

namespace PipeSystem
{
    public class CompRefillWithPipes : CompResource
    {
        public CompRefuelable compRefuelable;
        public Building_ItemProcessor itemProcessor;

        public new CompProperties_RefillWithPipes Props => (CompProperties_RefillWithPipes)props;

        /// <summary>
        /// Try get parent as Building_ItemProcessor. Get CompRefuelable matching Props.thing
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (parent is Building_ItemProcessor processor)
            {
                itemProcessor = processor;
            }

            var comps = parent.GetComps<CompRefuelable>().ToList();
            for (int i = 0; i < comps.Count; i++)
            {
                var comp = comps[i];
                if (comp.Props.fuelFilter.Allows(Props.thing))
                {
                    compRefuelable = comp;
                    break;
                }
            }
        }

        /// <summary>
        /// Refill CompRefuelable or Building_ItemProcessor. Return the amount used
        /// </summary>
        /// <param name="available">Amount available to refuel</param>
        /// <returns>Amount used</returns>
        public float Refill(float available)
        {
            if (itemProcessor != null && itemProcessor.thisRecipe != null && DefDatabase<CombinationDef>.GetNamed(itemProcessor.thisRecipe, false) is CombinationDef def)
            {
                return RefillProcessor(def, available);
            }
            else if (compRefuelable != null)
            {
                var toAdd = compRefuelable.TargetFuelLevel - compRefuelable.Fuel; // The amount of fuel needed by compRefuelable
                var resourceNeeded = toAdd * Props.ratio; // Converted to the amount of resource
                // Check if needed resource is more that available resource
                var resourceCanBeUsed = resourceNeeded < available ? resourceNeeded : available; // Can we spare all of it?
                // Refuel
                compRefuelable.Refuel(resourceCanBeUsed / Props.ratio);
                // Return amount used
                return resourceCanBeUsed;
            }
            return 0f;
        }

        /// <summary>
        /// Refill Building_ItemProcessor slot
        /// </summary>
        /// <param name="def">CombinationDef</param>
        /// <param name="available">Amount available to refuel</param>
        /// <returns>Amount used</returns>
        private int RefillProcessor(CombinationDef def, float available)
        {
            var defName = Props.thing.defName;
            if (def.items != null && def.items.Contains(defName))
            {
                // Get the amount needed
                int thingNeed = itemProcessor.ExpectedAmountFirstIngredient - itemProcessor.CurrentAmountFirstIngredient;
                if (itemProcessor.processorStage != ProcessorStage.Working && thingNeed > 0)
                {
                    // Converted to the amount of resource
                    int resourceNeeded = thingNeed * Props.ratio;
                    // Check if needed resource is more that available resource
                    int resourceUsed = (int)(resourceNeeded < available ? resourceNeeded : available); // Can we spare all of it?
                    // Create the thing
                    Thing thing = ThingMaker.MakeThing(Props.thing);
                    thing.stackCount = resourceUsed / Props.ratio;
                    // Refill the processor using it
                    if (itemProcessor.compItemProcessor.Props.transfersIngredientLists)
                    {
                        if (thing.TryGetComp<CompIngredients>() is CompIngredients ingredientComp)
                        {
                            itemProcessor.ingredients.AddRange(ingredientComp.ingredients);
                        }
                    }
                    itemProcessor.CurrentAmountFirstIngredient += thing.stackCount;
                    if (itemProcessor.ExpectedAmountFirstIngredient != 0)
                    {
                        if (itemProcessor.CurrentAmountFirstIngredient >= itemProcessor.ExpectedAmountFirstIngredient)
                        {
                            itemProcessor.firstIngredientComplete = true;
                        }
                    }
                    itemProcessor.TryAcceptFirst(thing);
                    itemProcessor.Notify_StartProcessing();
                    // Return amount used
                    return resourceUsed;
                }
            }
            if (def.secondItems != null && def.secondItems.Contains(defName))
            {
                // Get the amount needed
                int thingNeed = itemProcessor.ExpectedAmountSecondIngredient - itemProcessor.CurrentAmountSecondIngredient;
                if (itemProcessor.processorStage != ProcessorStage.Working && thingNeed > 0)
                {
                    // Converted to the amount of resource
                    int resourceNeeded = thingNeed * Props.ratio;
                    // Check if needed resource is more that available resource
                    int resourceUsed = (int)(resourceNeeded < available ? resourceNeeded : available); // Can we spare all of it?
                    // Create the thing
                    Thing thing = ThingMaker.MakeThing(Props.thing);
                    thing.stackCount = resourceUsed / Props.ratio;
                    // Refill the processor using it
                    if (itemProcessor.compItemProcessor.Props.transfersIngredientLists)
                    {
                        if (thing.TryGetComp<CompIngredients>() is CompIngredients ingredientComp)
                        {
                            itemProcessor.ingredients.AddRange(ingredientComp.ingredients);
                        }
                    }
                    itemProcessor.CurrentAmountSecondIngredient += thing.stackCount;
                    if (itemProcessor.ExpectedAmountSecondIngredient != 0)
                    {
                        if (itemProcessor.CurrentAmountSecondIngredient >= itemProcessor.ExpectedAmountSecondIngredient)
                        {
                            itemProcessor.secondIngredientComplete = true;
                        }
                    }
                    itemProcessor.TryAcceptSecond(thing);
                    itemProcessor.Notify_StartProcessing();
                    // Return amount used
                    return resourceUsed;
                }
            }
            if (def.thirdItems != null && def.thirdItems.Contains(defName))
            {
                // Get the amount needed
                int thingNeed = itemProcessor.ExpectedAmountThirdIngredient - itemProcessor.CurrentAmountThirdIngredient;
                if (itemProcessor.processorStage != ProcessorStage.Working && thingNeed > 0)
                {
                    // Converted to the amount of resource
                    int resourceNeeded = thingNeed * Props.ratio;
                    // Check if needed resource is more that available resource
                    int resourceUsed = (int)(resourceNeeded < available ? resourceNeeded : available); // Can we spare all of it?
                    // Create the thing
                    Thing thing = ThingMaker.MakeThing(Props.thing);
                    thing.stackCount = resourceUsed / Props.ratio;
                    // Refill the processor using it
                    if (itemProcessor.compItemProcessor.Props.transfersIngredientLists)
                    {
                        if (thing.TryGetComp<CompIngredients>() is CompIngredients ingredientComp)
                        {
                            itemProcessor.ingredients.AddRange(ingredientComp.ingredients);
                        }
                    }
                    itemProcessor.CurrentAmountThirdIngredient += thing.stackCount;
                    if (itemProcessor.ExpectedAmountThirdIngredient != 0)
                    {
                        if (itemProcessor.CurrentAmountThirdIngredient >= itemProcessor.ExpectedAmountThirdIngredient)
                        {
                            itemProcessor.thirdIngredientComplete = true;
                        }
                    }
                    itemProcessor.TryAcceptThird(thing);
                    itemProcessor.Notify_StartProcessing();
                    // Return amount used
                    return resourceUsed;
                }
            }
            if (def.fourthItems != null && def.fourthItems.Contains(defName))
            {
                // Get the amount needed
                int thingNeed = itemProcessor.ExpectedAmountFourthIngredient - itemProcessor.CurrentAmountFourthIngredient;
                if (itemProcessor.processorStage != ProcessorStage.Working && thingNeed > 0)
                {
                    // Converted to the amount of resource
                    int resourceNeeded = thingNeed * Props.ratio;
                    // Check if needed resource is more that available resource
                    int resourceUsed = (int)(resourceNeeded < available ? resourceNeeded : available); // Can we spare all of it?
                    // Create the thing
                    Thing thing = ThingMaker.MakeThing(Props.thing);
                    thing.stackCount = resourceUsed / Props.ratio;
                    // Refill the processor using it
                    if (itemProcessor.compItemProcessor.Props.transfersIngredientLists)
                    {
                        if (thing.TryGetComp<CompIngredients>() is CompIngredients ingredientComp)
                        {
                            itemProcessor.ingredients.AddRange(ingredientComp.ingredients);
                        }
                    }
                    itemProcessor.CurrentAmountFourthIngredient += thing.stackCount;
                    if (itemProcessor.ExpectedAmountFourthIngredient != 0)
                    {
                        if (itemProcessor.CurrentAmountFourthIngredient >= itemProcessor.ExpectedAmountFourthIngredient)
                        {
                            itemProcessor.fourthIngredientComplete = true;
                        }
                    }
                    itemProcessor.TryAcceptFourth(thing);
                    itemProcessor.Notify_StartProcessing();
                    // Return amount used
                    return resourceUsed;
                }
            }
            return 0;
        }
    }
}
