using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.Noise;

namespace PipeSystem
{
    public class Alert_NoStorage : Alert
    {
        private static readonly Dictionary<Map, PipeNetManager> managers = new Dictionary<Map, PipeNetManager>();

        public Alert_NoStorage()
        {
            defaultLabel = "PipeSystem_NoStorageInNet".Translate();
            defaultPriority = AlertPriority.Medium;

            managers.Clear();
        }

        private PipeNetManager GetManager(Map map)
        {
            PipeNetManager manager;
            // Get and cache
            if (managers.ContainsKey(map))
            {
                manager = managers[map];
            }
            else
            {
                manager = map.GetComponent<PipeNetManager>();
                managers.Add(map, manager);
            }

            return manager;
        }

        private List<Thing> ThingsList(out List<PipeNet> nets)
        {
            var list = new List<Thing>();
            var maps = Find.Maps;

            nets = new List<PipeNet>();

            for (int m = 0; m < maps.Count; m++)
            {
                var map = maps[m];
                var manager = GetManager(map);
                for (int n = 0; n < manager.pipeNets.Count; n++)
                {
                    var net = manager.pipeNets[n];
                    // Only alert on wanted nets
                    if (net.def.noStorageAlert)
                    {
                        var cCount = net.connectors.Count;
                        // Single building net and in alert proof list
                        if (cCount == 1 && net.def.alertProofDefs.Contains(net.connectors[0].parent.def))
                            break;
                        // Check if any connector is fogged, if any: no alert
                        if (!net.def.foggedNetAlert)
                        {
                            for (int c = 0; c < cCount; c++)
                            {
                                if (net.connectors[c].parent.Position.Fogged(map)) break;
                            }
                        }
                        // Count storage
                        if (net.storages.Count == 0)
                        {
                            list.Add(net.connectors[0].parent);
                            nets.Add(net);
                        }
                    }
                }
            }

            return list;
        }

        public override TaggedString GetExplanation()
        {
            ThingsList(out List<PipeNet> nets);

            var report = new StringBuilder();
            report.AppendLine("PipeSystem_NoStorageInNetDesc".Translate());
            for (int i = 0; i < nets.Count; i++)
            {
                var net = nets[i];
                report.AppendLine($"- {net.def.resource.name.CapitalizeFirst()} network");
            }

            return report.ToString().TrimEndNewlines();
        }

        public override AlertReport GetReport() => VFECore.VFEGlobal.settings.enablePipeSystemNoStorageAlert ? AlertReport.CulpritsAre(ThingsList(out _)) : AlertReport.Inactive;
    }
}
