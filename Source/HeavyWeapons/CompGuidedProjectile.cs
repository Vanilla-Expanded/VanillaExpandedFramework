using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HeavyWeapons
{
	public class CompProperties_GuidedProjectile : CompProperties
    {
		public float hitChance;
		public bool selectDifferentTargets;
		public CompProperties_GuidedProjectile()
		{
			compClass = typeof(CompGuidedProjectile);
		}
	}
	public class CompGuidedProjectile : ThingComp
	{
		public CompProperties_GuidedProjectile Props => (CompProperties_GuidedProjectile)props;

        private GuidedProjectiles guidedProjectilesComp;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            guidedProjectilesComp = this.parent.Map.GetComponent<GuidedProjectiles>();
        }
        public override void CompTick()
        {
            base.CompTick();
			var projectile = this.parent as Projectile;
            if (this.Props.selectDifferentTargets)
            {
                if (guidedProjectilesComp.launcherTargets == null) guidedProjectilesComp.launcherTargets = new Dictionary<Thing, Targets>();
                if (guidedProjectilesComp.launcherTargets.TryGetValue(projectile.Launcher, out Targets targets) && projectile.Launcher is Pawn pawn)
                {
                    if (targets.targetInfos == null)
                    {
                        targets.targetInfos = new Dictionary<Projectile, LocalTargetInfo>();
                    }
                    if (!targets.targetInfos.ContainsKey(projectile)) // if projectile isn't guided yet...
                    {
                        bool changeTarget = false;
                        List<Projectile> keysToRemove = new List<Projectile>();
                        foreach (var targetInfo in targets.targetInfos)
                        {
                            if (!targetInfo.Key.Spawned || targetInfo.Key.Destroyed)
                            {
                                keysToRemove.Add(targetInfo.Key);
                            }
                        }

                        foreach (var key in keysToRemove)
                        {
                            targets.targetInfos.Remove(key);
                        }

                        foreach (var targetInfo in targets.targetInfos)
                        {
                            if (targetInfo.Value.Thing == projectile.intendedTarget.Thing)
                            {
                                changeTarget = true;
                                break;
                            }
                        }

                        if (changeTarget)
                        {
                            float maxDist = 8f;
                            maxDist = Mathf.Clamp(pawn.CurrentEffectiveVerb.verbProps.range * 0.66f, 2f, 20f);
                            Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToAll, (Thing x) =>
                                    !targets.targetInfos.Where(y => y.Value.Thing == x).Any(), 0f, maxDist);

                            if (thing != null)
                            {
                                projectile.intendedTarget = thing;
                            }
                        }
                        targets.targetInfos[projectile] = projectile.intendedTarget;
                    }
                }
                else
                {
                    var targets2 = new Targets();
                    targets2.targetInfos = new Dictionary<Projectile, LocalTargetInfo> { { projectile, projectile.intendedTarget } };
                    guidedProjectilesComp.launcherTargets[projectile.Launcher] = targets2;
                }
            }
            if (projectile.intendedTarget.IsValid)
            {
                var destinationCell = (IntVec3)AccessTools.PropertyGetter(typeof(Projectile), "DestinationCell").Invoke(projectile, new object[0]);
                if (destinationCell != projectile.intendedTarget.Cell)
                {
                    Traverse.Create(projectile).Field("destination").SetValue(projectile.intendedTarget.CenterVector3);
                }
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
        }
    }
}
