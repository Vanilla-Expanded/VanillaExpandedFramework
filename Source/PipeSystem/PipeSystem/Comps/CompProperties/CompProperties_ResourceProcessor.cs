using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompProperties_ResourceProcessor : CompProperties_Resource
    {
        public CompProperties_ResourceProcessor()
        {
            compClass = typeof(CompResourceProcessor);
        }
        // Show inspect string?
        public bool showBufferInfo = true;
        // Translation key:
        public string notWorkingKey;
        // All possible results of the process
        public List<Result> results;

        /// <summary>
        /// Config errors handling. Empty results? Null translation key?
        /// </summary>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (results.NullOrEmpty())
                yield return $"CompProperties_ResourceProcessor of {parentDef.defName} cannot have empty or null <results>";
            if (notWorkingKey == null)
                yield return $"CompProperties_ResourceProcessor of {parentDef.defName} cannot have null <notWorkingKey>";
        }

        /// <summary>
        /// Result class, contain every information needed to produce stuff
        /// </summary>
        public class Result
        {
            // Produce each ticks: Default to 600 ticks (10 sec)
            public int eachTicks = 600;
            // Amount needed to produce result
            public int countNeeded;
            // Result as a thing
            public ThingDef thing;
            public int thingCount;
            // Result as a piped resource
            public PipeNetDef net;
            public int netCount;
        }
    }
}
