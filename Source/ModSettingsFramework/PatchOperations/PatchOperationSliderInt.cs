using System.Xml;
using Verse;

namespace ModSettingsFramework
{
    public class PatchOperationSliderInt : PatchOperationModSettings
    {
        protected string xpath;
        public IntRange range;
        public int defaultValue;

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            var value = (int)container.PatchOperationValue(id, defaultValue);
            value = (int)list.SliderLabeled(label + ": " + value, value, range.TrueMin,
                range.TrueMax, tooltip: tooltip);
            container.patchOperationValues[id] = value;
            list.Gap(5);
        }

        public override int SettingsHeight()
        {
            return 29;
        }
        public override bool ApplyWorker(XmlDocument xml)
        {
            if (CanRun())
            {
                var node = xml.SelectSingleNode(xpath);
                var container = SettingsContainer;
                if (container != null)
                {
                    var value = container.PatchOperationValue(id, defaultValue);
                    node.InnerText = value.ToString();
                }
            }
            return true;
        }
    }
}
