using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Noise;
using static PipeSystem.ProcessDef;

namespace PipeSystem
{
    /// <summary>
    /// Keep processor awaiting pickup and ingredients. Used in workgiver, prevent laggy full map scanning.
    /// </summary>
    public class AdvancedProcessorsManager : MapComponent
    {
        private ProcessIDsManager processIDsManager;

        private List<Thing> awaitingPickup = new List<Thing>();
        private List<Thing> awaitingIngredients = new List<Thing>();

        public List<Thing> AwaitingPickup => awaitingPickup;

        public List<Thing> AwaitingIngredients => awaitingIngredients;

        public ProcessIDsManager ProcessIDsManager => processIDsManager;

        public AdvancedProcessorsManager(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref awaitingPickup, "awaitingPickup", LookMode.Reference);
            Scribe_Collections.Look(ref awaitingIngredients, "awaitingIngredients", LookMode.Reference);

            Scribe_Deep.Look(ref processIDsManager, "uniqueIDsManager");
        }

        public override void FinalizeInit()
        {
            if (processIDsManager == null)
                processIDsManager = new ProcessIDsManager();
            base.FinalizeInit();
        }

        /// <summary>
        /// Add comp parent to awaitingPickup list
        /// </summary>
        /// <param name="comp"></param>
        public void PickupReady(CompAdvancedResourceProcessor comp)
        {
            if (!awaitingPickup.Contains(comp.parent))
                awaitingPickup.Add(comp.parent);
        }

        /// <summary>
        /// Remove comp parent from awaitingPickup list
        /// </summary>
        /// <param name="comp"></param>
        public void PickupDone(CompAdvancedResourceProcessor comp)
        {
            awaitingPickup.Remove(comp.parent);
        }

        /// <summary>
        /// Add comp parent to awaitingIngredients
        /// </summary>
        /// <param name="comp"></param>
        public void SetAwaitingIngredients(CompAdvancedResourceProcessor comp)
        {
            if (!awaitingIngredients.Contains(comp.parent))
                awaitingIngredients.Add(comp.parent);
        }

        /// <summary>
        /// Remove from awaitingIngredients
        /// </summary>
        /// <param name="comp"></param>
        public void RemoveFromAwaiting(CompAdvancedResourceProcessor comp)
        {
            if (awaitingIngredients.Contains(comp.parent))
                awaitingIngredients.Remove(comp.parent);
        }

        /// <summary>
        /// Add thing to matching comp owner. 
        /// Remove from awaitingIngredients if no more ingredient are required.
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="thing"></param>
        public void AddIngredient(CompAdvancedResourceProcessor comp, Thing thing)
        {
            var owner = comp.Process.GetOwnerFor(thing.def);
            if (owner == null)
            {
                owner = comp.Process.GetOwnerForCategory(thing.def.thingCategories);
            }
            owner.AddFromThing(thing);

            if (comp.Process.Def.transfersIngredientList)
            {
                CompIngredients compingredientsInput = thing?.TryGetComp<CompIngredients>();
                if (compingredientsInput != null)
                {
                    foreach(ThingDef ingredient in compingredientsInput.ingredients)
                    {
                        if (!comp.cachedIngredients.Contains(ingredient))
                        {
                            comp.cachedIngredients.Add(ingredient);
                        }
                    }
                    
                }
            }

            if (!owner.Require && awaitingIngredients.Contains(comp.parent)) { 
                awaitingIngredients.Remove(comp.parent);
                comp.Process.Notify_Started();
            }
            owner.BeingFilled = false;
        }
    }
}