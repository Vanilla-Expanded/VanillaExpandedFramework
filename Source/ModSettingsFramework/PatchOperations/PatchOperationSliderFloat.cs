using System;
using System.Linq;
using System.Xml;
using Verse;

namespace ModSettingsFramework
{
    public class PatchOperationSliderFloat : PatchOperationModSettings
    {
        protected string xpath;
        public FloatRange range;
        public float defaultValue;
        public int roundToDecimalPlaces = 2;

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            var value = container.PatchOperationValue(id, defaultValue);
            value = (float)Math.Round(list.SliderLabeled(label + ": " + value, value, range.TrueMin,
                range.TrueMax, tooltip: tooltip), roundToDecimalPlaces);
            container.patchOperationValues[id] = value;
        }

        public override int SettingsHeight()
        {
            return 32;
        }

        public override bool ApplyWorker(XmlDocument xml)
        {
            if (CanRun())
            {
                var node = xml.SelectSingleNode(xpath);
                var container = SettingsContainer;
                if (node != null)
                {
                    var value = container.PatchOperationValue(id, defaultValue);
                    node.InnerText = value.ToString();
                }
            }
            return true;
        }
    }
}
