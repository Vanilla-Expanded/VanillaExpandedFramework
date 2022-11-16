using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Here we cach compResourceTraders and at startup we cache SignalOff and SignalOn. They are the same for all trader that use the same resource.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class CachedSignals
    {
        private static readonly List<string> signals = new List<string>();

        static CachedSignals()
        {
            var pipeNetDefs = DefDatabase<PipeNetDef>.AllDefsListForReading;
            for (int i = 0; i < pipeNetDefs.Count; i++)
            {
                var pipeNetDef = pipeNetDefs[i];
                signals.Add($"Resource{pipeNetDef.resource.name}TurnedOn");
                signals.Add($"Resource{pipeNetDef.resource.name}TurnedOff");
            }
        }

        public static bool IsResourceSignal(string signal) => signals.Contains(signal);
    }
}