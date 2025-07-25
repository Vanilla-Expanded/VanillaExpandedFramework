﻿using System.Security.Cryptography;
using System;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace PipeSystem
{
    public class ITab_Processor : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(420f, 480f);

        private float viewHeight = 1000f;
        private Vector2 scrollPosition;

        public ITab_Processor()
        {
            size = WinSize;
            labelKey = "PipeSystem_Processes";
        }

        protected override void FillTab()
        {
            //var comp = SelThing.TryGetComp<CompAdvancedResourceProcessor>();


            var comp = CachedCompAdvancedProcessor.GetFor(SelThing);
            var rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            Widgets.BeginGroup(rect);
            // Add process button
            var processButton = new Rect(0f, 0f, 150f, 29f);
            if (Widgets.ButtonText(processButton, "PipeSystem_AddProcess".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(comp.ProcessesOptions));
            }
            UIHighlighter.HighlightOpportunity(processButton, "PipeSystem_AddProcess");
            // Add setting button
            if (!comp.Props.hideSettings)
            {
                var settingsButton = new Rect(rect.width - 24f - 16f, 2.5f, 24f, 24f);
                if (Widgets.ButtonImage(settingsButton, TexButton.OpenDebugActionsMenu))
                {
                    Find.WindowStack.Add(new FloatMenu(comp.Settings));
                }
            }

            if (ProcessUtility.Clipboard!=null)
            {
                var pasteButton = new Rect(rect.width - 24f - 32f -10f, 2.5f, 24f, 24f);
                if (Widgets.ButtonImage(pasteButton, TexButton.Paste, tooltip: "PipeSystem_PasteProcesses".Translate()))
                {
                    if (ProcessUtility.Clipboard.ContainsKey(SelThing.def))
                    {
                        comp.ProcessStack.Processes.Clear();
                        foreach (Process process in ProcessUtility.Clipboard[SelThing.def])
                        {
                            comp.ProcessStack.AddProcess(process.Def, (ThingWithComps)SelThing, process.targetCount,process.qualityToOutput);
                        }

                        foreach (Process process in comp.ProcessStack.Processes)
                        {
                            process.Progress = 0;
                        }
                    }
                    
                }
            }
            var copyButton = new Rect(rect.width - 24f - 48f - 16f, 2.5f, 24f, 24f);
            if (Widgets.ButtonImage(copyButton, TexButton.Copy, tooltip: "PipeSystem_CopyProcesses".Translate()))
            {
                ProcessUtility.Clipboard.Clear();
                ProcessUtility.Clipboard[SelThing.def]= comp.ProcessStack.Processes;
  
            }



            // Draw current process
            if (comp.ProcessStack?.FirstCanDo is Process pr)
            {
                pr.DoSimpleProgressInterface(new Rect(0f, 35f, rect.width - 16f, 24f));
            }
            else
            {
                var noProcessRect = new Rect(0f, 35f, rect.width - 16f, 24f);
                Widgets.DrawHighlight(noProcessRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(noProcessRect, "PipeSystem_NoProcess".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
            // Draw processes
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            var outRect = new Rect(0f, 65f, rect.width, rect.height - 35f - 24f - 6f - 6f);
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