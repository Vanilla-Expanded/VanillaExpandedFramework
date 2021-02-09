using System;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalBehaviours
{
    public class CompDestroyThisItem : ThingComp
    {
        public bool itemNeedsDestruction = false;


        public CompProperties_DestroyThisItem Props
        {
            get
            {
                return (CompProperties_DestroyThisItem)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (this.parent.Map != null)
            {
                DestroyableObjects_MapComponent mapComp = this.parent.Map.GetComponent<DestroyableObjects_MapComponent>();
                if (mapComp != null)
                {
                    mapComp.AddObjectToMap(this.parent);
                }
            }

        }

        public override void PostDeSpawn(Map map)
        {
            if (map != null)
            {
                DestroyableObjects_MapComponent mapComp = map.GetComponent<DestroyableObjects_MapComponent>();
                if (mapComp != null)
                {
                    mapComp.RemoveObjectFromMap(this.parent);
                }
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (previousMap != null)
            {
                DestroyableObjects_MapComponent mapComp = previousMap.GetComponent<DestroyableObjects_MapComponent>();
                if (mapComp != null)
                {
                    mapComp.RemoveObjectFromMap(this.parent);
                }
            }

        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<bool>(ref this.itemNeedsDestruction, "itemNeedsDestruction", false, false);

        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {



            if (itemNeedsDestruction)
            {
                yield return new Command_Action
                {
                    action = new Action(this.CancelObjectForDestruction),
                    hotKey = KeyBindingDefOf.Misc2,
                    defaultDesc = Props.buttonCancelDesc.Translate(),
                    icon = ContentFinder<Texture2D>.Get(Props.buttonCancelIcon, true),
                    defaultLabel = Props.buttonCancelLabel.Translate()
                };

            }
            else
            {

                yield return new Command_Action
                {
                    action = new Action(this.SetObjectForDestruction),
                    hotKey = KeyBindingDefOf.Misc2,
                    defaultDesc = Props.buttonDesc.Translate(),
                    icon = ContentFinder<Texture2D>.Get(Props.buttonIcon, true),
                    defaultLabel = Props.buttonLabel.Translate()
                };

            }



        }

        private void SetObjectForDestruction()
        {
            itemNeedsDestruction = true;

        }

        private void CancelObjectForDestruction()
        {
            itemNeedsDestruction = false;

        }

    }
}
