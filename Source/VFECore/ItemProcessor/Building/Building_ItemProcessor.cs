

using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Diagnostics;
using Verse.Sound;

using System.Reflection;
using System.Text;
using HarmonyLib;

namespace ItemProcessor
{

    //This is the mod's main class, and it handles all the heavy stuff

    public class Building_ItemProcessor : Building, IThingHolder
    {

        //Storing components. These are initialized on the constructor, and they'll be reinitialized after loading a game
        //Note that compPowerTrader and compFuelable may be null
        public CompPowerTrader compPowerTrader;
        public CompItemProcessor compItemProcessor;
        public CompRefuelable compFuelable;

        //The stage at which this item processor is. This variable controls the flow of the whole process
        public ProcessorStage processorStage = ProcessorStage.Inactive;

        //A variable to store the current recipe's defName (recipes defined in CombinationDef)
        public string thisRecipe;

        //Ingredient Containers. Ingredients are stored inside the building so if it is destroyed the ingredients pop up
        //When exactly they get destroyed is configurable through CompItemProcessor
        public ThingOwner innerContainerFirst = null;
        public ThingOwner innerContainerSecond = null;
        public ThingOwner innerContainerThird = null;
        public ThingOwner innerContainerFourth = null;

        //Variables to store the defNames of the ingredients
        public string firstItem = "";
        public string secondItem = "";
        public string thirdItem = "";
        public string fourthItem = "";

        //Variables to store the defNames of the ingredients' categories in case of isCategoryBuilding 
        public string firstCategory = "";
        public string secondCategory = "";
        public string thirdCategory = "";
        public string fourthCategory = "";
        public string firstItemSwallowedForButchery = "";

        //A List to store the ingredients as... well, ingredients for the product
        //If the product doesn't allow ingredients (eg. Steel) this is ignored
        public List<ThingDef> ingredients = new List<ThingDef>();

        //These are just counters for the Tick method. Can this be done with Tick.Manager? Yes
        //But I don't like it, so I use my own
        //This only increase when processorStage = ProcessorStage.Working
        public int progressCounter = 0;
        public int noPowerDestructionCounter = 0;
        public int noGoodLightDestructionCounter = 0;
        public int noGoodWeatherDestructionCounter = 0;
        public int noGoodTempDestructionCounter = 0;


        //Some constants. Most machines only use tickerType Rare to reduce possible lag, so
        //they update aprox every 4 seconds
        public const int rareTicksPerDay = 240;
        public const int ticksPerDay = 60000;

        //Variables extracted from the recipe
        public string productToTurnInto = "";
        public int amount = 0;
        public float days = 0;
        //If the recipe has differentProductsByQuality, the productsToTurnInto need to be stored in an array
        public List<string> productsToTurnInto = new List<string>();


        //These variables control how many of each ingredient is needed in total, and how many
        //has been already inserted
        public int CurrentAmountFirstIngredient = 0;
        public int ExpectedAmountFirstIngredient = 0;
        public int CurrentAmountSecondIngredient = 0;
        public int ExpectedAmountSecondIngredient = 0;
        public int CurrentAmountThirdIngredient = 0;
        public int ExpectedAmountThirdIngredient = 0;
        public int CurrentAmountFourthIngredient = 0;
        public int ExpectedAmountFourthIngredient = 0;
        public bool firstIngredientComplete = false;
        public bool secondIngredientComplete = false;
        public bool thirdIngredientComplete = false;
        public bool fourthIngredientComplete = false;

        //If the product uses quality increasing, these are the variables storing the number of days for each quality
        //The two last variables are used for auto-quality
        public bool usingQualityIncreasing = true;
        public float awfulQualityAgeDaysThreshold;
        public float poorQualityAgeDaysThreshold;
        public float normalQualityAgeDaysThreshold;
        public float goodQualityAgeDaysThreshold;
        public float excellentQualityAgeDaysThreshold;
        public float masterworkQualityAgeDaysThreshold;
        public float legendaryQualityAgeDaysThreshold;
        public QualityCategory qualityNow = QualityCategory.Awful;
        public bool qualityEstablished = false;
        public QualityCategory qualityRequested = QualityCategory.Awful;

        //This is used for machines that have auto mode. A gizmo toggles this variable
        public bool isAutoEnabled = true;

        //This is used for machines that have semiauto mode. A gizmo toggles this variable
        public bool isSemiAutoEnabled = false;

        //This variable signals the product has reached Awful quality and can be removed from the building
        //Obviously unused if usingQualityIncreasing is false
        public bool removeAfterAwful = false;

        //Cached adjacent cells to look for Hoppers in the case of auto mode
        private List<IntVec3> cachedAdjCellsCardinal;

        //A flag to only send the power warning message once
        public bool onlySendWarningMessageOnce = false;
        //A flag to only send the light warning message once
        public bool onlySendLightWarningMessageOnce = false;
        //A flag to only send the rain warning message once
        public bool onlySendRainWarningMessageOnce = false;
        //A flag to only send the temperature warning message once
        public bool onlySendTempWarningMessageOnce = false;

        //Cached progress bar material
        private Material barFilledCachedMat;
        //Size of the progress bar
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);
        //This variable is used to pass a numerical value of the building's progress when working. It is only
        //used to interpolate the colour of the progress bar
        private float progressInt;

        //Pause control for isMachinePausable processors
        public bool isPaused = false;

        //This is used for recipes that have their output measured so they don't output 45657564 Steel, for example
        bool OverThreshold = false;




        public Building_ItemProcessor()
        {
            //Constructor initializes the ingredient containers
            this.innerContainerFirst = new ThingOwner<Thing>(this, false, LookMode.Deep);
            this.innerContainerSecond = new ThingOwner<Thing>(this, false, LookMode.Deep);
            this.innerContainerThird = new ThingOwner<Thing>(this, false, LookMode.Deep);
            this.innerContainerFourth = new ThingOwner<Thing>(this, false, LookMode.Deep);

        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            //Initialize the components
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
            this.compItemProcessor = base.GetComp<CompItemProcessor>();
            this.compFuelable = base.GetComp<CompRefuelable>();

        }

