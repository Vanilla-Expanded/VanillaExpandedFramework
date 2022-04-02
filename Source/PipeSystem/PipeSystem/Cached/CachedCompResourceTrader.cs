using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Here we cach compResourceTraders and at startup we cache SignalOff and SignalOn. They are the same for all trader that use the same resource.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class CachedCompResourceTrader
    {
        public static readonly Dictionary<ThingWithComps, List<CompResourceTrader>> cachedCompResourceTrader = new Dictionary<ThingWithComps, List<CompResourceTrader>>();
        public static readonly Dictionary<PipeNetDef, string[]> cachedResourceDefSignals = new Dictionary<PipeNetDef, string[]>();

        // Cache signals here
        static CachedCompResourceTrader()
        {
            var pipeNetDefs = DefDatabase<PipeNetDef>.AllDefsListForReading;
            for (int i = 0; i < pipeNetDefs.Count; i++)
            {
                var pipeNetDef = pipeNetDefs[i];
                cachedResourceDefSignals.Add(pipeNetDef, new string[] { $"Resource{pipeNetDef.resource.name}TurnedOn", $"Resource{pipeNetDef.resource.name}TurnedOff" });
            }
        }

        public static void TryAdd(ThingWithComps thing)
        {
            if (!cachedCompResourceTrader.ContainsKey(thing))
                cachedCompResourceTrader.Add(thing, thing.GetComps<CompResourceTrader>().ToList());
        }

        public static bool AllResourceOn(ThingWithComps thing)
        {
            var compsList = cachedCompResourceTrader[thing];
            for (int i = 0; i < compsList.Count; i++)
            {
                var comp = compsList[i];
                if (comp.PipeNet == null || !comp.ResourceOn)
                    return false;
            }

            return true;
        }

        public static bool IsCompMessage(ThingWithComps thing, string msg)
        {
            var compsList = cachedCompResourceTrader[thing];

            for (int i = 0; i < compsList.Count; i++)
            {
                var signals = cachedResourceDefSignals[compsList[i].Props.pipeNet];
                if (signals[0] == msg || signals[1] == msg)
                {
                    return true;
                }
            }
            return false;
        }
    }
}