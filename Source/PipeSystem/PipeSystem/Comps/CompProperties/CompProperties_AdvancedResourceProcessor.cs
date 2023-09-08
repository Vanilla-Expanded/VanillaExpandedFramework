using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class CompProperties_AdvancedResourceProcessor : CompProperties
    {
        public CompProperties_AdvancedResourceProcessor()
        {
            compClass = typeof(CompAdvancedResourceProcessor);
        }

        // Show inspect string?
        public bool showBufferInfo = true;
        // Translation key:
        public string notWorkingKey;
        // All possible processes
        public List<ProcessDef> processes;
        // Sound when working
        public SoundDef soundAmbient; // TODO
        // TODO output cell
        // TODO input cell
        // Progress bar
        public bool showProgressBar = true;
        public Vector3 progressBarOffset = Vector3.zero;
        // Result icon
        public bool showResultItem = false;
        public Vector3 resultItemOffset = Vector3.zero;
        public Vector3 resultItemSize = new Vector3(0.5f, 1, 0.5f);
        // Time required to put item inside processor
        public int ticksToFill = 200;

        /// <summary>
        /// Config errors handling. Empty processes? Null translation key? Missing comps?
        /// </summary>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (!parentDef.inspectorTabs.NullOrEmpty() && !parentDef.inspectorTabs.Contains(typeof(ITab_Processor)))
                yield return $"CompProperties_AdvancedResourceProcessor of {parentDef.defName} need ITab_Processor";

            if (!(parentDef.tickerType == TickerType.Rare || parentDef.tickerType == TickerType.Normal))
                yield return $"CompProperties_AdvancedResourceProcessor of {parentDef.defName} need tickerType rare or normal";

            if (processes.NullOrEmpty())
                yield return $"CompProperties_AdvancedResourceProcessor of {parentDef.defName} cannot have empty or null <processes>";

            if (notWorkingKey == null)
                yield return $"CompProperties_AdvancedResourceProcessor of {parentDef.defName} cannot have null <notWorkingKey>";

            if (showProgressBar && parentDef.drawerType != DrawerType.MapMeshAndRealTime)
                yield return $"CompProperties_AdvancedResourceProcessor of {parentDef.defName} with showProgressBar true need MapMeshAndRealTime";

            for (int i = 0; i < processes.Count; i++)
            {
                var result = processes[i];
                if (result.ingredients.NullOrEmpty())
                {
                    yield return $"Result {i + 1} of {parentDef.defName} cannot have empty or null <ingredients>";
                    continue;
                }

                for (int o = 0; o < result.ingredients.Count; o++)
                {
                    var requirement = result.ingredients[o];
                    if (requirement.thing == null && requirement.pipeNet == null)
                        yield return $"Ingredient {o + 1} of result {i + 1} ({result.defName}) cannot have <thing> and <pipeNet> both null";

                    if (requirement.pipeNet != null)
                    {
                        var foundNeededComp = false;
                        for (int j = 0; j < parentDef.comps.Count; j++)
                        {
                            var comp = parentDef.comps[j];
                            if (comp is CompProperties_Resource resource && resource.pipeNet == requirement.pipeNet)
                            {
                                foundNeededComp = true;
                                break;
                            }
                        }

                        if (!foundNeededComp)
                            yield return $"{parentDef.defName} require a CompProperties_Resource with pipeNet: {requirement.pipeNet.defName} (requirement {o + 1} of result {i + 1})";
                    }
                }

                if (result.pipeNet != null)
                {
                    var foundResultComp = false;
                    for (int j = 0; j < parentDef.comps.Count; j++)
                    {
                        var comp = parentDef.comps[j];
                        if (comp is CompProperties_Resource resource && resource.pipeNet == result.pipeNet)
                        {
                            foundResultComp = true;
                            break;
                        }
                    }

                    if (!foundResultComp)
                        yield return $"{parentDef.defName} require a CompProperties_Resource with pipeNet: {result.pipeNet.defName}";
                }
            }
        }
    }
}