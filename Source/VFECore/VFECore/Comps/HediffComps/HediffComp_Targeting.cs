using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFECore
{
	public class HediffCompProperties_Targeting : HediffCompProperties
	{
		public bool neverMiss;
		public ThingDef targetingMote;
		public float initialTargetingMoteScale;
		public string targetingLineTexPath;
		public Color targetingLineColor = Color.red;
		public float targetingLineWidth = 0.2f;
		public HediffCompProperties_Targeting()
		{
			compClass = typeof(HediffComp_Targeting);
		}
	}

	public class HediffComp_Targeting : HediffComp
	{
		public Mote mote;
		public HediffCompProperties_Targeting Props => base.props as HediffCompProperties_Targeting;

		private Material targetingLine;
		public Material TargetingLine
        {
            get
            {
				if (targetingLine == null)
                {
					targetingLine = MaterialPool.MatFrom(Props.targetingLineTexPath, ShaderDatabase.Transparent, Props.targetingLineColor);
				}
				return targetingLine;
            }
        }

		public Action actionOnTick;
		public void DrawTargetingEffects(LocalTargetInfo target, float progress)
        {
			if (mote is null || mote.Destroyed)
            {
				actionOnTick = delegate
				{
					if (target.HasThing)
					{
						Log.Message("Making attached mote");
						mote = MoteMaker.MakeAttachedOverlay(target.Thing, Props.targetingMote, Vector3.zero, Props.initialTargetingMoteScale);
					}
					else
					{
						Log.Message("Making static mote");
						mote = MakeStaticMote(target.CenterVector3, Pawn.Map, Props.targetingMote, Props.initialTargetingMoteScale);
					}
				};
			}
			else
            {
				mote.Scale = progress;
				mote.Maintain();
			}
			if (!Props.targetingLineTexPath.NullOrEmpty())
            {
				Vector3 b = ((!target.HasThing) ? target.Cell.ToVector3Shifted() : target.Thing.TrueCenter());
				Vector3 a = this.Pawn.TrueCenter();
				b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				a.y = b.y;
				GenDraw.DrawLineBetween(a, b, TargetingLine, Props.targetingLineWidth);
			}
		}

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
			if (actionOnTick != null)
            {
				actionOnTick();
				actionOnTick = null;
            }
        }

        public static Mote MakeStaticMote(Vector3 loc, Map map, ThingDef moteDef, float scale = 1f)
		{
			Mote obj = (Mote)ThingMaker.MakeThing(moteDef);
			obj.exactPosition = loc;
			obj.Scale = scale;
			GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
			return obj;
		}

		public override void CompExposeData()
        {
            base.CompExposeData();
			Scribe_References.Look(ref mote, "mote");
        }
    }
}