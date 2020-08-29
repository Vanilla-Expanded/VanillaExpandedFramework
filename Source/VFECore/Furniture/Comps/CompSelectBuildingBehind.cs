using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaFurnitureExpanded
{
    public class CompSelectBuildingBehind : ThingComp
    {

        //A very simple comp class that adds a Gizmo command button that deselects this item, and selects a buildingToSelect
        //in the same tile.

        //This is used in holograms to add a "Select Base" button, but can be used with any other two Buildings that want to
        //share the same space

        private Texture2D cachedCommandTex;
        public Thing building;

        public CompProperties_SelectBuildingBehind Props
        {
            get
            {
                return (CompProperties_SelectBuildingBehind)this.props;
            }
        }

        private Texture2D CommandTex
        {
            get
            {
                if (this.cachedCommandTex == null)
                {
                    this.cachedCommandTex = ContentFinder<Texture2D>.Get(this.Props.commandButtonImage, true);
                }
                return this.cachedCommandTex;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            List<Thing> list = this.parent.Map.thingGrid.ThingsListAt(this.parent.Position);
            for (int i = 0; i < list.Count; i++)
            {
                if ((list[i] is Building) && list[i].def.defName == Props.buildingToSelect)
                {
                    building = list[i];
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
           
            if (this.parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    hotKey = KeyBindingDefOf.Command_TogglePower,
                    icon = CommandTex,
                    defaultLabel = this.Props.commandButtonText.Translate(),
                    defaultDesc = this.Props.commandButtonDesc.Translate(),
                    action = delegate ()
                    {
                        Find.Selector.Deselect(this.parent);
                        Find.Selector.Select(building);
                    }


                    
                };
            }
            yield break;
           
        }



    }
}
