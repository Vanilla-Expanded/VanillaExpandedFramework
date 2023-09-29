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
        // TODO: Multiple results
        public ThingDef thing;                                          // Result as a thing
        public PipeNetDef pipeNet;                                      // Result as a piped resource
        public int count;                                               // Count to produce

        public Color finishedColor = new Color(0.9f, 0.85f, 0.2f);      // Bar color when finished
        public Color lowProgressColor = new Color(0.4f, 0.27f, 0.22f);  // Bar color low progress

        public bool autoExtract = true;                                 // Auto extract
        public bool manualExtractAllowNet = true;                       // When pawn manually extract, result will try to go in net first
        public int extractTicks = 800;                                  // Manual extract ticks needed

        /// <summary>
        /// Ingredient: can be pipeNet or thingDef and a count
        /// </summary>
        public class Ingredient
        {
            // TODO: Allow ThingFilter
            public PipeNetDef pipeNet;
            public ThingDef thing;
            // Amount needed to produce result
            public int countNeeded;
        }
    }
}
