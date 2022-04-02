using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompProperties_Resource : CompProperties
    {
        public CompProperties_Resource()
        {
            compClass = typeof(CompResource);
        }

        public PipeNetDef pipeNet;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var msg in base.ConfigErrors(parentDef))
            {
                yield return msg;
            }

            if (pipeNet == null)
                yield return $"CompProperties_Resource can't have null resource";
        }

        public Resource Resource => pipeNet.resource;
    }
}