        public override void ExposeData()
        {
            //Save all the key variables so they work on game save / load
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainerFirst, "innerContainerFirst", new object[] { this });
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainerSecond, "innerContainerSecond", new object[] { this });
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainerThird, "innerContainerThird", new object[] { this });
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainerFourth, "innerContainerFourth", new object[] { this });

            Scribe_Values.Look<ProcessorStage>(ref this.processorStage, "processorStage", ProcessorStage.Inactive, false);

            Scribe_Values.Look<string>(ref this.firstItem, "firstItem", "", false);
            Scribe_Values.Look<string>(ref this.secondItem, "secondItem", "", false);
            Scribe_Values.Look<string>(ref this.thirdItem, "thirdItem", "", false);
            Scribe_Values.Look<string>(ref this.fourthItem, "fourthItem", "", false);

            Scribe_Values.Look<string>(ref this.firstCategory, "firstCategory", "", false);
            Scribe_Values.Look<string>(ref this.secondCategory, "secondCategory", "", false);
            Scribe_Values.Look<string>(ref this.thirdCategory, "thirdCategory", "", false);
            Scribe_Values.Look<string>(ref this.fourthCategory, "fourthCategory", "", false);
            Scribe_Values.Look<string>(ref this.firstItemSwallowedForButchery, "firstItemSwallowedForButchery", "", false);

            Scribe_Collections.Look<ThingDef>(ref this.ingredients, "ingredients");
            Scribe_Values.Look<int>(ref this.progressCounter, "progressCounter", 0, false);
            Scribe_Values.Look<int>(ref this.noPowerDestructionCounter, "noPowerDestructionCounter", 0, false);
            Scribe_Values.Look<int>(ref this.noGoodLightDestructionCounter, "noGoodLightDestructionCounter", 0, false);
            Scribe_Values.Look<int>(ref this.noGoodWeatherDestructionCounter, "noGoodWeatherDestructionCounter", 0, false);
            Scribe_Values.Look<int>(ref this.noGoodTempDestructionCounter, "noGoodTempDestructionCounter", 0, false);
            Scribe_Values.Look<string>(ref this.productToTurnInto, "productToTurnInto", "", false);
            Scribe_Collections.Look<string>(ref this.productsToTurnInto, "productsToTurnInto");
            Scribe_Values.Look<int>(ref this.amount, "amount", 0, false);
            Scribe_Values.Look<float>(ref this.days, "days", 0, false);
            Scribe_Values.Look<int>(ref this.CurrentAmountFirstIngredient, "CurrentAmountFirstIngredient", 0, false);
            Scribe_Values.Look<int>(ref this.ExpectedAmountFirstIngredient, "ExpectedAmountFirstIngredient", 0, false);
            Scribe_Values.Look<int>(ref this.CurrentAmountSecondIngredient, "CurrentAmountSecondIngredient", 0, false);
            Scribe_Values.Look<int>(ref this.ExpectedAmountSecondIngredient, "ExpectedAmountSecondIngredient", 0, false);
            Scribe_Values.Look<int>(ref this.CurrentAmountThirdIngredient, "CurrentAmountThirdIngredient", 0, false);
            Scribe_Values.Look<int>(ref this.ExpectedAmountThirdIngredient, "ExpectedAmountThirdIngredient", 0, false);
            Scribe_Values.Look<int>(ref this.CurrentAmountFourthIngredient, "CurrentAmountFourthIngredient", 0, false);
            Scribe_Values.Look<int>(ref this.ExpectedAmountFourthIngredient, "ExpectedAmountFourthIngredient", 0, false);
            Scribe_Values.Look<bool>(ref this.usingQualityIncreasing, "usingQualityIncreasing", true, false);
            Scribe_Values.Look<float>(ref this.awfulQualityAgeDaysThreshold, "awfulQualityAgeDaysThreshold", 0f, false);
            Scribe_Values.Look<float>(ref this.poorQualityAgeDaysThreshold, "poorQualityAgeDaysThreshold", 0f, false);
            Scribe_Values.Look<float>(ref this.normalQualityAgeDaysThreshold, "normalQualityAgeDaysThreshold", 0f, false);
            Scribe_Values.Look<float>(ref this.goodQualityAgeDaysThreshold, "goodQualityAgeDaysThreshold", 0f, false);
            Scribe_Values.Look<float>(ref this.excellentQualityAgeDaysThreshold, "excellentQualityAgeDaysThreshold", 0f, false);
            Scribe_Values.Look<float>(ref this.masterworkQualityAgeDaysThreshold, "masterworkQualityAgeDaysThreshold", 0f, false);
            Scribe_Values.Look<float>(ref this.legendaryQualityAgeDaysThreshold, "legendaryQualityAgeDaysThreshold", 0f, false);
            Scribe_Values.Look(ref qualityNow, "qualityNow", QualityCategory.Awful);
            Scribe_Values.Look(ref qualityRequested, "qualityRequested", QualityCategory.Awful);
            Scribe_Values.Look<bool>(ref this.qualityEstablished, "qualityEstablished", false, false);
            Scribe_Values.Look<bool>(ref this.removeAfterAwful, "removeAfterAwful", false, false);

            Scribe_Values.Look<bool>(ref this.firstIngredientComplete, "firstIngredientComplete", false, false);
            Scribe_Values.Look<bool>(ref this.secondIngredientComplete, "secondIngredientComplete", false, false);
            Scribe_Values.Look<bool>(ref this.thirdIngredientComplete, "thirdIngredientComplete", false, false);
            Scribe_Values.Look<bool>(ref this.fourthIngredientComplete, "fourthIngredientComplete", false, false);
            Scribe_Values.Look<bool>(ref this.isAutoEnabled, "isAutoEnabled", true, false);
            Scribe_Values.Look<bool>(ref this.isSemiAutoEnabled, "isSemiAutoEnabled", false, false);
            Scribe_Values.Look<string>(ref this.thisRecipe, "thisRecipe", null, false);
            Scribe_Values.Look<bool>(ref this.onlySendWarningMessageOnce, "onlySendWarningMessageOnce", false, false);
            Scribe_Values.Look<bool>(ref this.onlySendLightWarningMessageOnce, "onlySendLightWarningMessageOnce", false, false);
            Scribe_Values.Look<bool>(ref this.onlySendRainWarningMessageOnce, "onlySendRainWarningMessageOnce", false, false);
            Scribe_Values.Look<bool>(ref this.onlySendTempWarningMessageOnce, "onlySendTempWarningMessageOnce", false, false);

            Scribe_Values.Look<bool>(ref this.isPaused, "isPaused", false, false);
            Scribe_Values.Look<bool>(ref this.OverThreshold, "OverThreshold", false, false);


        }

        public ThingOwner GetDirectlyHeldThings()
        {
            //Not used, included just in case something external calls it
            return this.innerContainerFirst;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            //Not used, included just in case something external calls it
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public virtual void EjectContentsFirst()
        {
            //Remove ingredients from the first ingredient container. Call MapMeshDirty so the building graphic changes (if needed)
            this.innerContainerFirst?.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, null);
            base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);

        }
        public virtual void EjectContentsSecond()
        {
            //Remove ingredients from the second ingredient container. Call MapMeshDirty so the building graphic changes (if needed)
            this.innerContainerSecond?.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, null);
            base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);

        }
        public virtual void EjectContentsThird()
        {
            //Remove ingredients from the third ingredient container. Call MapMeshDirty so the building graphic changes (if needed)
            this.innerContainerThird?.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, null);
            base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);

        }

        public virtual void EjectContentsFourth()
        {
            //Remove ingredients from the third ingredient container. Call MapMeshDirty so the building graphic changes (if needed)

            this.innerContainerFourth?.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, null);
            base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);

        }

        public virtual void ResetEverything()
        {
            //Resets everything that needs resetting
            Progress = 0;
            isPaused = false;
            EjectContentsFirst();
            EjectContentsSecond();
            EjectContentsThird();
            EjectContentsFourth();
            CurrentAmountFirstIngredient = 0;
            CurrentAmountSecondIngredient = 0;
            CurrentAmountThirdIngredient = 0;
            CurrentAmountFourthIngredient = 0;
            usingQualityIncreasing = true;
            firstIngredientComplete = false;
            secondIngredientComplete = false;
            thirdIngredientComplete = false;
            fourthIngredientComplete = false;
            firstItemSwallowedForButchery = "";
            progressCounter = 0;
            noPowerDestructionCounter = 0;
            noGoodLightDestructionCounter = 0;
            noGoodWeatherDestructionCounter = 0;
            noGoodTempDestructionCounter = 0;
            removeAfterAwful = false;
            onlySendWarningMessageOnce = false;
            onlySendLightWarningMessageOnce = false;
            onlySendRainWarningMessageOnce = false;
            onlySendTempWarningMessageOnce = false;
            qualityNow = QualityCategory.Awful;
            ingredients = new List<ThingDef>();

            //If the machine is automatic, it will continue grabbing ingredients from Hoppers
            //Unless the player toggles the auto gizmo


            if (compItemProcessor.Props.isAutoMachine && isAutoEnabled)
            {
                processorStage = ProcessorStage.AutoIngredients;
            }
            else if (compItemProcessor.Props.isSemiAutomaticMachine && !isSemiAutoEnabled)
            {
                IngredientsChosenBringThemIn();
            }
            else
            {
                thisRecipe = null;
                processorStage = ProcessorStage.Inactive;
                ExpectedAmountFirstIngredient = 0;
                ExpectedAmountSecondIngredient = 0;
                ExpectedAmountThirdIngredient = 0;
                ExpectedAmountFourthIngredient = 0;
                productToTurnInto = "";
                productsToTurnInto = new List<string>();
                isSemiAutoEnabled = false;
                awfulQualityAgeDaysThreshold = 0;
                poorQualityAgeDaysThreshold = 0;
                normalQualityAgeDaysThreshold = 0;
                goodQualityAgeDaysThreshold = 0;
                excellentQualityAgeDaysThreshold = 0;
                masterworkQualityAgeDaysThreshold = 0;
                legendaryQualityAgeDaysThreshold = 0;
                firstItem = "";
                secondItem = "";
                thirdItem = "";
                fourthItem = "";
                firstCategory = "";
                secondCategory = "";
                thirdCategory = "";
                fourthCategory = "";





            }
            base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);

        }

        public void DestroyIngredients()
        {
            //Empties all ingredient containers and destroys ingredients
            //When exactly they get destroyed is configurable through CompItemProcessor
            if (this.innerContainerFirst != null && this.innerContainerFirst.Any)
            {
                this.innerContainerFirst.ClearAndDestroyContents();
            }
            if (this.innerContainerSecond != null && this.innerContainerSecond.Any)
            {
                this.innerContainerSecond.ClearAndDestroyContents();
            }
            if (this.innerContainerThird != null && this.innerContainerThird.Any)
            {
                this.innerContainerThird.ClearAndDestroyContents();
            }
            if (this.innerContainerFourth != null && this.innerContainerFourth.Any)
            {
                this.innerContainerFourth.ClearAndDestroyContents();
            }

        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            //If the building is destroyed, ingredients pop up
            //Unless they were destroyed by the process first, of course
            EjectContentsFirst();
            EjectContentsSecond();
            EjectContentsThird();
            EjectContentsFourth();

            base.Destroy(mode);
        }

        public bool TryAcceptFirst(Thing thing, int Count = 0, bool allowSpecialEffects = true)
        {
            //Inserts and item in innerContainerFirst
            //These are copied from vanilla classes, with one exception. Count is used on auto mode, and the method uses
            //count to split the necessary amounts from item stacks in nearby Hoppers. If splitOff isn't used the
            //game complains about trying to insert items directly from the map
            //Log.Message(thing.def.defName);
            bool result;
            bool flag;
            if (thing.holdingOwner != null)
            {
                if (Count == 0)
                {
                    thing.holdingOwner.TryTransferToContainer(thing, this.innerContainerFirst, thing.stackCount, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    this.innerContainerFirst.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                flag = true;
            }
            else
            {
                if (Count == 0)
                {
                    flag = this.innerContainerFirst.TryAdd(thing, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    flag = this.innerContainerFirst.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
            }
            if (compItemProcessor.Props.isCategoryBuilding || (thisRecipe != null && DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isCategoryRecipe))
            {
                firstItem = thing.def.thingCategories.FirstOrDefault().ToString();
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                }
                result = true;
            }
            else
            {
                result = false;
            }
            this.TickRare();
            return result;
        }

        public bool TryAcceptSecond(Thing thing, int Count = 0, bool allowSpecialEffects = true)
        {
            //Inserts and item in innerContainerSecond
            bool result;
            //Log.Message(thing.def.defName);
            bool flag;
            if (thing.holdingOwner != null)
            {
                if (Count == 0)
                {
                    thing.holdingOwner.TryTransferToContainer(thing, this.innerContainerSecond, thing.stackCount, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    this.innerContainerSecond.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                flag = true;
            }
            else
            {
                if (Count == 0)
                {
                    flag = this.innerContainerSecond.TryAdd(thing, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    flag = this.innerContainerSecond.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
            }
            if (compItemProcessor.Props.isCategoryBuilding || (thisRecipe != null && DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isCategoryRecipe))
            {
                secondItem = thing.def.thingCategories.FirstOrDefault().ToString();
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                }
                result = true;
            }
            else
            {
                result = false;
            }
            this.TickRare();
            return result;
        }

        public bool TryAcceptThird(Thing thing, int Count = 0, bool allowSpecialEffects = true)
        {
            //Inserts and item in innerContainerThird

            bool result;

            bool flag;
            if (thing.holdingOwner != null)
            {
                if (Count == 0)
                {
                    thing.holdingOwner.TryTransferToContainer(thing, this.innerContainerThird, thing.stackCount, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    this.innerContainerThird.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                flag = true;
            }
            else
            {
                if (Count == 0)
                {
                    flag = this.innerContainerThird.TryAdd(thing, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    flag = this.innerContainerThird.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
            }
            if (compItemProcessor.Props.isCategoryBuilding || (thisRecipe != null && DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isCategoryRecipe))
            {
                thirdItem = thing.def.thingCategories.FirstOrDefault().ToString();
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                }
                result = true;
            }
            else
            {
                result = false;
            }
            this.TickRare();
            return result;
        }

        public bool TryAcceptFourth(Thing thing, int Count = 0, bool allowSpecialEffects = true)
        {
            //Inserts an item in innerContainerFourth

            bool result;

            bool flag;
            if (thing.holdingOwner != null)
            {
                if (Count == 0)
                {
                    thing.holdingOwner.TryTransferToContainer(thing, this.innerContainerFourth, thing.stackCount, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    this.innerContainerFourth.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                flag = true;
            }
            else
            {
                if (Count == 0)
                {
                    flag = this.innerContainerFourth.TryAdd(thing, true);
                }
                else
                {
                    Thing newThing = thing.SplitOff(Count);
                    flag = this.innerContainerFourth.TryAdd(newThing, true);
                }
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
            }
            if (compItemProcessor.Props.isCategoryBuilding || (thisRecipe != null && DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isCategoryRecipe))
            {
                fourthItem = thing.def.thingCategories.FirstOrDefault().ToString();
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                }
                result = true;
            }
            else
            {
                result = false;
            }
            this.TickRare();
            return result;
        }

        //The core of this class. Gizmos control the flow of the process. Ceratin gizmos will only be active on certain stages
        //while others are activated or not depending on configured options on CompProperties_ItemProcessor

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {

            //First, get the base building gizmos (copy, desconstruct, etc)
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            //If more than one processor is selected, detect if they are in the same stage. If not, no gizmos
            bool allProcessorsInSameStage = true;
            ProcessorStage stageToDetect = ProcessorStage.Invalid;
            ThingDef processorDef = null;
            List<object> selectedObjects = Find.Selector.SelectedObjects;
            foreach (object selectedObject in selectedObjects)
            {
                Building_ItemProcessor processor = selectedObject as Building_ItemProcessor;
                if (processor != null)
                {

                    if (processorDef == null)
                    {
                        processorDef = processor.def;
                    }
                    else if (processor.def != processorDef)
                    {
                        allProcessorsInSameStage = false;
                    }


                    if (stageToDetect == ProcessorStage.Invalid) { 
                        stageToDetect = processor.processorStage; 
                    } else if(processor.processorStage!= stageToDetect)
                    {
                        allProcessorsInSameStage = false;
                    }
                }

            }

           
            if (allProcessorsInSameStage)
            {

                //Ingredient selection gizmos only appear when the building is in Inactive or IngredientsChosen, the first because
                //it is the baseline of the building, the second because player might change his mind and want to change one of the ingredients
                if ((!compItemProcessor.Props.isSemiAutomaticMachine && processorStage <= ProcessorStage.ExpectingIngredients && processorStage != ProcessorStage.AutoIngredients && !compItemProcessor.Props.isMachineSpecifiesOutput)||
                    compItemProcessor.Props.isSemiAutomaticMachine && processorStage == ProcessorStage.Inactive)
                {
                    //Different number of ingredients gizmos depending on the number of building slots
                    switch (compItemProcessor.Props.numberOfInputs)
                    {
                        case 1:
                            yield return ItemListSetupUtility.SetFirstItemListCommand(this, this.Map, compItemProcessor.Props.InsertFirstItemDesc);
                            break;
                        case 2:
                            yield return ItemListSetupUtility.SetFirstItemListCommand(this, this.Map, compItemProcessor.Props.InsertFirstItemDesc);
                            yield return ItemListSetupUtility.SetSecondItemListCommand(this, this.Map, compItemProcessor.Props.InsertSecondItemDesc);
                            break;
                        case 3:
                            yield return ItemListSetupUtility.SetFirstItemListCommand(this, this.Map, compItemProcessor.Props.InsertFirstItemDesc);
                            yield return ItemListSetupUtility.SetSecondItemListCommand(this, this.Map, compItemProcessor.Props.InsertSecondItemDesc);
                            yield return ItemListSetupUtility.SetThirdItemListCommand(this, this.Map, compItemProcessor.Props.InsertThirdItemDesc);
                            break;
                        case 4:
                            yield return ItemListSetupUtility.SetFirstItemListCommand(this, this.Map, compItemProcessor.Props.InsertFirstItemDesc);
                            yield return ItemListSetupUtility.SetSecondItemListCommand(this, this.Map, compItemProcessor.Props.InsertSecondItemDesc);
                            yield return ItemListSetupUtility.SetThirdItemListCommand(this, this.Map, compItemProcessor.Props.InsertThirdItemDesc);
                            yield return ItemListSetupUtility.SetFourthItemListCommand(this, this.Map, compItemProcessor.Props.InsertFourthItemDesc);

                            break;
                        default:
                            yield return ItemListSetupUtility.SetFirstItemListCommand(this, this.Map, compItemProcessor.Props.InsertFirstItemDesc);
                            break;
                    }


                }

                if (processorStage <= ProcessorStage.ExpectingIngredients && processorStage != ProcessorStage.AutoIngredients && compItemProcessor.Props.isMachineSpecifiesOutput)
                {
                    yield return ItemListSetupUtility.SetOutputListCommand(this, this.Map, compItemProcessor.Props.InsertFirstItemDesc);


                }

                if (processorStage == ProcessorStage.IngredientsChosen) //This stage is set in the classes in Command_ItemLists
                {
                    //If at least one of the ingredients was selected...
                    Command_Action IP_Gizmo_StartInsertion = new Command_Action();
                    IP_Gizmo_StartInsertion.action = delegate
                    {
                        IngredientsChosenBringThemIn();

                    };
                    IP_Gizmo_StartInsertion.defaultLabel = compItemProcessor.Props.bringIngredientsText.Translate();
                    IP_Gizmo_StartInsertion.defaultDesc = compItemProcessor.Props.bringIngredientsDesc.Translate();
                    IP_Gizmo_StartInsertion.icon = ContentFinder<Texture2D>.Get(compItemProcessor.Props.bringIngredientsIcon, true);
                    yield return IP_Gizmo_StartInsertion;
                }
                //This gizmo sets the current semi-automated recipe to be cancelled after the current product has finished
                if (processorStage == ProcessorStage.Working && compItemProcessor.Props.isSemiAutomaticMachine)
                {
                    Command_Toggle IP_Gizmo_ToggleSemiAuto = new Command_Toggle();
                    IP_Gizmo_ToggleSemiAuto.defaultLabel = compItemProcessor.Props.resetSemiautomaticText.Translate();
                    IP_Gizmo_ToggleSemiAuto.defaultDesc = compItemProcessor.Props.resetSemiautomaticDesc.Translate();
                    IP_Gizmo_ToggleSemiAuto.icon = ContentFinder<Texture2D>.Get(compItemProcessor.Props.resetSemiautomaticIcon, true);
                    IP_Gizmo_ToggleSemiAuto.isActive = (() => this.isSemiAutoEnabled);
                    Command_Toggle IP_Gizmo_ToggleSemiAuto2 = IP_Gizmo_ToggleSemiAuto;
                    IP_Gizmo_ToggleSemiAuto2.toggleAction = (Action)Delegate.Combine(IP_Gizmo_ToggleSemiAuto2.toggleAction, new Action(delegate ()
                    {
                        this.isSemiAutoEnabled = !this.isSemiAutoEnabled;
                    }));
                    yield return IP_Gizmo_ToggleSemiAuto;
                }
                //This gizmo cancels either ingredient bringing or automatic grabbing from Hoppers
                if ((processorStage == ProcessorStage.ExpectingIngredients || processorStage == ProcessorStage.AutoIngredients) && !compItemProcessor.Props.isCompletelyAutoMachine)
                {
                    Command_Action IP_Gizmo_CancelJobs = new Command_Action();
                    IP_Gizmo_CancelJobs.action = delegate
                    {
                        //We set these two variables so auto and semiauto buildings know they should drop the recipe
                        isAutoEnabled = false;
                        isSemiAutoEnabled = true;
                        //And the we cancel by brute force resetting everything
                        ResetEverything();
                    };
                    IP_Gizmo_CancelJobs.defaultLabel = compItemProcessor.Props.cancelIngredientsText.Translate();
                    IP_Gizmo_CancelJobs.defaultDesc = compItemProcessor.Props.cancelIngredientsDesc.Translate();
                    IP_Gizmo_CancelJobs.icon = ContentFinder<Texture2D>.Get(compItemProcessor.Props.cancelIngredientsIcon, true);
                    yield return IP_Gizmo_CancelJobs;
                }



                //This gizmo appears when the machines is working, using quality increasing, and the removeAfterAwful flag has been set
                //The flag is set only after awful quality has been reached
                if (processorStage == ProcessorStage.Working && usingQualityIncreasing && removeAfterAwful)
                {
                    Command_Action IP_Gizmo_RemoveProduct = new Command_Action();
                    IP_Gizmo_RemoveProduct.action = delegate
                    {
                        //Stops working counters, and sets building to Finished so the product can be removed by a colonist
                        progressCounter = 0;
                        processorStage = ProcessorStage.Finished;
                        base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                    };
                    if (productsToTurnInto != null && productsToTurnInto.Count > 0)
                    {
                        IP_Gizmo_RemoveProduct.defaultLabel = compItemProcessor.Props.removeProductText.Translate(ThingDef.Named(productsToTurnInto[(int)qualityNow]).LabelCap, qualityNow.ToString());
                        IP_Gizmo_RemoveProduct.defaultDesc = compItemProcessor.Props.removeProductDesc.Translate(ThingDef.Named(productsToTurnInto[(int)qualityNow]).LabelCap, this.def.LabelCap, qualityNow.ToString());
                    }
                    else
                    {
                        IP_Gizmo_RemoveProduct.defaultLabel = compItemProcessor.Props.removeProductText.Translate(ThingDef.Named(productToTurnInto).LabelCap, qualityNow.ToString());
                        IP_Gizmo_RemoveProduct.defaultDesc = compItemProcessor.Props.removeProductDesc.Translate(ThingDef.Named(productToTurnInto).LabelCap, this.def.LabelCap, qualityNow.ToString());
                    }

                    IP_Gizmo_RemoveProduct.icon = ContentFinder<Texture2D>.Get(compItemProcessor.Props.removeProductIcon, true);
                    yield return IP_Gizmo_RemoveProduct;
                }
                //If a desired quality can be selected, show the quality selection gizmo
                if (processorStage <= ProcessorStage.IngredientsChosen && compItemProcessor.Props.qualitySelector)
                {
                    yield return ItemListSetupUtility.SetQualityListCommand(this, this.Map);
                }
                //If building accepts auto mode, show the auto mode toggle. This toggles isAutoEnabled variable
                if (compItemProcessor.Props.isAutoMachine && !compItemProcessor.Props.isCompletelyAutoMachine)
                {
                    Command_Toggle IP_Gizmo_ToggleAuto = new Command_Toggle();
                    IP_Gizmo_ToggleAuto.defaultLabel = "IP_ToggleAuto".Translate();
                    IP_Gizmo_ToggleAuto.defaultDesc = "IP_ToggleAutoDesc".Translate();
                    IP_Gizmo_ToggleAuto.icon = ContentFinder<Texture2D>.Get("UI/IP_IngredientAuto", true);
                    IP_Gizmo_ToggleAuto.isActive = (() => this.isAutoEnabled);
                    Command_Toggle IP_Gizmo_ToggleAuto2 = IP_Gizmo_ToggleAuto;
                    IP_Gizmo_ToggleAuto2.toggleAction = (Action)Delegate.Combine(IP_Gizmo_ToggleAuto2.toggleAction, new Action(delegate ()
                    {
                        this.isAutoEnabled = !this.isAutoEnabled;
                    }));
                    yield return IP_Gizmo_ToggleAuto;
                }

                //If building is pausable, show the paused toggle. This allows the player to pause and unpause the machine
                if (processorStage == ProcessorStage.Working && compItemProcessor.Props.isMachinePausable)
                {
                    Command_Toggle IP_Gizmo_TogglePause = new Command_Toggle();
                    IP_Gizmo_TogglePause.defaultLabel = "IP_TogglePause".Translate();
                    IP_Gizmo_TogglePause.defaultDesc = "IP_TogglePauseDesc".Translate();
                    if (this.isPaused)
                    {
                        IP_Gizmo_TogglePause.icon = ContentFinder<Texture2D>.Get("UI/IP_MachineOn", true);
                    }
                    else
                    {
                        IP_Gizmo_TogglePause.icon = ContentFinder<Texture2D>.Get("UI/IP_MachineOff", true);
                    }
                    IP_Gizmo_TogglePause.isActive = (() => !this.isPaused);
                    Command_Toggle IP_Gizmo_TogglePause2 = IP_Gizmo_TogglePause;
                    IP_Gizmo_TogglePause2.toggleAction = (Action)Delegate.Combine(IP_Gizmo_TogglePause2.toggleAction, new Action(delegate ()
                    {
                        this.isPaused = !this.isPaused;
                    }));
                    yield return IP_Gizmo_TogglePause;
                }

                //This gizmo only appears in dev mode, for testing purposes. It resets all variables

                if (Prefs.DevMode)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEBUG: Reset",
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                        action = delegate ()
                        {
                            isAutoEnabled = false;
                            isSemiAutoEnabled = true;
                            ResetEverything();

                        }
                    };
                }
                else

                //Full auto machines with no pawn interaction also include a similar Reset button. will only appear if
                //Dev mode is not active, to avoid duplicating it
                if (compItemProcessor.Props.isCompletelyAutoMachine)
                {
                    Command_Action IP_Gizmo_ResetForAutoMachines = new Command_Action();
                    IP_Gizmo_ResetForAutoMachines.defaultLabel = "IP_ResetFullAuto".Translate();
                    IP_Gizmo_ResetForAutoMachines.defaultDesc = "IP_ResetFullAutoDesc".Translate();
                    IP_Gizmo_ResetForAutoMachines.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
                    IP_Gizmo_ResetForAutoMachines.action = delegate
                    {
                        isAutoEnabled = false;
                        isSemiAutoEnabled = true;
                        ResetEverything();
                    };
                    yield return IP_Gizmo_ResetForAutoMachines;
                }


            }


        }

        public void IngredientsChosenBringThemIn()
        {

            if (compItemProcessor.Props.isSemiAutomaticMachine)
            {
                switch (compItemProcessor.Props.numberOfInputs)
                {
                    case 1:
                        if (this.firstItem == "") { return; }
                        break;
                    case 2:
                        if (this.firstItem == "" || this.secondItem == "") { return; }
                        break;
                    case 3:
                        if (this.firstItem == "" || this.secondItem == "" || this.thirdItem == "") { return; }
                        break;
                    case 4:
                        if (this.firstItem == "" || this.secondItem == "" || this.thirdItem == "" || this.fourthItem == "") { return; }
                        break;
                    default:
                        if (this.firstItem == "") { return; }
                        break;

                }
            }

            //If the building is powered or fueled and there is no power or fuel, don't start bringing ingredients
            if (compPowerTrader != null && !compPowerTrader.PowerOn && compItemProcessor.Props.noPowerDestroysProgress && ((!compItemProcessor.Props.isSemiAutomaticMachine) || (compItemProcessor.Props.isSemiAutomaticMachine && !isSemiAutoEnabled)))
            {
                Messages.Message("IP_NoPowerDestroysWarning".Translate(this.def.LabelCap), this, MessageTypeDefOf.NegativeEvent, true);
                if (isAutoEnabled || !isSemiAutoEnabled)
                {
                    isAutoEnabled = false;
                    isSemiAutoEnabled = true;
                    ResetEverything();
                    return;
                }
            }
            else if (compFuelable != null && !compFuelable.HasFuel && compItemProcessor.Props.noPowerDestroysProgress && ((!compItemProcessor.Props.isSemiAutomaticMachine) || (compItemProcessor.Props.isSemiAutomaticMachine && !isSemiAutoEnabled)))
            {
                Messages.Message("IP_NoFuelDestroysWarning".Translate(this.def.LabelCap), this, MessageTypeDefOf.NegativeEvent, true);
                if (isAutoEnabled || !isSemiAutoEnabled)
                {
                    isAutoEnabled = false;
                    isSemiAutoEnabled = true;
                    ResetEverything();
                    return;
                }
            }

            else if (compItemProcessor.Props.isLightDependingMachine)
            {
                float num = base.Map.glowGrid.GroundGlowAt(base.Position, false);
                if ((num > compItemProcessor.Props.maxLight) || (num < compItemProcessor.Props.minLight))
                {
                    Messages.Message("IP_LightDestroysWarning".Translate(this.def.LabelCap), this, MessageTypeDefOf.NegativeEvent, true);
                    if (isAutoEnabled || !isSemiAutoEnabled)
                    {
                        isAutoEnabled = false;
                        isSemiAutoEnabled = true;
                        ResetEverything();
                        return;
                    }
                }

            }
            else if (compItemProcessor.Props.isRainDependingMachine)
            {
                if (this.Map.weatherManager.curWeather.rainRate > 0 && !this.Position.Roofed(this.Map))
                {
                    Messages.Message("IP_RainDestroysWarning".Translate(this.def.LabelCap), this, MessageTypeDefOf.NegativeEvent, true);
                    if (isAutoEnabled || !isSemiAutoEnabled)
                    {
                        isAutoEnabled = false;
                        isSemiAutoEnabled = true;
                        ResetEverything();
                        return;
                    }
                }

            }
            else if (compItemProcessor.Props.isTemperatureDependingMachine)
            {
                float currentTempInMap = this.Position.GetTemperature(this.Map);
                if ((currentTempInMap > compItemProcessor.Props.maxTemp) || (currentTempInMap < compItemProcessor.Props.minTemp))
                {
                    Messages.Message("IP_TemperatureDestroysWarning".Translate(this.def.LabelCap, compItemProcessor.Props.minTemp.ToStringTemperatureRaw(), compItemProcessor.Props.maxTemp.ToStringTemperatureRaw()), this, MessageTypeDefOf.NegativeEvent, true);
                    if (isAutoEnabled || !isSemiAutoEnabled)
                    {
                        isAutoEnabled = false;
                        isSemiAutoEnabled = true;
                        ResetEverything();
                        return;
                    }
                }

            }

            //This finds the recipe in CombinationDefs, and stores its defName in the thisRecipe variable for easy access
            CombinationDef thisElement;

            switch (compItemProcessor.Props.numberOfInputs)
            {
                case 1:
                    thisElement = DefDatabase<CombinationDef>.AllDefs.Where(element => ((element.building == this.def.defName) && element.items.Contains(firstItem))).First();
                    if (thisRecipe == null)
                    {
                        if (thisElement.isNutritionGetterRecipe) {
                           

                            ExpectedAmountFirstIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[0]/ThingDef.Named(thisElement.items[0]).GetStatValueAbstract(StatDefOf.Nutrition)));
                        } else {
                            ExpectedAmountFirstIngredient = thisElement.amount[0];
                        }
                        thisRecipe = thisElement.defName;
                    }
                    break;
                case 2:
                    thisElement = DefDatabase<CombinationDef>.AllDefs.Where(element => ((element.building == this.def.defName) && (element.items.Contains(firstItem) && element.secondItems.Contains(secondItem)))).First();
                    if (thisRecipe == null)
                    {
                       
                        if (thisElement.isNutritionGetterRecipe)
                        {
                            ExpectedAmountFirstIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[0] / ThingDef.Named(thisElement.items[0]).GetStatValueAbstract(StatDefOf.Nutrition)));
                            ExpectedAmountSecondIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[1] / ThingDef.Named(thisElement.items[1]).GetStatValueAbstract(StatDefOf.Nutrition)));

                        }
                        else
                        {
                            ExpectedAmountFirstIngredient = thisElement.amount[0];
                            ExpectedAmountSecondIngredient = thisElement.amount[1];
                        }
                        thisRecipe = thisElement.defName;
                    }
                    break;
                case 3:
                    thisElement = DefDatabase<CombinationDef>.AllDefs.Where(element => ((element.building == this.def.defName) && (element.items.Contains(firstItem) && element.secondItems.Contains(secondItem) && element.thirdItems.Contains(thirdItem)))).First();
                    if (thisRecipe == null)
                    {
                       
                        if (thisElement.isNutritionGetterRecipe)
                        {
                            ExpectedAmountFirstIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[0] / ThingDef.Named(thisElement.items[0]).GetStatValueAbstract(StatDefOf.Nutrition)));
                            ExpectedAmountSecondIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[1] / ThingDef.Named(thisElement.items[1]).GetStatValueAbstract(StatDefOf.Nutrition)));
                            ExpectedAmountThirdIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[2] / ThingDef.Named(thisElement.items[2]).GetStatValueAbstract(StatDefOf.Nutrition)));

                        }
                        else
                        {
                            ExpectedAmountFirstIngredient = thisElement.amount[0];
                            ExpectedAmountSecondIngredient = thisElement.amount[1];
                            ExpectedAmountThirdIngredient = thisElement.amount[2];

                        }
                        thisRecipe = thisElement.defName;
                    }
                    break;
                case 4:
                    thisElement = DefDatabase<CombinationDef>.AllDefs.Where(element => ((element.building == this.def.defName) && (element.items.Contains(firstItem) && element.secondItems.Contains(secondItem) && element.thirdItems.Contains(thirdItem) && element.fourthItems.Contains(fourthItem)))).First();
                    if (thisRecipe == null)
                    {
                        if (thisElement.isNutritionGetterRecipe)
                        {
                            ExpectedAmountFirstIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[0] / ThingDef.Named(thisElement.items[0]).GetStatValueAbstract(StatDefOf.Nutrition)));
                            ExpectedAmountSecondIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[1] / ThingDef.Named(thisElement.items[1]).GetStatValueAbstract(StatDefOf.Nutrition)));
                            ExpectedAmountThirdIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[2] / ThingDef.Named(thisElement.items[2]).GetStatValueAbstract(StatDefOf.Nutrition)));
                            ExpectedAmountFourthIngredient = (int)(Math.Ceiling(thisElement.nutritionAmount[3] / ThingDef.Named(thisElement.items[3]).GetStatValueAbstract(StatDefOf.Nutrition)));

                        }
                        else
                        {
                            ExpectedAmountFirstIngredient = thisElement.amount[0];
                            ExpectedAmountSecondIngredient = thisElement.amount[1];
                            ExpectedAmountThirdIngredient = thisElement.amount[2];
                            ExpectedAmountFourthIngredient = thisElement.amount[3];

                        }
                      
                        thisRecipe = thisElement.defName;
                    }
                    break;
                default:
                    thisElement = DefDatabase<CombinationDef>.AllDefs.Where(element => ((element.building == this.def.defName) && element.items.Contains(firstItem))).First();
                    if (thisRecipe == null)
                    {
                        if (thisElement.isNutritionGetterRecipe)
                        {
                            ExpectedAmountFirstIngredient = (int)Math.Ceiling(thisElement.nutritionAmount[0] / ThingDef.Named(thisElement.items[0]).GetStatValueAbstract(StatDefOf.Nutrition));
                        }
                        else
                        {
                            ExpectedAmountFirstIngredient = thisElement.amount[0];
                        }
                        thisRecipe = thisElement.defName;
                    }
                    break;
            }

            //This little change allows isCompletelyAutoMachines to use the same reset button as semiautomatic
            if (compItemProcessor.Props.isCompletelyAutoMachine)
            {
                isAutoEnabled = true;
            }


            //Two different "next stages", one for auto mode machines, and one for regular ones 
            if (compItemProcessor.Props.isAutoMachine && isAutoEnabled)
            {
                processorStage = ProcessorStage.AutoIngredients;
            }
            else processorStage = ProcessorStage.ExpectingIngredients;


        }

        //This method is called either by Jobs when they insert an item, or by this class when the building extracts and inserts and item from adjacent Hoppers

        public void Notify_StartProcessing()
        {
            //This switch controls progress depending on the number of slots in the processor
            bool StartProcessingByInputSlots = false;
            switch (compItemProcessor.Props.numberOfInputs)
            {
                //Check that we have finished inserting the demanded number of items (or expected = 0, which means just one item)
                case 1:
                    if ((CurrentAmountFirstIngredient >= ExpectedAmountFirstIngredient) || ExpectedAmountFirstIngredient == 0)
                    {
                        StartProcessingByInputSlots = true;
                    }
                    break;
                case 2:
                    if (((CurrentAmountFirstIngredient >= ExpectedAmountFirstIngredient) || ExpectedAmountFirstIngredient == 0) &&
                            ((CurrentAmountSecondIngredient >= ExpectedAmountSecondIngredient) || ExpectedAmountSecondIngredient == 0))
                    {
                        StartProcessingByInputSlots = true;
                    }
                    break;
                case 3:
                    if (((CurrentAmountFirstIngredient >= ExpectedAmountFirstIngredient) || ExpectedAmountFirstIngredient == 0) &&
                            ((CurrentAmountSecondIngredient >= ExpectedAmountSecondIngredient) || ExpectedAmountSecondIngredient == 0) &&
                            ((CurrentAmountThirdIngredient >= ExpectedAmountThirdIngredient) || ExpectedAmountThirdIngredient == 0))
                    {
                        StartProcessingByInputSlots = true;
                    }
                    break;
                case 4:
                    if (((CurrentAmountFirstIngredient >= ExpectedAmountFirstIngredient) || ExpectedAmountFirstIngredient == 0) &&
                            ((CurrentAmountSecondIngredient >= ExpectedAmountSecondIngredient) || ExpectedAmountSecondIngredient == 0) &&
                            ((CurrentAmountThirdIngredient >= ExpectedAmountThirdIngredient) || ExpectedAmountThirdIngredient == 0) &&
                            ((CurrentAmountFourthIngredient >= ExpectedAmountFourthIngredient) || ExpectedAmountFourthIngredient == 0))
                    {
                        StartProcessingByInputSlots = true;
                    }
                    break;
                default:
                    if ((CurrentAmountFirstIngredient >= ExpectedAmountFirstIngredient) || ExpectedAmountFirstIngredient == 0)
                    {
                        StartProcessingByInputSlots = true;
                    }
                    break;
            }


            //Only proceed if we have finished inserting the demanded number of items (or expected = 0, which means just one item)
            if (StartProcessingByInputSlots)
            {
                //And only if no one fucked up writing the recipes in CombinationDefs...
                if (thisRecipe != "")
                {
                    //Change the stage so it doesn't keep waiting for arriving ingredients
                    processorStage = ProcessorStage.AllIngredientReceived;

                    //Get recipe details from the stored recipe defName
                    CombinationDef thisCombinationRecipe = DefDatabase<CombinationDef>.GetNamed(thisRecipe);
                    if (thisCombinationRecipe.differentProductsByQuality)
                    {
                        this.productsToTurnInto = thisCombinationRecipe.productsByQuality;

                    }
                    else
                    {
                        this.productToTurnInto = thisCombinationRecipe.result;
                    }

                    this.amount = thisCombinationRecipe.yield;
                    this.usingQualityIncreasing = thisCombinationRecipe.useQualityIncreasing;
                    //If it is a quality increasing product, use the day intervals, if not, just the single one
                    if (usingQualityIncreasing)
                    {
                        this.awfulQualityAgeDaysThreshold = thisCombinationRecipe.awfulQualityAgeDaysThreshold;
                        this.poorQualityAgeDaysThreshold = thisCombinationRecipe.poorQualityAgeDaysThreshold;
                        this.normalQualityAgeDaysThreshold = thisCombinationRecipe.normalQualityAgeDaysThreshold;
                        this.goodQualityAgeDaysThreshold = thisCombinationRecipe.goodQualityAgeDaysThreshold;
                        this.excellentQualityAgeDaysThreshold = thisCombinationRecipe.excellentQualityAgeDaysThreshold;
                        this.masterworkQualityAgeDaysThreshold = thisCombinationRecipe.masterworkQualityAgeDaysThreshold;
                        this.legendaryQualityAgeDaysThreshold = thisCombinationRecipe.legendaryQualityAgeDaysThreshold;
                    }
                    else
                    {
                        if (this.def.defName.Contains("VFEM_") && ModLister.HasActiveModWithName("Vanilla Factions Expanded - Mechanoids")) 
                        {
                            var type = AccessTools.TypeByName("MechShipsMod");
                            var mod = LoadedModManager.GetMod(type);
                            var settings = type.GetField("settings").GetValue(mod);
                            var settingsType = AccessTools.TypeByName("VFEM.MechShipsSettings");
                            float multiplier = (float)settingsType.GetField("VFEM_factorySpeedMultiplier").GetValue(settings);
                            this.days = thisCombinationRecipe.singleTimeIfNotQualityIncreasing * multiplier * FactoryMultiplierClass.FactoryPreceptMultiplier;
                        }
                        else
                        {
                            this.days = thisCombinationRecipe.singleTimeIfNotQualityIncreasing;
                        }

                    }
                    //Reset ingredient counter (this is probably not necessary cause ResetEverything does it, but oh well
                    CurrentAmountFirstIngredient = 0;
                    CurrentAmountSecondIngredient = 0;
                    CurrentAmountThirdIngredient = 0;
                    CurrentAmountFourthIngredient = 0;
                    //Add the ingredient to the ingredients list, but only if transfersIngredientLists = false. If true, the insertion jobs do it
                    if (!compItemProcessor.Props.transfersIngredientLists && !DefDatabase<CombinationDef>.GetNamed(thisRecipe).isCategoryRecipe)
                    {
                        switch (compItemProcessor.Props.numberOfInputs)
                        {
                            case 1:
                                this.ingredients.Add(ThingDef.Named(firstItem));
                                break;
                            case 2:
                                this.ingredients.Add(ThingDef.Named(firstItem));
                                this.ingredients.Add(ThingDef.Named(secondItem));
                                break;
                            case 3:
                                this.ingredients.Add(ThingDef.Named(firstItem));
                                this.ingredients.Add(ThingDef.Named(secondItem));
                                this.ingredients.Add(ThingDef.Named(thirdItem));
                                break;
                            case 4:
                                this.ingredients.Add(ThingDef.Named(firstItem));
                                this.ingredients.Add(ThingDef.Named(secondItem));
                                this.ingredients.Add(ThingDef.Named(thirdItem));
                                this.ingredients.Add(ThingDef.Named(fourthItem));
                                break;
                            default:
                                this.ingredients.Add(ThingDef.Named(firstItem));
                                break;
                        }

                    }
                    //Change process stage to Working. Counters will start here
                    this.processorStage = ProcessorStage.Working;
                    //If the building is configured to destroy ingredients now, it will do so
                    if (compItemProcessor.Props.destroyIngredientsAtStartOfProcess)
                    {
                        DestroyIngredients();
                    }
                    //Call MapMeshDirty so the building graphic changes(if needed)
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
                }
                else
                {
                    //Someone fucked up
                    Messages.Message("IP_IngredientImproperlyDefined".Translate(), null, MessageTypeDefOf.NegativeEvent, true);
                }
            }

        }





        //Tick should do nothing. I want item processors to be tickerType Rare, but some comps need them to be Normal.
        //So what this does is just calle TickRare every 250 ticks
        public override void Tick()
        {
            base.Tick();
            if (this.def.tickerType == TickerType.Normal)
            {
                //Trigger a rare tick
                if (Find.TickManager.TicksGame % 250 == 0)
                {

                    this.TickRare();
                }
            }
            if (processorStage == ProcessorStage.Working && !this.isPaused)
            {
                if (compFuelable != null && compFuelable.Props.consumeFuelOnlyWhenUsed)
                {
                    compFuelable.Notify_UsedThisTick();
                }
            }
        }

        public void CheckTheHoppers(string itemToCheckFor, ref int ExpectedAmountXIngredient, ref int CurrentAmountXIngredient,
            ref bool XIngredientComplete)
        {

            if((compItemProcessor.Props.noPowerDestroysProgress && compPowerTrader != null && !compPowerTrader.PowerOn) ||
                    (compItemProcessor.Props.noPowerDestroysProgress && compFuelable != null && !compFuelable.HasFuel))
            {
                return;
            }
            //Log.Message("Checking hoppers for "+ itemToCheckFor);
            bool OncePerTick = false;
            for (int i = 0; i < compItemProcessor.Props.inputSlots.Count; i++)
            {
                //OncePerTick avoids having the processor gobble all resources in case more than one hopper is present
                if (!OncePerTick)
                {
                    //Search only tiles that are part of inputSlots 

                    Thing thing = null;
                    Thing thing2 = null;
                    List<Thing> thingList = (this.Position + compItemProcessor.Props.inputSlots[i].RotatedBy(this.Rotation)).GetThingList(base.Map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        Thing thing3 = thingList[j];
                        if (DefDatabase<CombinationDef>.GetNamed(thisRecipe).isCategoryRecipe)
                        {

                            if (thing3.def.IsWithinCategory(ThingCategoryDef.Named(itemToCheckFor)))
                            {
                                thing = thing3;
                            }
                        }
                        else if (thing3.def.defName == itemToCheckFor)
                        {
                            thing = thing3;
                        }
                        if (thing3.def == ThingDefOf.Hopper || thing3.def == DefDatabase<ThingDef>.GetNamedSilentFail("VFEM_HeavyHopper"))
                        {
                            thing2 = thing3;

                        }
                    }
                    //If you find ingredient 1 AND a Hopper...
                    if (thing != null && thing2 != null)
                    {
                        //Don't search again this Tick
                        OncePerTick = true;
                        if (ExpectedAmountXIngredient != 0)
                        {
                            //Calculate remaining amount for this ingredient
                            int amountRemaining = ExpectedAmountXIngredient - CurrentAmountXIngredient;
                            //If the stack is greater than the remaining amount 
                            if (thing.stackCount - amountRemaining > 0)
                            {
                                CurrentAmountXIngredient += amountRemaining;
                                if (CurrentAmountXIngredient >= ExpectedAmountXIngredient)
                                {
                                    XIngredientComplete = true;
                                }
                                firstItemSwallowedForButchery = thing.def.defName;
                                Notify_StartProcessing();
                                //Insert the remaining amount

                                thing.stackCount -= amountRemaining;
                                if (thing.stackCount <= 0)
                                {
                                    thing.Destroy();
                                }
                            }
                            else if ((thing.stackCount - amountRemaining <= 0) && !XIngredientComplete)
                            {
                                //If not, insert the whole stack
                                CurrentAmountXIngredient += thing.stackCount;
                                firstItemSwallowedForButchery = thing.def.defName;
                                Notify_StartProcessing();
                                thing.Destroy();
                            }


                        }
                        else
                        {
                            Notify_StartProcessing();
                            thing.Destroy();

                        }
                    }
                }


            }

        }


        public override void TickRare()
        {
            base.TickRare();
            //TickRare does all the processing work when stage is ProcessorStage.AutoIngredients and ProcessorStage.Working

            //First are fourth calls checking Hoppers in auto machines.
            if (processorStage == ProcessorStage.AutoIngredients)
            {

                //Only keep searching if the first ingredient isn't above ExpectedAmountFirstIngredient
                if (!firstIngredientComplete)
                {
                    CheckTheHoppers(firstItem, ref ExpectedAmountFirstIngredient, ref CurrentAmountFirstIngredient, ref firstIngredientComplete);
                }
                if (!secondIngredientComplete && secondItem != "")
                {
                    CheckTheHoppers(secondItem, ref ExpectedAmountSecondIngredient, ref CurrentAmountSecondIngredient, ref secondIngredientComplete);
                }
                if (!thirdIngredientComplete && thirdItem != "")
                {
                    CheckTheHoppers(thirdItem, ref ExpectedAmountThirdIngredient, ref CurrentAmountThirdIngredient, ref thirdIngredientComplete);
                }
                if (!fourthIngredientComplete && fourthItem != "")
                {
                    CheckTheHoppers(fourthItem, ref ExpectedAmountFourthIngredient, ref CurrentAmountFourthIngredient, ref fourthIngredientComplete);
                }


            }

            //This part of the method deals with the checks (light level, power, quality) when the machine is actually working

            if (processorStage == ProcessorStage.Working && !this.isPaused)
            {
                progressCounter++;


                //If isTemperatureAcceleratingMachine has been set in CompProperties_ItemProcessor, we will multiply the times by the specified time factor

               
              
                if (compItemProcessor.Props.isTemperatureAcceleratingMachine)
                {
                    float currentTempInMap = this.Position.GetTemperature(this.Map);
                    CombinationDef thisCombinationRecipe = DefDatabase<CombinationDef>.GetNamed(thisRecipe);
                    if ((currentTempInMap > compItemProcessor.Props.minAccelerationTemp) && (currentTempInMap < compItemProcessor.Props.maxAccelerationTemp))                     
                    {
                        
                        this.days = thisCombinationRecipe.singleTimeIfNotQualityIncreasing * compItemProcessor.Props.accelerationFactor;
                        this.awfulQualityAgeDaysThreshold = thisCombinationRecipe.awfulQualityAgeDaysThreshold * compItemProcessor.Props.accelerationFactor;
                        this.poorQualityAgeDaysThreshold = thisCombinationRecipe.poorQualityAgeDaysThreshold * compItemProcessor.Props.accelerationFactor;
                        this.normalQualityAgeDaysThreshold = thisCombinationRecipe.normalQualityAgeDaysThreshold * compItemProcessor.Props.accelerationFactor;
                        this.goodQualityAgeDaysThreshold = thisCombinationRecipe.goodQualityAgeDaysThreshold * compItemProcessor.Props.accelerationFactor;
                        this.excellentQualityAgeDaysThreshold = thisCombinationRecipe.excellentQualityAgeDaysThreshold * compItemProcessor.Props.accelerationFactor;
                        this.masterworkQualityAgeDaysThreshold = thisCombinationRecipe.masterworkQualityAgeDaysThreshold * compItemProcessor.Props.accelerationFactor;
                        this.legendaryQualityAgeDaysThreshold = thisCombinationRecipe.legendaryQualityAgeDaysThreshold * compItemProcessor.Props.accelerationFactor;
                    }
                    else
                    {
                        this.days = thisCombinationRecipe.singleTimeIfNotQualityIncreasing;
                        this.awfulQualityAgeDaysThreshold = thisCombinationRecipe.awfulQualityAgeDaysThreshold;
                        this.poorQualityAgeDaysThreshold = thisCombinationRecipe.poorQualityAgeDaysThreshold;
                        this.normalQualityAgeDaysThreshold = thisCombinationRecipe.normalQualityAgeDaysThreshold;
                        this.goodQualityAgeDaysThreshold = thisCombinationRecipe.goodQualityAgeDaysThreshold;
                        this.excellentQualityAgeDaysThreshold = thisCombinationRecipe.excellentQualityAgeDaysThreshold;
                        this.masterworkQualityAgeDaysThreshold = thisCombinationRecipe.masterworkQualityAgeDaysThreshold;
                        this.legendaryQualityAgeDaysThreshold = thisCombinationRecipe.legendaryQualityAgeDaysThreshold;
                    }
                  

                }

                //If noPowerDestroysProgress has been set in CompProperties_ItemProcessor, a new counter starts. This is shared for fueled
                //and powered buildings, since there are none that are both

                if ((compItemProcessor.Props.noPowerDestroysProgress && compPowerTrader != null && !compPowerTrader.PowerOn) ||
                    (compItemProcessor.Props.noPowerDestroysProgress && compFuelable != null && !compFuelable.HasFuel))
                {
                    if (!onlySendWarningMessageOnce)
                    {
                        Messages.Message(compItemProcessor.Props.noPowerDestroysInitialWarning.Translate(), this, MessageTypeDefOf.NegativeEvent, true);

                        onlySendWarningMessageOnce = true;
                    }
                    noPowerDestructionCounter++;
                    //And if it gets higher than the also configurable rareTicksToDestroy...
                    if (noPowerDestructionCounter > compItemProcessor.Props.rareTicksToDestroy)
                    {
                        //You lose the process, the product and the ingredients. haha, fuck you
                        Messages.Message(compItemProcessor.Props.noPowerDestroysMessage.Translate(), this, MessageTypeDefOf.NegativeEvent, true);
                        DestroyIngredients();
                        isSemiAutoEnabled = true;
                        ResetEverything();
                    }

                }

                //If isLightDependingMachine has been set in CompProperties_ItemProcessor, a new counter starts. This will check the light level and
                //compare it to the maxLight and minLight values

                if (compItemProcessor.Props.isLightDependingMachine)
                {
                    float num = base.Map.glowGrid.GroundGlowAt(base.Position, false);
                    if ((num > compItemProcessor.Props.maxLight) || (num < compItemProcessor.Props.minLight))
                    {
                        if (!onlySendLightWarningMessageOnce)
                        {
                            Messages.Message(compItemProcessor.Props.messageIfOutsideLightRangesWarning.Translate(), this, MessageTypeDefOf.NegativeEvent, true);

                            onlySendLightWarningMessageOnce = true;
                        }
                        noGoodLightDestructionCounter++;
                        //And if it gets higher than the also configurable rareTicksToDestroyDueToLight...
                        if (noGoodLightDestructionCounter > compItemProcessor.Props.rareTicksToDestroyDueToLight)
                        {
                            //You lose the process, the product and the ingredients. haha, fuck you
                            Messages.Message(compItemProcessor.Props.messageIfOutsideLightRanges.Translate(), this, MessageTypeDefOf.NegativeEvent, true);
                            DestroyIngredients();
                            isSemiAutoEnabled = true;
                            ResetEverything();
                        }
                    }
                }

                //If isRainDependingMachine has been set in CompProperties_ItemProcessor, a new counter starts. This will check the weather and see if it's raining

                if (compItemProcessor.Props.isRainDependingMachine)
                {

                    if (this.Map.weatherManager.curWeather.rainRate > 0 && !this.Position.Roofed(this.Map))
                    {
                        if (!onlySendRainWarningMessageOnce)
                        {
                            Messages.Message(compItemProcessor.Props.messageIfRainWarning.Translate(), this, MessageTypeDefOf.NegativeEvent, true);

                            onlySendRainWarningMessageOnce = true;
                        }
                        noGoodWeatherDestructionCounter++;
                        //And if it gets higher than the also configurable rareTicksToDestroyDueToRain...
                        if (noGoodWeatherDestructionCounter > compItemProcessor.Props.rareTicksToDestroyDueToRain)
                        {
                            //You lose the process, the product and the ingredients. haha, fuck you
                            Messages.Message(compItemProcessor.Props.messageIfRain.Translate(), this, MessageTypeDefOf.NegativeEvent, true);
                            DestroyIngredients();
                            isSemiAutoEnabled = true;
                            ResetEverything();
                        }
                    }
                }

                //If isTemperatureDependingMachine has been set in CompProperties_ItemProcessor, a new counter starts. This will check the temperature in the building's
                //Position and compare it to maxTemp and minTemp 

                if (compItemProcessor.Props.isTemperatureDependingMachine)
                {
                    float currentTempInMap = this.Position.GetTemperature(this.Map);
                    if ((currentTempInMap > compItemProcessor.Props.maxTemp) || (currentTempInMap < compItemProcessor.Props.minTemp))
                    {
                        if (!onlySendTempWarningMessageOnce)
                        {
                            Messages.Message(compItemProcessor.Props.messageIfWrongTempWarning.Translate(), this, MessageTypeDefOf.NegativeEvent, true);

                            onlySendTempWarningMessageOnce = true;
                        }
                        noGoodTempDestructionCounter++;
                        //And if it gets higher than the also configurable rareTicksToDestroyDueToWrongTemp...
                        if (noGoodTempDestructionCounter > compItemProcessor.Props.rareTicksToDestroyDueToWrongTemp)
                        {
                            //You lose the process, the product and the ingredients. haha, fuck you
                            Messages.Message(compItemProcessor.Props.messageIfWrongTemp.Translate(), this, MessageTypeDefOf.NegativeEvent, true);
                            DestroyIngredients();
                            isSemiAutoEnabled = true;
                            ResetEverything();
                        }
                    }
                }

                //This uses progressCounter to advance through quality levels

                //If recipe uses quality increasing

                if (usingQualityIncreasing)
                {
                    //Progress is only used on showProgressBar buildings, so only update it in that case
                    if (compItemProcessor.Props.showProgressBar)
                    {
                        this.Progress = (float)progressCounter / (rareTicksPerDay * awfulQualityAgeDaysThreshold);
                    }

                    //We check first legendaryQualityAgeDaysThreshold, because if it reaches this, the item is extracted no matter what 
                    if (progressCounter > rareTicksPerDay * this.legendaryQualityAgeDaysThreshold)
                    {

                        qualityNow = QualityCategory.Legendary;
                        removeProductOperation(QualityCategory.Legendary);

                    }
                    //In the other DaysThresholds, product only is extracted if the machine has a desired quality selectro

                    else if (progressCounter == rareTicksPerDay * this.awfulQualityAgeDaysThreshold)
                    {
                        //Also, ingredients are destrioyed here if not at the very beginning, depending on configured destroyIngredientsAtAwfulQuality
                        qualityNow = QualityCategory.Awful;
                        if (compItemProcessor.Props.destroyIngredientsAtAwfulQuality)
                        {
                            DestroyIngredients();
                        }
                        removeAfterAwful = true;
                        if (qualityEstablished && qualityRequested == QualityCategory.Awful)
                        {
                            removeProductOperation(QualityCategory.Awful);
                        }
                    }
                    else if (progressCounter == rareTicksPerDay * this.poorQualityAgeDaysThreshold)
                    {
                        qualityNow = QualityCategory.Poor;
                        if (qualityEstablished && qualityRequested == QualityCategory.Poor)
                        {
                            removeProductOperation(QualityCategory.Poor);
                        }
                    }
                    else if (progressCounter == rareTicksPerDay * this.normalQualityAgeDaysThreshold)
                    {
                        qualityNow = QualityCategory.Normal;
                        if (qualityEstablished && qualityRequested == QualityCategory.Normal)
                        {
                            removeProductOperation(QualityCategory.Normal);
                        }
                    }
                    else if (progressCounter == rareTicksPerDay * this.goodQualityAgeDaysThreshold)
                    {
                        qualityNow = QualityCategory.Good;
                        if (qualityEstablished && qualityRequested == QualityCategory.Good)
                        {
                            removeProductOperation(QualityCategory.Good);
                        }
                    }
                    else if (progressCounter == rareTicksPerDay * this.excellentQualityAgeDaysThreshold)
                    {
                        qualityNow = QualityCategory.Excellent;
                        if (qualityEstablished && qualityRequested == QualityCategory.Excellent)
                        {
                            removeProductOperation(QualityCategory.Excellent);
                        }
                    }
                    else if (progressCounter == rareTicksPerDay * this.masterworkQualityAgeDaysThreshold)
                    {
                        qualityNow = QualityCategory.Masterwork;
                        if (qualityEstablished && qualityRequested == QualityCategory.Masterwork)
                        {
                            removeProductOperation(QualityCategory.Masterwork);
                        }
                    }
                }
                else
                //If the recipe doesn't use quality advancing, it will only have a single processing time
                {

                    //Progress is only used on showProgressBar buildings, so only update it in that case
                    if (compItemProcessor.Props.showProgressBar || compItemProcessor.Props.showFactoryProgressBar)
                    {
                        this.Progress = (float)progressCounter / (rareTicksPerDay * days);
                    }

                    if (progressCounter > rareTicksPerDay * this.days)
                    {
                        if (DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).outputLimitControlled) {
                            OverThreshold = false;
                            List<Thing> presentProductsList = this.InteractionCell.GetThingList(base.Map);
                            for (int j = 0; j < presentProductsList.Count; j++)
                            {
                                Thing thingPresent = presentProductsList[j];
                                if (thingPresent.def.defName == productToTurnInto &&
                                    thingPresent.stackCount>= DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).maxTotalOutput)
                                {
                                    progressCounter = (int)(rareTicksPerDay * this.days);
                                    OverThreshold = true;
                                    break;
                                }
                            }
                            if (!OverThreshold)
                            {
                                removeProductOperation(QualityCategory.Awful);
                            }
                        }
                        else
                        {
                            removeProductOperation(QualityCategory.Awful);
                        }
                        
                    }
                }
            }
        }


        //Method copied from a vanilla class that gets adjacent cells
        public List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (this.cachedAdjCellsCardinal == null)
                {
                    this.cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
                                                   where c.InBounds(base.Map)
                                                   select c).ToList<IntVec3>();
                }
                return this.cachedAdjCellsCardinal;
            }
        }

        //method with operations to do when a product is removed
        public void removeProductOperation(QualityCategory quality)
        {
            //If machine is configured as an auto dropper
            if (compItemProcessor.Props.isAutoDropper)
            {
                //Create product Thing 
                Thing newProduct;
                bool usesStuffedRecipe = DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).resultUsesStuffed;
                if (this.productsToTurnInto != null && this.productsToTurnInto.Count > 0)
                {
                    if (usesStuffedRecipe)
                    {

                        newProduct = ThingMaker.MakeThing(ThingDef.Named(productsToTurnInto[(int)quality]),
                        ThingDef.Named(DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).resultStuff));


                    }
                    else
                    {
                        newProduct = ThingMaker.MakeThing(ThingDef.Named(productsToTurnInto[(int)quality]));

                    }

                }
                else
                {
                    if (usesStuffedRecipe)
                    {
                        newProduct = ThingMaker.MakeThing(ThingDef.Named(productToTurnInto),
                        ThingDef.Named(DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).resultStuff));
                    }
                    else
                    {
                        newProduct = ThingMaker.MakeThing(ThingDef.Named(productToTurnInto));
                    }
                }
                //Set its amount (yield)
                if (DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isButcheryRecipe)
                {
                    newProduct.stackCount = Mathf.RoundToInt(ThingDef.Named(firstItemSwallowedForButchery).GetStatValueAbstract(StatDefOf.MeatAmount) * DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).efficiency);
                    amount = newProduct.stackCount;
                }
                else newProduct.stackCount = amount;

                //If it's a CompIngredients item, transfer ingredients list to it, but only if ignoresIngredientLists=false or transfersIngredientLists=true
                if ((newProduct.TryGetComp<CompIngredients>() is CompIngredients ingredientComp) && (!compItemProcessor.Props.ignoresIngredientLists || compItemProcessor.Props.transfersIngredientLists))
                {
                    ingredientComp.ingredients = ingredients;
                }

                //If it's a CompQuality item, transfer quality to it
                if (usingQualityIncreasing && newProduct.TryGetComp<CompQuality>() is CompQuality qualityComp)
                {
                    qualityComp.SetQuality(qualityNow, ArtGenerationContext.Colony);
                }




                //Spawn this Thing in the buildings interaction cell, which can have a stockpile, a Hopper, or nothing

                bool AlreadyPresent = false;

                List<Thing> presentProductsList = this.InteractionCell.GetThingList(base.Map);
                for (int j = 0; j < presentProductsList.Count; j++)
                {
                    Thing thingPresent = presentProductsList[j];
                    if (thingPresent.def.defName == newProduct.def.defName && !usesStuffedRecipe)
                    {
                        //Log.Message("getting here because "+ thingPresent.def.defName+" is "+ newProduct.def.defName);
                        thingPresent.stackCount += amount;
                        AlreadyPresent = true;
                        break;
                    }
                }
                if (!AlreadyPresent)
                {

                    if (newProduct.stackCount > newProduct.def.stackLimit)
                    {
                        GenPlace.TryPlaceThing(newProduct, this.InteractionCell, this.Map, ThingPlaceMode.Direct, null, null, default(Rot4));
                        List<Thing> presentProductsListAfterFirstStack = this.InteractionCell.GetThingList(base.Map);
                        for (int j = 0; j < presentProductsListAfterFirstStack.Count; j++)
                        {
                            Thing thingPresent = presentProductsList[j];
                            if (thingPresent.def.defName == newProduct.def.defName ) {
                                thingPresent.stackCount += newProduct.stackCount;
                            }
                        }
                    }
                    else
                        GenPlace.TryPlaceThing(newProduct, this.InteractionCell, this.Map, ThingPlaceMode.Direct, null, null, default(Rot4));
                }

                //Reset building (if it is also an auto machine (isAutoMachine) it will begin grabbing ingredients again)
                DestroyIngredients();
                ResetEverything();

            }
            else
            {
                //If machine is NOT configured as an auto dropper

                Messages.Message(DefDatabase<CombinationDef>.GetNamed(thisRecipe).finishedProductMessage.Translate(), this, MessageTypeDefOf.PositiveEvent, true);
                progressCounter = 0;
                //This ProcessorStage.Finished enables the WorkGiver_RemoveProduct scanner, which does the same as above, but with a pawn extracting the product
                processorStage = ProcessorStage.Finished;
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
            }


        }

        public override string GetInspectString()
        {
            // Don't do anything if minified/missing the comp
            if (compItemProcessor == null)
            {
                return base.GetInspectString();
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }

            string incubationTxt = "";
            if (compPowerTrader != null || compFuelable != null)
            {
                incubationTxt += "\n";
            }
            if (processorStage <= ProcessorStage.IngredientsChosen)
            {

                incubationTxt += "IP_IsEmpty".Translate(this.def.LabelCap);
            }
            else if (processorStage == ProcessorStage.ExpectingIngredients || processorStage == ProcessorStage.AutoIngredients)
            {
                string productOrCategoryName = "";
                string secondProductOrCategoryName = "";
                string thirdProductOrCategoryName = "";
                string fourthProductOrCategoryName = "";

                if (compItemProcessor.Props.isCategoryBuilding || (this.thisRecipe != null && DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isCategoryRecipe))
                {
                    productOrCategoryName = ThingCategoryDef.Named(firstCategory).LabelCap;
                    if (compItemProcessor.Props.numberOfInputs >= 2)
                    {
                        secondProductOrCategoryName = ThingCategoryDef.Named(secondCategory).LabelCap;
                    }
                    if (compItemProcessor.Props.numberOfInputs >= 3)
                    {
                        thirdProductOrCategoryName = ThingCategoryDef.Named(thirdCategory).LabelCap;
                    }
                    if (compItemProcessor.Props.numberOfInputs >= 3)
                    {
                        fourthProductOrCategoryName = ThingCategoryDef.Named(fourthCategory).LabelCap;
                    }
                }
                else
                {
                    productOrCategoryName = ThingDef.Named(firstItem).LabelCap;
                    if (compItemProcessor.Props.numberOfInputs >= 2)
                    {
                        secondProductOrCategoryName = ThingDef.Named(secondItem).LabelCap;
                    }
                    if (compItemProcessor.Props.numberOfInputs >= 3)
                    {
                        thirdProductOrCategoryName = ThingDef.Named(thirdItem).LabelCap;
                    }
                    if (compItemProcessor.Props.numberOfInputs >= 4)
                    {
                        fourthProductOrCategoryName = ThingDef.Named(fourthItem).LabelCap;
                    }
                }

                incubationTxt += "IP_FilledWith".Translate(this.def.LabelCap, productOrCategoryName) + "IP_IngredientPercentage".Translate(CurrentAmountFirstIngredient.ToString(), ExpectedAmountFirstIngredient.ToString());

                if (compItemProcessor.Props.numberOfInputs >= 2)
                {
                    incubationTxt += "\n" + "IP_FilledWithSecond".Translate(this.def.LabelCap, secondProductOrCategoryName) + "IP_IngredientPercentage".Translate(CurrentAmountSecondIngredient.ToString(), ExpectedAmountSecondIngredient.ToString());

                }
                if (compItemProcessor.Props.numberOfInputs >= 3)
                {
                    incubationTxt += "\n" + "IP_FilledWithThird".Translate(this.def.LabelCap, thirdProductOrCategoryName) + "IP_IngredientPercentage".Translate(CurrentAmountThirdIngredient.ToString(), ExpectedAmountThirdIngredient.ToString());

                }
                if (compItemProcessor.Props.numberOfInputs >= 4)
                {
                    incubationTxt += "\n" + "IP_FilledWithFourth".Translate(this.def.LabelCap, fourthProductOrCategoryName) + "IP_IngredientPercentage".Translate(CurrentAmountFourthIngredient.ToString(), ExpectedAmountFourthIngredient.ToString());

                }
            }
            else
            if (processorStage == ProcessorStage.Working && usingQualityIncreasing)
            {
                string productOrCategoryName;
                if (compItemProcessor.Props.isCategoryBuilding || (this.thisRecipe != null && DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isCategoryRecipe))
                {
                    productOrCategoryName = ThingCategoryDef.Named(firstCategory).LabelCap;
                }
                else
                {
                    productOrCategoryName = ThingDef.Named(firstItem).LabelCap;
                }
                if (this.isPaused)
                {
                    incubationTxt += "IP_ProcessorPaused".Translate(this.def.LabelCap);
                }
                else
                {
                    incubationTxt += "IP_ProcessorWorking".Translate(this.def.LabelCap);
                }

                if (!removeAfterAwful)
                {
                    if (productsToTurnInto != null && productsToTurnInto.Count > 0)
                    {
                        incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productsToTurnInto[0]).LabelCap, qualityNow.ToString(), ((int)(ticksPerDay * this.awfulQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));

                    }
                    else
                    {
                        incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, qualityNow.ToString(), ((int)(ticksPerDay * this.awfulQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));

                    }
                }
                else
                {

                    if (productsToTurnInto != null && productsToTurnInto.Count > 0)
                    {



                        switch (qualityNow)
                        {
                            case QualityCategory.Awful:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productsToTurnInto[1]).LabelCap, QualityCategory.Poor.ToString(), ((int)(ticksPerDay * this.poorQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Poor:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productsToTurnInto[2]).LabelCap, QualityCategory.Normal.ToString(), ((int)(ticksPerDay * this.normalQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Normal:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productsToTurnInto[3]).LabelCap, QualityCategory.Good.ToString(), ((int)(ticksPerDay * this.goodQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Good:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productsToTurnInto[4]).LabelCap, QualityCategory.Excellent.ToString(), ((int)(ticksPerDay * this.excellentQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Excellent:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productsToTurnInto[5]).LabelCap, QualityCategory.Masterwork.ToString(), ((int)(ticksPerDay * this.masterworkQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Masterwork:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productsToTurnInto[6]).LabelCap, QualityCategory.Legendary.ToString(), ((int)(ticksPerDay * this.legendaryQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                        }

                    }
                    else
                    {

                        switch (qualityNow)
                        {
                            case QualityCategory.Awful:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, QualityCategory.Poor.ToString(), ((int)(ticksPerDay * this.poorQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Poor:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, QualityCategory.Normal.ToString(), ((int)(ticksPerDay * this.normalQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Normal:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, QualityCategory.Good.ToString(), ((int)(ticksPerDay * this.goodQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Good:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, QualityCategory.Excellent.ToString(), ((int)(ticksPerDay * this.excellentQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Excellent:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, QualityCategory.Masterwork.ToString(), ((int)(ticksPerDay * this.masterworkQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                            case QualityCategory.Masterwork:
                                incubationTxt += "\n" + "IP_ProcessingInProgress".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, QualityCategory.Legendary.ToString(), ((int)(ticksPerDay * this.legendaryQualityAgeDaysThreshold) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
                                break;
                        }
                    }

                }
                if (qualityEstablished)
                {
                    incubationTxt += "\n" + "IP_QualityAutoEstablished".Translate(qualityRequested.ToString());
                }
            }
            else if (processorStage == ProcessorStage.Working && !usingQualityIncreasing)
            {
                string productOrCategoryName;
                if (compItemProcessor.Props.isCategoryBuilding || (this.thisRecipe != null && DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).isCategoryRecipe))
                {
                    productOrCategoryName = ThingCategoryDef.Named(firstCategory).LabelCap;
                }
                else
                {
                    productOrCategoryName = ThingDef.Named(firstItem).LabelCap;
                }
                if (this.isPaused)
                {
                    incubationTxt += "IP_ProcessorPaused".Translate(this.def.LabelCap);
                }
                else
                {
                    incubationTxt += "IP_ProcessorWorking".Translate(this.def.LabelCap);
                }
                incubationTxt += "\n" + "IP_ProcessingInProgressNoQuality".Translate(this.def.LabelCap, productOrCategoryName, ThingDef.Named(productToTurnInto).LabelCap, ((int)(ticksPerDay * this.days) - (progressCounter * 250)).ToStringTicksToPeriod(true, false, true, true));
            }
            if (processorStage == ProcessorStage.Finished)
            {
                incubationTxt += "IP_ProductWaiting".Translate();
            }

            if (compItemProcessor.Props.isTemperatureDependingMachine)
            {
                incubationTxt += "IP_TempRangeInThisMachine".Translate(compItemProcessor.Props.minTemp.ToStringTemperature(), compItemProcessor.Props.maxTemp.ToStringTemperature(), Prefs.TemperatureMode.ToString());
            }
            if (this.OverThreshold)
            {
                if (this.thisRecipe != null)
                {
                    incubationTxt += "IP_OverThreshold".Translate(DefDatabase<CombinationDef>.GetNamedSilentFail(this.thisRecipe).maxTotalOutput);
                }
            }

            stringBuilder.Append(incubationTxt);

            return stringBuilder.ToString();
        }

        public override Graphic Graphic
        {
            //This changes the graphic of the building. Runs very seldom unless called by  base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things | MapMeshFlagDefOf.Buildings);
            get
            {
                if (compItemProcessor == null)
                {
                    return this.DefaultGraphic;
                }
                else if (processorStage == ProcessorStage.Working && compItemProcessor.Props.buildingOnGraphic != "")
                {
                    Shader shader;
                    if (compItemProcessor.Props.shaderForBuildingOnGraphic != null)
                    {
                        shader = compItemProcessor.Props.shaderForBuildingOnGraphic.Shader;
                    }
                    else
                    {
                        shader = this.def.graphicData.shaderType.Shader;
                    }

                    Graphic newgraphic = GraphicDatabase.Get(typeof(Graphic_Multi), compItemProcessor.Props.buildingOnGraphic, shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
                    return newgraphic;
                }
                else if (processorStage == ProcessorStage.Finished && compItemProcessor.Props.buildingFinishedGraphic!="")
                {
                    Shader shader;
                    if (compItemProcessor.Props.shaderForBuildingFinishedGraphic != null)
                    {
                        shader = compItemProcessor.Props.shaderForBuildingFinishedGraphic.Shader;
                    }
                    else
                    {
                        shader = this.def.graphicData.shaderType.Shader;
                    }
                    Graphic newgraphic = GraphicDatabase.Get(typeof(Graphic_Multi), compItemProcessor.Props.buildingFinishedGraphic, shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
                    return newgraphic;
                }
                else
                {
                    return this.DefaultGraphic;
                }
            }
        }

        //Material for the progress bar. Cached, only updated every tick rare

        private Material BarFilledMat
        {
            get
            {

                if (this.barFilledCachedMat == null)
                {
                    //Bar colour interpolation depending on progress
                    this.barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(GraphicsCache.BarZeroProgressColor, GraphicsCache.BarFermentedColor, this.Progress), false);
                }
                return this.barFilledCachedMat;
            }
        }

        private Material FactoryBarFilledMat
        {
            get
            {

                if (this.barFilledCachedMat == null)
                {
                    //Bar colour interpolation depending on progress
                    this.barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(GraphicsCache.FactoryBarZeroProgressColor, GraphicsCache.FactoryBarFinishedColor, this.Progress), false);
                }
                return this.barFilledCachedMat;
            }
        }

        //This progress counter is only used by the progress bar

        public float Progress
        {
            get
            {
                return this.progressInt;
            }
            set
            {

                if (value == this.progressInt)
                {
                    return;
                }
                this.progressInt = value;
                this.barFilledCachedMat = null;
            }


        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (compItemProcessor.Props.inputSlots != null)
            {
                foreach (IntVec3 cell in compItemProcessor.Props.inputSlots)
                {
                    Vector3 vector = (this.Position + cell.RotatedBy(this.Rotation)).ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                    Graphics.DrawMesh(MeshPool.plane10, vector, Quaternion.identity, GraphicsCache.InputCellMaterial, 0);
                }
                Vector3 vectorInteractionSpot = this.InteractionCell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                Graphics.DrawMesh(MeshPool.plane10, vectorInteractionSpot, Quaternion.identity, GraphicsCache.OutputCellMaterial, 0);
            }
        }


        //A draw method for the progress bar

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (compItemProcessor.Props.showProgressBar)
            {
                Vector3 drawPos = this.DrawPos;
                drawPos.y += 0.0454545468f;
                drawPos.z += 0.25f;
                float CalculatedFillPercent = 0;
                //If the machine is working, the bar shows as full (if it's not full it won't start working)
                if (processorStage == ProcessorStage.Working)
                {
                    CalculatedFillPercent = 1;
                }
                else
                {
                    //No divide by zero errors on my watch
                    if (ExpectedAmountFirstIngredient == 0)
                    {
                        CalculatedFillPercent = 0;

                    }
                    else
                    {
                        CalculatedFillPercent = (float)CurrentAmountFirstIngredient / ExpectedAmountFirstIngredient;
                    }
                }
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = BarSize,
                    fillPercent = CalculatedFillPercent,
                    filledMat = this.BarFilledMat,
                    unfilledMat = GraphicsCache.BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });


            }
            if (compItemProcessor.Props.showFactoryProgressBar)
            {
                Vector3 drawPos = this.DrawPos;
                drawPos.y += 0.0454545468f;
                drawPos.z += 0.25f;
                float CalculatedFillPercent = 0;

                if (processorStage == ProcessorStage.Working)
                {
                    CalculatedFillPercent = this.Progress;
                } else CalculatedFillPercent = 0;
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = BarSize,
                    fillPercent = CalculatedFillPercent,
                    filledMat = this.FactoryBarFilledMat,
                    unfilledMat = GraphicsCache.BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });


            }


        }


    }
}
