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
            var value = container.PatchOperationValue(id, defaultValue);
            DoSlider(list, label + ": " + value, ref value, value.ToString(), range.TrueMin, range.TrueMax, tooltip);
            container.patchOperationValues[id] = value;
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
