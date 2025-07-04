﻿using UnityEngine;
using Verse;

namespace VEF.Buildings
{
    public class CompProperties_BouncingArrow : CompProperties
    {
        public bool startBouncingArrowUponSpawning;
        public CompProperties_BouncingArrow()
        {
            compClass = typeof(CompBouncingArrow);
        }
    }


    [StaticConstructorOnStartup]
    public class CompBouncingArrow : ThingComp
    {
        private static readonly Material ArrowMatWhite = MaterialPool.MatFrom("UI/Overlays/Arrow", ShaderDatabase.CutoutFlying, Color.white);
        public bool doBouncingArrow;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent.def.GetCompProperties<CompProperties_BouncingArrow>()
                is CompProperties_BouncingArrow props && props.startBouncingArrowUponSpawning)
            {
                doBouncingArrow = true;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref doBouncingArrow, "doBouncingArrow");
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (doBouncingArrow)
            {
                var progress = Find.TickManager.TicksGame % 1000;
                if (progress < 500) progress = 1000 - progress;

                var pos = parent.DrawPos + (Vector3.forward * (1f + progress / 1000f));
                pos.y = AltitudeLayer.FogOfWar.AltitudeFor() + 1;
                var opacity = 1f - progress / 2000f;
                var rotation = Quaternion.AngleAxis(180, Vector3.up);

                ArrowMatWhite.color = new(1f, 1f, 1f, opacity);

                UnityEngine.Graphics.DrawMesh(MeshPool.plane10, pos, rotation, ArrowMatWhite, 0);
            }
        }
    }
}
