using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class ITab_Processor : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(420f, 480f);
        private static Color WindowBGBorderColor;

        private float viewHeight = 1000f;
        private Vector2 scrollPosition;

        public ITab_Processor()
        {
            size = WinSize;
            labelKey = "PipeSystem_Processes";
            WindowBGBorderColor = new ColorInt(97, 108, 122).ToColor;
        }

        protected override void FillTab()
        {
            var comp = CachedCompAdvancedProcessor.GetFor(SelThing);
            var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            Widgets.BeginGroup(rect);
            // Add def button
            var processButton = new Rect(0f, 0f, 150f, 29f);
            if (Widgets.ButtonText(processButton, "PipeSystem_AddProcess".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(comp.ProcessesOptions));
            }
            UIHighlighter.HighlightOpportunity(processButton, "PipeSystem_AddProcess");
            // Draw current process
            comp.ProcessStack?.FirstCanDo?.DoSimpleProgressInterface(new Rect(156f, 2.5f, rect.width - 150f - 6f - 16f, 24f));
            // Draw processes
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            var outRect = new Rect(0f, 35f, rect.width, rect.height - 35f - 6f);
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var processes = comp.ProcessStack.Processes;
            var num = 0f;
            for (int i = 0; i < processes.Count; i++)
            {
                var process = processes[i];
                var processRect = process.DoInterface(0f, num, viewRect.width, i);
                num += processRect.height + 6f;
            }
            if (Event.current.type == EventType.Layout)
            {
                viewHeight = num + 60f;
            }
            Widgets.EndScrollView();
            Widgets.EndGroup();
        }
    }
}
