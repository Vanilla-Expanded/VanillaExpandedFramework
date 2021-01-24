using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore
{
	public class ExpandableProjectileDef : ThingDef
	{
		public int lifeTimeDuration = 100;
		public float widthScaleFactor = 1f;
		public float heightScaleFactor = 1f;
		public Vector3 startingPositionOffset = Vector3.zero;
		public float totalSizeScale = 1f;
		public new ExpandableGraphicData graphicData;
		public int tickFrameRate = 1;
		public int finalTickFrameRate = 0;
		public int tickDamageRate = 60;
		public float minDistanceToAffect;
		public bool disableVanillaDamageMethod;
		public bool dealsDamageOnce;
		public bool reachMaxRangeAlways;
		public bool stopWhenHit = true;
		protected override void ResolveIcon()
		{
			base.ResolveIcon();
			this.uiIcon = this.graphicData.Materials[0].mainTexture as Texture2D;
		}
	}
}
