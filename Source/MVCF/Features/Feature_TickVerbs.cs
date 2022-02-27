using System.Collections.Generic;
using HarmonyLib;
using MVCF.Verbs;
using Verse;

namespace MVCF.Features
{
    public class Feature_TickVerbs : Feature
    {
        public override string Name => "TickVerbs";

        public override IEnumerable<Patch> GetPatches()
        {
            foreach (var patch in base.GetPatches()) yield return patch;

            yield return Patch.Postfix(AccessTools.Method(typeof(Verb), nameof(Verb.VerbTick)), AccessTools.Method(GetType(), nameof(TickVerb)));
        }

        public static void TickVerb(Verb __instance)
        {
            if (__instance is IVerbTick tick) tick.Tick();
        }
    }
}