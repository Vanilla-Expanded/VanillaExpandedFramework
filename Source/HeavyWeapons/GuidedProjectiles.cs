using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HeavyWeapons
{
	public class Targets : IExposable
	{
		public Dictionary<Projectile, LocalTargetInfo> targetInfos;
		public Targets()
		{

		}
		public void ExposeData()
		{
			Scribe_Collections.Look(ref targetInfos, "targetInfos", LookMode.Reference, LookMode.LocalTargetInfo, ref thingKeys, ref targetValues);
		}

		private List<Projectile> thingKeys;
		private List<LocalTargetInfo> targetValues;

	}
	public class GuidedProjectiles : MapComponent
	{
		public GuidedProjectiles(Map map) : base(map)
		{

		}

		public Dictionary<Thing, Targets> launcherTargets = new Dictionary<Thing, Targets>();

		public void RegisterTarget(Pawn launcher, LocalTargetInfo target)
        {

        }
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref launcherTargets, "launcherTargets", LookMode.Reference, LookMode.Deep, ref thingKeys, ref targetValues);
		}

		private List<Thing> thingKeys;
		private List<Targets> targetValues;

	}
}
