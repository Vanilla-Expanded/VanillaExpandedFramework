using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using LudeonTK;

namespace VanillaFurnitureExpanded
{
	public class CompProperties_FireOverlayRotatable : CompProperties_FireOverlay
	{
		public Vector3 northOffset;
		public Vector3 southOffset;
		public Vector3 westOffset;
		public Vector3 eastOffset;
		public string texPath = "Things/Special/Fire";
		public Color color = Color.white;
		public Vector2 size = Vector2.one;
		public CompProperties_FireOverlayRotatable()
        {
			this.compClass = typeof(CompFireOverlayRotatable);
        }
	}
	[StaticConstructorOnStartup]
	public class CompFireOverlayRotatable : CompFireOverlayBase
	{
		protected CompRefuelable refuelableComp;

		public Graphic cachedGraphic;
		public  Graphic FireGraphic
        {
            get
            {
				if (cachedGraphic == null)
                {
					cachedGraphic = GraphicDatabase.Get<Graphic_Flicker>(Props.texPath, ShaderDatabase.TransparentPostLight, Props.size, Props.color);
				}
				return cachedGraphic;
			}
        }

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
                switch (parent.Rotation.AsByte)
                {
					case 0: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.northOffset; break;
					case 1: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.eastOffset; break;
					case 2: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.southOffset; break;
					case 3: drawPos += Quaternion.Euler(0, parent.Rotation.AsAngle, 0) * Props.westOffset; break;
				}
				drawPos.y += 0.05f;
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
