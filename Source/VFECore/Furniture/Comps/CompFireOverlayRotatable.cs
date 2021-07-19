using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaFurnitureExpanded
{
	public class CompProperties_FireOverlayRotatable : CompProperties_FireOverlay
	{
		public Vector3 northOffset;
		public Vector3 southOffset;
		public Vector3 westOffset;
		public Vector3 eastOffset;
		public CompProperties_FireOverlayRotatable()
        {
			this.compClass = typeof(CompFireOverlayRotatable);
        }
	}
	[StaticConstructorOnStartup]
	public class CompFireOverlayRotatable : CompFireOverlayBase
	{
		protected CompRefuelable refuelableComp;

		public static readonly Graphic FireGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Fire", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

		public new CompProperties_FireOverlayRotatable Props => (CompProperties_FireOverlayRotatable)props;

		[TweakValue("0M", -1, 1)] public static float yOffset;
		[TweakValue("0M", -1, 1)] public static float xOffset;
		[TweakValue("0M", -1, 1)] public static float zOffset;
		public override void PostDraw()
		{
			base.PostDraw();
			if (refuelableComp == null || refuelableComp.HasFuel)
			{
				Vector3 drawPos = parent.DrawPos;
				drawPos.y += 3f / 74f;
                switch (parent.Rotation.AsByte)
                {
					case 0: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.northOffset; break;
					case 1: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.eastOffset; break;
					case 2: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.southOffset; break;
					case 3: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.westOffset; break;
				}
				FireGraphic.Draw(drawPos, parent.Rotation, parent);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			refuelableComp = parent.GetComp<CompRefuelable>();
		}

		public override void CompTick()
		{
			if ((refuelableComp == null || refuelableComp.HasFuel) && startedGrowingAtTick < 0)
			{
				startedGrowingAtTick = GenTicks.TicksAbs;
			}
		}
	}
}
