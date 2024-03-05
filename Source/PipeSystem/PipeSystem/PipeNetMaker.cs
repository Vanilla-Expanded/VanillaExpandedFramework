using System;
using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public static class PipeNetMaker
    {
        public static PipeNet MakePipeNet(IEnumerable<CompResource> connectors, Map map, PipeNetDef def)
        {
            PipeNet pipeNet = (PipeNet)Activator.CreateInstance(def.pipeNetClass);
            pipeNet.map = map;
            pipeNet.def = def;
            pipeNet.networkGrid = new BoolGrid(map);

            pipeNet.NextTick = Find.TickManager.TicksGame;

            // Register all
            foreach (var connector in connectors)
            {
                pipeNet.RegisterComp(connector);
            }

            return pipeNet;
        }
    }
}