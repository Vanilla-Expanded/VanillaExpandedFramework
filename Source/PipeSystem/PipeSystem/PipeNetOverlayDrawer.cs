using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VEF.Maps;
using Verse;

namespace PipeSystem
{
    public class PipeNetOverlayDrawer : MapComponent
    {
        private CustomOverlayDrawer overlayDrawer;

        public PipeNetOverlayDrawer(Map map) : base(map)
        {
        }

        public void TogglePulsing(Thing thing, Material mat, bool val)
        {
            if (mat == null)
                return;

            if (val)
            {
                overlayDrawer.Enable(thing, PSDefOf.PS_GenericPipeSystemOverlayPulsing);
                ((GenericPipeNetOverlayWorker)PSDefOf.PS_GenericPipeSystemOverlayPulsing.Worker).OverlayForThing[thing] = mat;
            }
            else
            {
                overlayDrawer.Disable(thing, PSDefOf.PS_GenericPipeSystemOverlayPulsing);
                ((GenericPipeNetOverlayWorker)PSDefOf.PS_GenericPipeSystemOverlayPulsing.Worker).OverlayForThing.Remove(thing);
            }
        }

        public void ToggleStatic(Thing thing, Material mat, bool val)
        {
            if (mat == null)
                return;

            if (val)
            {
                overlayDrawer.Enable(thing, PSDefOf.PS_GenericPipeSystemOverlayStatic);
                ((GenericPipeNetOverlayWorker)PSDefOf.PS_GenericPipeSystemOverlayStatic.Worker).OverlayForThing[thing] = mat;
            }
            else
            {
                overlayDrawer.Disable(thing, PSDefOf.PS_GenericPipeSystemOverlayStatic);
                ((GenericPipeNetOverlayWorker)PSDefOf.PS_GenericPipeSystemOverlayStatic.Worker).OverlayForThing.Remove(thing);
            }
        }

        public override void FinalizeInit() => overlayDrawer = map.GetComponent<CustomOverlayDrawer>();
    }
}