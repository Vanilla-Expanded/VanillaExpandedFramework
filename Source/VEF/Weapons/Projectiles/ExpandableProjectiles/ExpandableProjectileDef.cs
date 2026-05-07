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
		/// A filth that will be spawned if the projectile hits the maximum distance without being interrupted/stopped
		/// </summary>
		public ThingDef filthOnUninterrupted = null;
		/// <summary>
		/// A chance that <see cref="filthOnUninterrupted"/> will be spawned.
		/// </summary>
		public float filthOnUninterruptedChance = 1f;
		/// <summary>
		/// The amount of <see cref="filthOnUninterrupted"/> to spawn.
		/// </summary>
		public IntRange filthOnUninterruptedCount;

		/// <summary>
		/// The projectile will always stop when hitting anything, as long as it matches <see cref="stopAtBuildingWithCover"/>.
		/// </summary>
		public bool stopWhenHit = true;
		/// <summary>
		/// If <see cref="stopWhenHit"/> is true, this determines the minimum fillPercent the building needs to have.
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

		/// <summary>
		/// Properties specifically used by the <see cref="GaussProjectile"/>.
		/// </summary>
		public GaussProperties gauss;

		public bool IsGaussProjectile => thingClass.SameOrSubclassOf<GaussProjectile>();

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

		public override void ResolveReferences()
		{
			base.ResolveReferences();

			gauss ??= GaussProperties.DefaultProperties;
			gauss.ResolveReferences(this);
		}
    }
}
