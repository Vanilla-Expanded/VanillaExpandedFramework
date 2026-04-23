using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VEF.Weapons
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
		public float arcSize = 2.5f;
		public bool debugMode;
		public bool wideAtStart;

		/// <summary>
		/// The projectile will always stop when hitting anything, as long as it matches <see cref="stopAtBuildingWithCover"/>.
		/// </summary>
		public bool stopWhenHit = true;
		/// <summary>
		/// If <see cref="stopWhenHitAt"/> is true, this determines the minimum fillPercent the building needs to have.
		/// </summary>
		public float stopAtBuildingWithCover = 1f;
		/// <summary>
		/// The projectile will always stop when hitting any natural rock, regardless of anything else. This includes rocks, smoothed rocks, as well as ores.
		/// </summary>
		public bool stopWhenNaturalRockHit = false;
		/// <summary>
		/// The projectile will always stop after hitting something when its damage is 0, regardless of anything else.
		/// </summary>
		public bool stopWhenZeroDamageAfterHit = false;
		/// <summary>
		/// The projectile will always stop when hitting anything whose defName is in this list, regardless of anything else.
		/// </summary>
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
