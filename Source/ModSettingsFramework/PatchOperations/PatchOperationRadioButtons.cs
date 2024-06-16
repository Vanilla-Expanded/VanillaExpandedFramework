using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;

namespace ModSettingsFramework
{
    public class PatchOperationRadioButtons : PatchOperationModSettings
    {
        public List<PatchOperationModOption> options;
        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            DoLabel(list, label, tooltip);
            var firstActive = options.FirstOrDefault(x => container.PatchOperationEnabled(x.id, x.defaultValue)).id;
            foreach (var option in options)
            {
                if (DoRadioButton(list, option.label, option.id == firstActive, 15f, 0, option.tooltip, null, false))
                {
                    container.patchOperationStates[option.id] = true;
                    foreach (var option2 in options)
                    {
                        if (option2 != option)
                        {
                            container.patchOperationStates[option2.id] = false;
                        }
                    }
                }
            }
        }

        public bool DoRadioButton(Listing_Standard list, string label, bool active, float tabIn, 
            float tabInRight, string tooltip, float? tooltipDelay, bool disabled)
        {
            float lineHeight = Text.LineHeight;
            Rect rect = list.GetRect(lineHeight);
            rect.xMin += tabIn;
            rect.xMax -= tabInRight;
            if (list.BoundingRectCached.HasValue && !rect.Overlaps(list.BoundingRectCached.Value))
            {
                return false;
            }
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                TipSignal tip = (tooltipDelay.HasValue ? new TipSignal(tooltip, tooltipDelay.Value) : new TipSignal(tooltip));
                TooltipHandler.TipRegion(rect, tip);
            }
            bool result = Widgets.RadioButtonLabeled(rect, label, active, disabled);
            list.Gap(list.verticalSpacing);
            scrollHeight += rect.height + list.verticalSpacing;
            return result;
        }

        public override bool ApplyWorker(XmlDocument xml)
        {
            if (CanRun())
            {
                foreach (var option in options)
                {
                    option.SettingsContainer = SettingsContainer;
                    if (SettingsContainer.PatchOperationEnabled(option.id, option.defaultValue))
                    {
                        option.Apply(xml);
                    }
                }
            }
            return true;
        }

        public void Reset()
        {
            var container = SettingsContainer;
            foreach (var option in options)
            {
                container.patchOperationStates.Remove(option.id);
            }
        }
    }
}
