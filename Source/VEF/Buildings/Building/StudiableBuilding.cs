using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using static HarmonyLib.Code;
using Verse.Noise;
using Verse.AI;



namespace VEF.Buildings
{
    public class StudiableBuilding : Building
    {

        MapComponent_InteractableBuildingsInMap cachedMapComp;
        StudiableBuildingDetails cachedExtension;

        public StudiableBuildingDetails StudiableExtension
        {
            get
            {
                if (cachedExtension is null)
                {
                    cachedExtension = this.def.GetModExtension<StudiableBuildingDetails>();
                }
                return cachedExtension;
            }
        }
        public MapComponent_InteractableBuildingsInMap InteractablesMapComp
        {
            get
            {
                if (cachedMapComp is null)
                {
                    cachedMapComp = Map.GetComponent<MapComponent_InteractableBuildingsInMap>(); ;
                }
                return cachedMapComp;
            }
        }



        public override IEnumerable<Gizmo> GetGizmos()
        {

            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            Command_Action command_Action = new Command_Action();

            if (InteractablesMapComp?.studiables_InMap.Contains(this) == false)
            {
                command_Action.defaultDesc = StudiableExtension.gizmoDesc.Translate();

                command_Action.defaultLabel = StudiableExtension.gizmoText.Translate();
                command_Action.icon = ContentFinder<Texture2D>.Get(StudiableExtension.gizmoTexture, true);
                command_Action.hotKey = KeyBindingDefOf.Misc1;
                command_Action.action = delegate
                {
                    InteractablesMapComp?.AddStudiablesToMap(this);
                };
            }
            else
            {
                command_Action.defaultDesc = StudiableExtension.gizmoDesc.Translate();
                command_Action.defaultLabel = StudiableExtension.gizmoText.Translate();
                command_Action.icon = ContentFinder<Texture2D>.Get(StudiableExtension.gizmoTexture, true);
                command_Action.Disabled = true;
            }

            yield return command_Action;

        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {

            InteractablesMapComp?.RemoveStudiablesFromMap(this);

            base.Destroy(mode);

        }

        public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
        {

            InteractablesMapComp?.RemoveStudiablesFromMap(this);

            base.Kill(dinfo, exactCulprit);

        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (InteractablesMapComp?.studiables_InMap.Contains(this) == true)
            {
                Vector3 drawPos = DrawPos;
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.181818187f;
                float num = ((float)Math.Sin((double)((Time.realtimeSinceStartup + 397f * (float)(thingIDNumber % 571)) * 4f)) + 1f) * 0.5f;
                num = 0.3f + num * 0.7f;
                Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom(StudiableExtension.overlayTexture, ShaderDatabase.MetaOverlay), num);
                UnityEngine.Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, material, 0);
            }
        }

        public virtual void Study(Pawn pawn)
        {

            if (StudiableExtension != null)
            {

                if (StudiableExtension.buildingLeft != null)
                {


                    Thing buildingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(StudiableExtension.buildingLeft), Position, Map, Rotation);

                    if (buildingToMake.def.CanHaveFaction)
                    {
                        buildingToMake.SetFaction(this.Faction);
                    }
                }
                if (StudiableExtension.deconstructSound != null)
                {
                    StudiableExtension.deconstructSound.PlayOneShot(this);
                }

                if (StudiableExtension.craftingInspiration)
                {
                    pawn.mindState.inspirationHandler.TryStartInspiration(InspirationDefOf.Inspired_Creativity);
                }

                if (this.Spawned)
                {
                    this.DeSpawn();
                }

            }


        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }
            if (selPawn.CanReserve(this) && selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation)
                && !selPawn.skills.GetSkill(SkillDefOf.Intellectual).TotallyDisabled)
            {
                if (!selPawn.CanReach(this, PathEndMode.OnCell, Danger.Deadly))
                {
                    yield return new FloatMenuOption("CannotUseReason".Translate("NoPath".Translate().CapitalizeFirst()), null);
                }
                else
                {
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(StudiableExtension.gizmoText.Translate().CapitalizeFirst(), delegate
                    {
                        selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(InternalDefOf.VFE_StudyBuilding, this), JobTag.Misc);
                    }), selPawn, this);
                }



            }
        }


    }
}
