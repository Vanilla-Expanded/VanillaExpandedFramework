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
	[HotSwappable]
	public class FlamethrowProjectile : ExpandableProjectile
	{
		public HashSet<IntVec3> affectedCells = new HashSet<IntVec3>();
		public override void DoDamage(IntVec3 pos)
		{
			base.DoDamage(pos);
			try
			{
				if (pos != this.launcher.Position && this.Map != null && GenGrid.InBounds(pos, this.Map))
				{
					if (affectedCells.Contains(pos) is false) 
					{
                        FilthMaker.TryMakeFilth(pos, Map, ThingDefOf.Filth_Ash);
                        ThrowSmoke(pos.ToVector3Shifted(), Map, 1f);
                        ThrowMicroSparks(pos.ToVector3Shifted(), base.Map);
                        affectedCells.Add(pos);
                    }
                    var list = this.Map.thingGrid.ThingsListAt(pos);
					for (int num = list.Count - 1; num >= 0; num--)
					{
						if (IsDamagable(list[num]))
						{
							if (!list.Where(x => x.def == ThingDefOf.Fire).Any())
							{
								CompAttachBase compAttachBase = list[num].TryGetComp<CompAttachBase>();
								Fire obj = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
								obj.fireSize = 1f;
								GenSpawn.Spawn(obj, list[num].Position, list[num].Map, Rot4.North);
								if (compAttachBase != null)
								{
									obj.AttachTo(list[num]);
									Pawn pawn = list[num] as Pawn;
									if (pawn != null)
									{
										pawn.jobs.StopAll();
										pawn.records.Increment(RecordDefOf.TimesOnFire);
									}
								}
							}
							this.customImpact = true;
							base.Impact(list[num]);
							this.customImpact = false;
						}
					}
				}
			}
			catch { };
		}

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Collections.Look(ref affectedCells, "affectedCells", LookMode.Value);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				affectedCells ??= new HashSet<IntVec3>();
			}
        }
        public static void ThrowSmoke(Vector3 loc, Map map, float size)
        {
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.Smoke, Rand.Range(1.5f, 2.5f) * size);
            dataStatic.rotationRate = Rand.Range(-30f, 30f);
            dataStatic.velocityAngle = Rand.Range(30, 40);
            dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
            dataStatic.instanceColor = Color.black;
            map.flecks.CreateFleck(dataStatic);
        }

        public static void ThrowMicroSparks(Vector3 loc, Map map)
        {
            loc -= new Vector3(0.5f, 0f, 0.5f);
            loc += new Vector3(Rand.Value, 0f, Rand.Value);
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.MicroSparks, Rand.Range(0.8f, 1.2f));
            dataStatic.rotationRate = Rand.Range(-12f, 12f);
            dataStatic.velocityAngle = Rand.Range(35, 45);
            dataStatic.velocitySpeed = 1.2f;
            map.flecks.CreateFleck(dataStatic);
        }

        public override bool IsDamagable(Thing t)
        {
            return base.IsDamagable(t) && t.def != ThingDefOf.Fire;
        }
    }
}
