using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using Verse.AI;
using VanillaFurnitureExpanded;
using VFECore;

//These lootable buildings don't use base game IOpenable interface

namespace VFECore
{
    public class LootableBuilding_Custom : Building
    {

        MapComponent_InteractableBuildingsInMap cachedMapComp;
        LootableBuildingDetails cachedExtension;

        public LootableBuildingDetails LootableExtension
        {
            get
            {
                if (cachedExtension is null)
                {
                    cachedExtension = this.def.GetModExtension<LootableBuildingDetails>();
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

            if (InteractablesMapComp?.lootables_InMap.Contains(this) == false)
            {
                command_Action.defaultDesc = LootableExtension.gizmoDesc.Translate(this.LabelCap);
                command_Action.defaultLabel = LootableExtension.gizmoText.Translate(this.LabelCap);
                command_Action.icon = ContentFinder<Texture2D>.Get(LootableExtension.gizmoTexture, true);
                command_Action.hotKey = KeyBindingDefOf.Misc1;
                command_Action.action = delegate
                {
                    InteractablesMapComp?.AddLootableToMap(this);
                };
            }
            else
            {
                command_Action.defaultDesc = LootableExtension.gizmoDesc.Translate(this.LabelCap);
                command_Action.defaultLabel = LootableExtension.gizmoText.Translate(this.LabelCap);
                command_Action.icon = ContentFinder<Texture2D>.Get(LootableExtension.gizmoTexture, true);
                command_Action.Disabled = true;
            }
            yield return command_Action;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            InteractablesMapComp?.RemoveLootableFromMap(this);          
            base.Destroy(mode);
        }

        public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
        {
            InteractablesMapComp?.RemoveLootableFromMap(this);          
            base.Kill(dinfo, exactCulprit);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (InteractablesMapComp?.lootables_InMap.Contains(this) == true)
            {
                Vector3 drawPos = DrawPos;
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.181818187f;
                float num = ((float)Math.Sin((double)((Time.realtimeSinceStartup + 397f * (float)(thingIDNumber % 571)) * 4f)) + 1f) * 0.5f;
                num = 0.3f + num * 0.7f;
                Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom(LootableExtension.overlayTexture, ShaderDatabase.MetaOverlay), num);
                Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, material, 0);
            }
        }

        public void Open()
        {
            if (LootableExtension != null)
            {
                if (LootableExtension.randomFromContents)
                {
                    for (int i = 0; i < LootableExtension.totalRandomLoops.RandomInRange; i++)
                    {
                        ThingAndCount thingDefCount = LootableExtension.contents.RandomElement();
                        Thing thingToMake = ThingMaker.MakeThing(thingDefCount.thing, null);
                        thingToMake.stackCount = thingDefCount.count;
                        thingToMake.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                        GenPlace.TryPlaceThing(thingToMake, Position, Map, ThingPlaceMode.Near);
                    }

                }
                else
                {

                    foreach (ThingAndCount thingDefCount in LootableExtension.contents)
                    {
                        Thing thingToMake = ThingMaker.MakeThing(thingDefCount.thing, null);
                        thingToMake.stackCount = thingDefCount.count;
                        thingToMake.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                        GenPlace.TryPlaceThing(thingToMake, Position, Map, ThingPlaceMode.Near);
                    }
                }


                if (LootableExtension.buildingLeft != null)
                {
                    Thing buildingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(LootableExtension.buildingLeft), Position, Map);

                    if (buildingToMake.def.CanHaveFaction)
                    {
                        buildingToMake.SetFaction(this.Faction);
                    }
                }
                if (LootableExtension.deconstructSound != null)
                {
                    LootableExtension.deconstructSound.PlayOneShot(this);
                }
                if (this.Spawned)
                {
                    this.Destroy();
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
                )
            {
                if (!selPawn.CanReach(this, PathEndMode.OnCell, Danger.Deadly))
                {
                    yield return new FloatMenuOption("CannotUseReason".Translate("NoPath".Translate().CapitalizeFirst()), null);
                }
                else
                {
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(LootableExtension.gizmoText.Translate().CapitalizeFirst(), delegate
                    {
                        selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(InternalDefOf.VFE_Loot, this), JobTag.Misc);
                    }), selPawn, this);
                }



            }
        }

    }
}
