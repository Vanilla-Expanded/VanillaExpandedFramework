using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VanillaApparelExpanded
{
    /// <summary>
    /// New Comp that allows switching between different apparels, but only if they have the same layers and bodypartgroups. 
    /// XML should be as below:
    /// <li Class="VanillaApparelExpanded.CompProperties_SwitchApparel">
    ///		<SwitchTo></SwitchTo>                   ThingDef to switch to
    ///		<graphicPath></graphicPath>                     graphicPath for the gizmo icon
    ///		<label></label>                                 description label for the gizmo
    ///	</li>
    /// </summary>

    /// LOG 25/03: Layers and bodypartsgroups dont't need to be equal currently in both ThingDefs. Code is therefore commented out and doesn't work for unknown reason.

    [StaticConstructorOnStartup]
    public class CompSwitchApparel : ThingComp
    {
        // Make the CompProperties accessible.
        public CompProperties_SwitchApparel Props
        {
            get
            {
                return (CompProperties_SwitchApparel)this.props;
            }
        }

        // Destory old item and create new one. Inherit stuff type, healthpoints and quality.
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {

            // Check if graphicPath returns an actual texture.
            if (!ContentFinder<Texture2D>.Get(Props.graphicPath))
            {
                Log.Error("No Gizmo texture found");
            }

            return base.CompGetWornGizmosExtra().Append(new Command_Action
            {

                defaultLabel = "Switch",
                defaultDesc = $"Switch to {Props.SwitchTo.label} \n{Props.Label}",
                icon = ContentFinder<Texture2D>.Get(Props.graphicPath),
                action = delegate
                {
                    Pawn casterPawn = (this.parent as Apparel).Wearer;

                    // Check if both the old and new apparel items have the same layers and bodypartgroups
                    if (true) // Enumerable.SequenceEqual(Props.SwitchTo.apparel.layers, parent.def.apparel.layers)
                    {
                        if (true) //Enumerable.SequenceEqual(Props.SwitchTo.apparel.bodyPartGroups,parent.def.apparel.bodyPartGroups)
                        {
                            // Save old item health
                            int oldItemHealth = this.parent.HitPoints;

                            ThingDef oldItemStuff = null;

                            // Save old item stuff type
                            if (this.parent.Stuff != null)
                            {
                                oldItemStuff = this.parent.Stuff;
                            }

                            // Create new item with same stuff type
                            var item = ThingMaker.MakeThing(Props.SwitchTo, oldItemStuff);

                            if (item != null)
                            {
                                // Set new item health to old one
                                item.HitPoints = oldItemHealth;

                                // If the new item has CompQuality set the quality to the same one as the old item
                                if (this.parent.TryGetQuality(out QualityCategory qc) && item.TryGetComp<CompQuality>() != null)
                                {
                                    item.TryGetComp<CompQuality>().SetQuality(qc, ArtGenerationContext.Colony);
                                }

                                // Cast item from Thing to Apparel
                                Apparel itemAsApparel = (Apparel)item;

                                // Destroy old item
                                this.parent.Destroy();

                                // Create new one
                                casterPawn.apparel.Wear(itemAsApparel);

                            }
                        }
                        
                    }
                    
                }
            });
        }

    }
}
