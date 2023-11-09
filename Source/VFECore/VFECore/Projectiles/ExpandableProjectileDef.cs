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
		public float arcSize = 2.5f;
        public float stopAtBuildingWithCover = 1f;
		public bool debugMode;
		public bool wideAtStart;
        public List<string> stopWhenHitAt = new List<string>();
		protected override void ResolveIcon()
		{
			base.ResolveIcon();
			this.uiIcon = this.graphicData.Materials[0].mainTexture as Texture2D;
		}

        public override void PostLoad()
        {
            base.PostLoad();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.graphicData.InitMainTextures();
				this.graphicData.InitFadeOutTextures();
			});
		}
        public override IEnumerable<string> ConfigErrors()
        {
			return base.ConfigErrors();
        }
    }
}
