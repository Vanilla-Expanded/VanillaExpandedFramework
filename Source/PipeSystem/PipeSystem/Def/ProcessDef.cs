using RimWorld;
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
        public int wastePackToProduce = 0;                              // Number of wastepack to produce when process end
        public List<ResearchProjectDef> researchPrerequisites;          // Research required to unlock the process

        public List<Result> results = new List<Result>();

        public Color finishedColor = new Color(0.9f, 0.85f, 0.2f);      // Bar color when finished
        public Color lowProgressColor = new Color(0.4f, 0.27f, 0.22f);  // Bar color low progress

        public bool autoGrabFromHoppers = false;
        public List<IntVec3> autoInputSlots = null;
        public bool autoExtract = true;                                 // Auto extract
        public bool manualExtractAllowNet = true;                       // When pawn manually extract, result will try to go in net first
        public int extractTicks = 800;                                  // Manual extract ticks needed

        public bool temperatureRuinable = false;                        // Can be ruined by wrong temperature
        public float minSafeTemperature;                                // Minimum safe temperature
        public float maxSafeTemperature = 100f;                         // Maximum safe temperature
        public float progressPerDegreePerTick = 1E-05f;                 // Ruining due to incorrect temp progress per tick
                                                                        //Use ingredients
        public bool useIngredients = false;
        public bool transfersIngredientList = false;
        public bool spawnOnInteractionCell = false;
        public string labelOverride = "";
        public int priorityInBillList = 0;
        public bool stopAtQuality = false;
        public QualityCategory quality = QualityCategory.Normal;

        public bool hideProgressInInfobox = false;

        public bool sustainerWhenWorking = false;
        public SoundDef sustainerDef;

        public bool isFactoryProcess = false;

        /// <summary>
        /// Ingredient: can be pipeNet or thingDef and a count, or a category
        /// </summary>
        public class Ingredient
        {
            // TODO: Allow ThingFilter
            public PipeNetDef pipeNet;
            public ThingDef thing;
            public ThingCategoryDef thingCategory;
            // Amount needed to produce result
            public int countNeeded;
            public bool nutritionGetter = false;
        }

        /// <summary>
        /// Ingredient: can be pipeNet or thingDef and a count
        /// </summary>
        public class Result
        {
            public PipeNetDef pipeNet;                          // Result as a piped resource
            public ThingDef thing;                              // Result as a thing
            public int count;                                   // Count to produce
            public IntVec3 outputCellOffset = IntVec3.Invalid;  // Result cell output (offset based on center)
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
                yield return error;

            if (ingredients.NullOrEmpty())
                yield return $"ProcessDef cannot have empty or null <ingredients>";
            if (results.NullOrEmpty())
                yield return $"ProcessDef cannot have empty or null <results>";

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                if (result.pipeNet != null && result.thing == null && result.outputCellOffset != IntVec3.Invalid)
                    yield return $"ProcessDef result ({i}) <outputCellOffset> does not apply to net result";
            }
        }
    }
}
