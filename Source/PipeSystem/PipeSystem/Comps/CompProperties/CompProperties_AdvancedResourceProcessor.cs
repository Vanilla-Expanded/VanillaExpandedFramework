using System.Collections.Generic;
using RimWorld;
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
        // Wastepack bar
        public bool showWastepackBar = true;
        public Vector3 wastepackBarOffset = Vector3.zero;
        public bool stopWhenWastepackFull = false;
        // Result icon
        public bool showResultItem = false;
        public Vector3 resultItemOffset = Vector3.zero;
        public Vector3 resultItemSize = new Vector3(0.5f, 1, 0.5f);
        // Time required to put item inside processor
        public int ticksToFill = 200;
        // Heat push
        public bool heatPushWhileWorking = false;
        // Image changing
        public string inProgressTexture="";
        public string finishedTexture="";

        /// <summary>
        /// Config errors handling. Empty processes? Null translation key? Missing comps?
        /// </summary>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (!parentDef.inspectorTabs.NullOrEmpty() && !parentDef.inspectorTabs.Contains(typeof(ITab_Processor)))
                yield return $"CompProperties_AdvancedResourceProcessor parent need ITab_Processor";

            if (!(parentDef.tickerType == TickerType.Rare || parentDef.tickerType == TickerType.Normal))
                yield return $"CompProperties_AdvancedResourceProcessor parent need tickerType long, rare or normal";

            if (processes.NullOrEmpty())
                yield return $"CompProperties_AdvancedResourceProcessor cannot have empty or null <processes>";

            if (notWorkingKey == null)
                yield return $"CompProperties_AdvancedResourceProcessor cannot have null <notWorkingKey>";

            if ((showProgressBar || showWastepackBar) && parentDef.drawerType != DrawerType.MapMeshAndRealTime)
                yield return $"CompProperties_AdvancedResourceProcessor with showProgressBar true need MapMeshAndRealTime drawerType";

            if (heatPushWhileWorking && (parentDef.building == null || parentDef.building.heatPerTickWhileWorking <= 0f))
                yield return $"CompProperties_AdvancedResourceProcessor parent need building.heatPerTickWhileWorking to be more than 0";

            if (processes.Any(p => p.wastePackToProduce > 0) && !parentDef.HasSingleOrMultipleInteractionCells && parentDef.GetCompProperties<CompProperties_WasteProducer>() == null && parentDef.GetCompProperties<CompProperties_ThingContainer>() == null)
                yield return $"CompProperties_AdvancedResourceProcessor need interaction cell & CompProperties_WasteProducer & CompProperties_ThingContainer to be able to use <wastePackToProduce>";

            for (int i = 0; i < processes.Count; i++)
            {
                var result = processes[i];
                for (int o = 0; o < result.ingredients.Count; o++)
                {
                    var requirement = result.ingredients[o];
                    if (requirement.thing == null && requirement.pipeNet == null && requirement.thingCategory == null)
                        yield return $"Ingredient {o + 1} of result {i + 1} ({result.defName}) cannot have <thing>, <thingCategory> and <pipeNet> all null";

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

                for (int j = 0; j < result.results.Count; j++)
                {
                    var r = result.results[j];
                    if (r.pipeNet != null)
                    {
                        var foundResultComp = false;
                        for (int k = 0; k < parentDef.comps.Count; k++)
                        {
                            var comp = parentDef.comps[k];
                            if (comp is CompProperties_Resource resource && resource.pipeNet == r.pipeNet)
                            {
                                foundResultComp = true;
                                break;
                            }
                        }

                        if (!foundResultComp)
                            yield return $"{parentDef.defName} require a CompProperties_Resource with pipeNet: {r.pipeNet.defName}";
                    }
                }
            }
        }
    }
}