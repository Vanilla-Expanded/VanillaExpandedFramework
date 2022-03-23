using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Patch resolved designator getter. If there is any resource wanting a desconstruct designator here we add it to
    /// the "resolvedDesignators" list of the DesignationCategoryDef.
    /// </summary>
    [HarmonyPatch(typeof(DesignationCategoryDef))]
    [HarmonyPatch("ResolvedAllowedDesignators", MethodType.Getter)]
    public static class ResolvedAllowedDesignators_Patch
    {
        public static void Postfix(ref DesignationCategoryDef __instance, ref List<Designator> ___resolvedDesignators)
        {
            var pipeNetDefs = DefDatabase<PipeNetDef>.AllDefsListForReading;
            for (int i = 0; i < pipeNetDefs.Count; i++)
            {
                var net = pipeNetDefs[i];
                if (net.designator != null &&
                    net.designator.designationCategoryDef.defName == __instance.defName &&
                    !___resolvedDesignators.Any(d => d is Designator_DeconstructPipe ddp && ddp.pipeNetDef == net))
                {
                    ___resolvedDesignators.Add(new Designator_DeconstructPipe(net));
                }
            }
        }
    }
}