using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public class DoorTeleporter : ThingWithComps, IRenameable
    {
        public Material backgroundMat;
        public RenderTexture background1;
        public RenderTexture background2;
        public float rotation;
        public float distortAmount = 1.5f;
        public Vector2 backgroundOffset;
        public Sustainer sustainer;
        public string Name { get; set; }

        public string RenamableLabel
        {
            get
            {
                return Name ?? BaseLabel;
            }
            set
            {
                Name = value;
            }
        }

        public string BaseLabel => this.def.label;

        public string InspectLabel => RenamableLabel;

        public static Dictionary<ThingDef, DoorTeleporterMaterials> doorTeleporterMaterials = new();
        static DoorTeleporter()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (typeof(DoorTeleporter).IsAssignableFrom(def.thingClass))
                {
                    var doorMaterials = doorTeleporterMaterials[def] = new DoorTeleporterMaterials();
                    doorMaterials.Init(def);
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters.Add(this);
            var mat = doorTeleporterMaterials[this.def];
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                background1 = new RenderTexture(mat.backgroundTex.width, mat.backgroundTex.height, 0);
                background2 = new RenderTexture(mat.backgroundTex.width, mat.backgroundTex.height, 0);
                backgroundMat = new Material(ShaderDatabase.TransparentPostLight);
                this.RecacheBackground();
            });
        }

        public override void Tick()
        {
            base.Tick();
            this.rotation = (this.rotation + 0.5f) % 360f;
            this.distortAmount += 0.01f;
            if (this.distortAmount >= 3f) this.distortAmount = 1.5f;
            this.backgroundOffset += Vector2.one * 0.001f;
            this.RecacheBackground();
            var extension = this.def.GetModExtension<DoorTeleporterExtension>();
            if (extension.sustainer != null)
            {
                PlaySustainer(extension.sustainer);
            }
        }

        protected virtual void PlaySustainer(SoundDef soundDef)
        {
            if (this.sustainer == null || this.sustainer.Ended)
            {
                this.sustainer = soundDef.TrySpawnSustainer(this);
            }
            this.sustainer.Maintain();
        }

        public void RecacheBackground()
        {
            if (this.backgroundMat == null) return;
            var doorMaterials = doorTeleporterMaterials[def];
            Graphics.Blit(doorMaterials.backgroundTex, this.background1, Vector2.one, this.backgroundOffset, 0, 0);
            Graphics.Blit(this.background1, this.background2, doorMaterials.maskMat);
            this.backgroundMat.mainTexture = this.background2;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            this.sustainer?.End();
            this.sustainer = null;
            base.DeSpawn(mode);
            WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters.Remove(this);
            Object.Destroy(this.background1);
            Object.Destroy(this.background2);
            Object.Destroy(this.backgroundMat);
        }

        public Dictionary<Thing, Effecter> teleportEffecters = new Dictionary<Thing, Effecter>();
        public virtual void DoTeleportEffects(Thing thing, int ticksLeftThisToil,
            Map targetMap, ref IntVec3 targetCell, DoorTeleporter dest)
        {

        }

        public virtual void Teleport(Thing thing, Map mapTarget, IntVec3 cellTarget)
        {
            if (thing is Pawn pawn)
            {
                var carriedThing = pawn.carryTracker.CarriedThing;
                if (carriedThing != null)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out carriedThing);
                    carriedThing.DeSpawn();
                    GenSpawn.Spawn(carriedThing, cellTarget, mapTarget);
                }
                bool drafted = pawn.drafter != null && pawn.Drafted;
                bool selected = Find.Selector.IsSelected(pawn);
                pawn.teleporting = true;
                pawn.ClearAllReservations(false);
                pawn.ExitMap(false, Rot4.Invalid);
                pawn.teleporting = false;
                GenSpawn.Spawn(pawn, cellTarget, mapTarget);
                if (pawn.drafter != null)
                {
                    pawn.drafter.Drafted = drafted;
                }
                if (selected) Find.Selector.Select(pawn);
            }
            else
            {
                thing.DeSpawn();
                GenSpawn.Spawn(thing, cellTarget, mapTarget);
            }
            teleportEffecters.Remove(thing);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var doorMaterials = doorTeleporterMaterials[def];
            var drawSize = new Vector3(this.def.size.x, 1, this.def.size.z);
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(this.rotation, Vector3.up), drawSize * 1.5f), 
                doorMaterials.MainMat, 0);
            if (this.backgroundMat != null)
                Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawLoc - Altitudes.AltIncVect / 2, Quaternion.identity, drawSize * 1.5f),
                                  this.backgroundMat, 0);
            Graphics.DrawMesh(MeshPool.plane10,
                              Matrix4x4.TRS(drawLoc.Yto0() + Vector3.up * AltitudeLayer.MoteOverhead.AltitudeFor(), Quaternion.identity,
                                            drawSize * this.distortAmount * 2f),
                              doorMaterials.DistortionMat, 0);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) 
                yield return gizmo;
            foreach (Gizmo gizmo in GetDoorTeleporterGismoz())
                yield return gizmo;
        }
        public virtual IEnumerable<Gizmo> GetDoorTeleporterGismoz()
        {
            var extension = def.GetModExtension<DoorTeleporterExtension>();
            var doorMaterials = doorTeleporterMaterials[def];
            if (doorMaterials.DestroyIcon != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = extension.destroyLabelKey.Translate(),
                    defaultDesc = extension.destroyDescKey.Translate(),
                    icon = doorMaterials.DestroyIcon,
                    action = () => this.Destroy()
                };
            }

            if (doorMaterials.RenameIcon != null) 
            {
                yield return new Command_Action
                {
                    defaultLabel = extension.renameLabelKey.Translate(),
                    defaultDesc = extension.renameDescKey.Translate(),
                    icon = doorMaterials.RenameIcon,
                    action = () => Find.WindowStack.Add(new Dialog_RenameDoorTeleporter(this))
                };
            }
        }

        public override string GetInspectString()
        {
            var str = base.GetInspectString();
            var sb = str.Any() ? new StringBuilder(str + "\n") : new StringBuilder();
            sb.AppendLine("VEF.Name".Translate() + ": " + Name);
            return sb.ToString().TrimEndNewlines();
        }


        [HarmonyPatch(typeof(JobGiver_AIFollowPawn), "TryGiveJob")]
        public static class JobGiver_AIFollowPawn_TryGiveJob_Patch
        {
            private static MethodInfo GetFolloweeInfo = AccessTools.Method(typeof(JobGiver_AIFollowPawn), "GetFollowee");
            public static void Postfix(JobGiver_AIFollowPawn __instance, Pawn pawn, ref Job __result)
            {
                if (__result != null && pawn.CurJobDef == VFEDefOf.VEF_UseDoorTeleporter)
                {
                    __result = JobMaker.MakeJob(VFEDefOf.VEF_UseDoorTeleporter, pawn.CurJob.targetA);
                    __result.globalTarget = pawn.CurJob.globalTarget;
                }
                if (__result is null)
                {
                    var followee = GetFolloweeInfo.Invoke(__instance, new object[] { pawn }) as Pawn;
                    if (followee != null)
                    {
                        if (followee.Map != pawn.Map)
                        {
                            var doorTeleportersPawn = WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters
                                .Where(x => x.Map == pawn.Map && pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly))
                                .OrderBy(x => x.Position.DistanceTo(pawn.Position)).FirstOrDefault();
                            if (doorTeleportersPawn != null)
                            {
                                var doorTeleportersFollowee = WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters
                                .Where(x => x.Map == followee.Map && followee.CanReach(x, PathEndMode.OnCell, Danger.Deadly))
                                .OrderBy(x => x.Position.DistanceTo(followee.Position)).FirstOrDefault();
                                if (doorTeleportersFollowee != null)
                                {
                                    __result = JobMaker.MakeJob(VFEDefOf.VEF_UseDoorTeleporter, doorTeleportersPawn);
                                    __result.globalTarget = doorTeleportersFollowee;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn)) yield return option;
            if (!selPawn.CanReach(this, PathEndMode.OnCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("NoPath".Translate().CapitalizeFirst()), null);
            }
            else
            {
                foreach (DoorTeleporter doorTeleporter in WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters.Except(this))
                {
                    yield return new FloatMenuOption("VEF.TeleportTo".Translate(doorTeleporter.Name), () =>
                    {
                        Job job = JobMaker.MakeJob(VFEDefOf.VEF_UseDoorTeleporter, this);
                        job.globalTarget = doorTeleporter;
                        selPawn.jobs.StartJob(job, JobCondition.InterruptForced, canReturnCurJobToPool: true);
                        foreach (var otherPawn in selPawn.Map.mapPawns.AllPawnsSpawned)
                        {
                            if (otherPawn.CurJobDef == JobDefOf.FollowClose && otherPawn.CurJob.targetA.Pawn == selPawn)
                            {
                                Job job2 = JobMaker.MakeJob(VFEDefOf.VEF_UseDoorTeleporter, this);
                                job2.globalTarget = doorTeleporter;
                                var result = otherPawn.jobs.TryTakeOrderedJob(job2);
                            }
                        }
                    });
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            string name = this.Name;
            Scribe_Values.Look(ref name, nameof(name));
            this.Name = name;
        }
    }
}
