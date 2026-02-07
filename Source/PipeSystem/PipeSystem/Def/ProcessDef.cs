using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Result class, contain every information needed to produce stuff
    /// </summary>
    public class ProcessDef : Def
    {
        public List<Ingredient> ingredients = new List<Ingredient>();   // Ingredients to produce
        public bool destroyIngredientsOnStart = false;                  // Destroy ingredients when started?
        public bool destroyIngredientsDirectly = false;                 // Destroy ingredients as soon as inside processor
        public int ticks = 600;                                         // Produce each X tick(s): Default to 600 ticks (10 sec)
        public List<int> ticksQuality;                                  // A list of seven tick counts for each of the quality levels
        public int wastePackToProduce = 0;                              // Number of wastepack to produce when process end
        public List<ResearchProjectDef> researchPrerequisites;          // Research required to unlock the process
        public bool hideProcessIfNotNaturalRock = false;                // Process will not be shown if the result is not a natural rock in this map
        public ThingDef rockToDetect;                                   // Only used by hideProcessIfNotNaturalRock=true

        public List<Result> results = new List<Result>();

        public Color finishedColor = new Color(0.9f, 0.85f, 0.2f);      // Bar color when finished
        public Color lowProgressColor = new Color(0.4f, 0.27f, 0.22f);  // Bar color low progress

        // Variables handling input / output

        public bool isFactoryProcess = false;                           // This just affects precepts from VE Memes
        public bool autoGrabFromHoppers = false;                        // Auto insert from hoppers
        public List<IntVec3> autoInputSlots = null;                     // Position of the input slots for autoGrabFromHoppers
        public bool onlyGrabAndOutputToFactoryHoppers = false;          // If this is true, normal hoppers won't be enough
        public bool autoExtract = true;                                 // Auto extract
        public bool manualExtractAllowNet = true;                       // When pawn manually extract, result will try to go in net first
        public int extractTicks = 800;                                  // Manual extract ticks needed
        public bool spawnOnInteractionCell = false;                     // For manual extracts

        // Variables handling processes being ruined by lack of power, temperature, etc

        public bool temperatureRuinable = false;                        // Can be ruined by wrong temperature
        public float minSafeTemperature;                                // Minimum safe temperature
        public float maxSafeTemperature = 100f;                         // Maximum safe temperature
        public float progressPerDegreePerTick = 1E-05f;                 // Ruining due to incorrect temp progress per tick
        public string noProperTempDestroyed = "IP_SpoiledDueToWrongTemp";

        public int rareTicksToDestroy;                                  // This handles the rest of them

        public bool noPowerDestroysProgress = false;                    // Can be ruined by a lack of power
        public string noPowerDestroysMessage = "IP_NoPowerDestroysMessage";
        public string noPowerDestroysInitialWarning = "IP_NoPowerDestroysInitialWarning";      

        public bool isLightDependingProcess = false;                    //This defines whether the process is affected by bright light or lack of light
        public float maxLight = 1f;
        public float minLight = 0f;
        public string messageIfOutsideLightRangesWarning = "IP_OutsideLightRange";
        public string messageIfOutsideLightRanges = "IP_SpoiledDueToLight";

        public bool isRainDependingProcess = false;                    //This defines whether the process is affected by rain
        public string messageIfRainingWarning = "IP_ItRains";
        public string messageIfRaining = "IP_SpoiledDueToRain";
        
        public bool isTemperatureAcceleratingProcess = false;          // This defines whether the process goes faster if temperature is in a given range. UNIMPLEMENTED ATM
        public float maxAccelerationTemp = 1f;
        public float minAccelerationTemp = 0f;
        public float accelerationFactor = 1f;


        // Variables handling ingredients and their handling

        public bool useIngredients = false;                             // Use ingredients
        public bool transfersIngredientList = false;                    // Ingredient list of the product will be ingredient list of the input 

        // Variables handling quality

        public bool stopAtQuality = false;                              // Process stops when reaching a given quality
        public QualityCategory quality = QualityCategory.Normal;
        public bool allowExtractAtCurrentQuality;                       // Allow players to stop the process

        // Misc

        public string labelOverride = "";
        public int priorityInBillList = 0;     
        public bool hideProgressInInfobox = false;

        // Sound handling

        public bool sustainerWhenWorking = false;
        public SoundDef sustainerDef;

        public string uiIconPath; // Path to the icon, used for UI
        [Unsaved]
        internal Texture2D uiIcon; // Resource icon

        public bool disallowMixing;
        /// <summary>
        /// Ingredient: can be pipeNet or thingDef and a count, or a category
        /// </summary>
        public class Ingredient
        {
            // TODO: Allow ThingFilter. Thousand year stare
            public PipeNetDef pipeNet;
            public ThingDef thing;
            public ThingCategoryDef thingCategory;
            public List<ThingDef> disallowedThingDefs= new List<ThingDef>();
            // Amount needed to produce result
            public float countNeeded;
            public bool nutritionGetter = false;
        }

        public class ResultWorker
        {
            public Result result;
            public virtual ThingDef GetResult(Process process)
            {
                return result.thing;
            }
        }

        /// <summary>
        /// Ingredient: can be pipeNet or thingDef and a count
        /// </summary>
        public class Result
        {
            public PipeNetDef pipeNet;                          // Result as a piped resource
            public ThingDef thing;                              // Result as a thing
            public Type workerClass = typeof(ResultWorker);
            public ResultWorker _worker;
            public ResultWorker Worker
            {
                get
                {
                    if (_worker == null)
                    {
                        _worker = (ResultWorker)Activator.CreateInstance(workerClass ?? typeof(ResultWorker));
                        _worker.result = this;
                    }
                    return _worker;
                }
            }

            public ThingDef GetOutput(Process process)
            {
                return Worker.GetResult(process);
            }

            public int count;                                   // Count to produce
            public IntVec3 outputCellOffset = IntVec3.Invalid;  // Result cell output (offset based on center)
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
                yield return error;

            if (ingredients.NullOrEmpty() && autoGrabFromHoppers)
                yield return $"ProcessDef cannot have empty or null <ingredients> and autoGrabFromHoppers";
            if (results.NullOrEmpty())
                yield return $"ProcessDef cannot have empty or null <results>";
            if (autoGrabFromHoppers && autoInputSlots.NullOrEmpty())
                yield return $"ProcessDef with <autoGrabFromHoppers> set to true cannot have empty or null <autoInputSlots>";
            if (hideProcessIfNotNaturalRock && rockToDetect is null)
                yield return $"ProcessDef with <hideProcessIfNotNaturalRock> needs a valid natural rock type on <rockToDetect>";

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                if (result.pipeNet != null && result.thing == null && result.outputCellOffset != IntVec3.Invalid)
                    yield return $"ProcessDef result ({i}) <outputCellOffset> does not apply to net result";
            }
        }

        /// <summary>
        /// Get ui icon as Texture2D
        /// </summary>
        public override void PostLoad()
        {
            if (uiIconPath != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
                });
            }
            base.PostLoad();
        }
    }
}
