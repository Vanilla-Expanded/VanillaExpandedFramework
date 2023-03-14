using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

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

        private List<Thing> ThingsList(out List<PipeNet> nets)
        {
            var list = new List<Thing>();
            var maps = Find.Maps;

            nets = new List<PipeNet>();

            for (int i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
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

                for (int o = 0; o < manager.noStorage.Count; o++)
                {
                    var net = manager.noStorage[o];
                    if (net.connectors.Count > 0)
                    {
                        list.Add(net.connectors[0].parent);
                        nets.Add(net);
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
