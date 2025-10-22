using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Plants
{

    [StaticConstructorOnStartup]
    public class Plant_WinterBlooming : Plant
    {
        private static Graphic GraphicSowing = GraphicDatabase.Get<Graphic_Single>("Things/Plant/Plant_Sowing", ShaderDatabase.Cutout, Vector2.one, Color.white);

        private static Graphic GraphicWinter = GraphicDatabase.Get<Graphic_Random>("Things/Plant/Trees/VCE_TreeCherry_Blossomed", ShaderDatabase.CutoutPlant, Vector2.one, Color.white);


        private bool? extensionPresent;
        private WinterBloomingExtension defExtension;
        public WinterBloomingExtension DefExtension
        {
            get
            {
                if (this.extensionPresent.HasValue)
                    return this.extensionPresent.Value ?
                               this.defExtension : null;
                this.extensionPresent = (this.defExtension = this.def.GetModExtension<WinterBloomingExtension>()) != null;
                return this.defExtension;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (this.LifeStage == PlantLifeStage.Sowing)
                {
                    return this.DefExtension?.graphicSowing?.Graphic ?? GraphicSowing;
                }
                Vector2 vector = Find.WorldGrid.LongLatOf(this.Map.Tile);
                Season season = GenDate.Season((long)Find.TickManager.TicksAbs, vector);
                if (season == Season.Winter)
                {
                    return this.DefExtension?.graphicWinter?.Graphic ?? GraphicWinter;
                }
                if (this.def.plant.leaflessGraphic != null && this.LeaflessNow && (!this.sown || !this.HarvestableNow))
                {
                    return this.def.plant.leaflessGraphic;
                }
                if (this.def.plant.immatureGraphic != null && !this.HarvestableNow)
                {
                    return this.def.plant.immatureGraphic;
                }

                return base.Graphic;
            }
        }

    }



}