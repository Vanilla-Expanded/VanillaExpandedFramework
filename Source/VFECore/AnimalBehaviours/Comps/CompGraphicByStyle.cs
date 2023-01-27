using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    [StaticConstructorOnStartup]
    public class CompGraphicByStyle : ThingComp
    {

        private PawnRenderer pawn_renderer;
        public Graphic dessicatedGraphic;
        public int changeGraphicsCounter = 0;



        public CompProperties_GraphicByStyle Props
        {
            get
            {
                return (CompProperties_GraphicByStyle)this.props;
            }
        }

        public override void CompTick()
        {
            changeGraphicsCounter++;
            if (changeGraphicsCounter > Props.changeGraphicsInterval)
            {
                this.ChangeTheGraphics();
                changeGraphicsCounter = 0;
            }
            base.CompTick();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Pawn pawn = this.parent as Pawn;
            this.pawn_renderer = pawn.Drawer.renderer;

            if (Props.changeDesiccatedGraphic) {
                GraphicData dessicatedgraphicdata = new GraphicData();
                dessicatedgraphicdata.texPath = Props.dessicatedTxt;
                dessicatedGraphic = dessicatedgraphicdata.Graphic;
            }
            
            this.ChangeTheGraphics();

        }




        public void ChangeTheGraphics()
        {

            if (this.parent.Map != null && this.parent.Faction == Faction.OfPlayer && AnimalBehaviours_Settings.flagGraphicChanging)
            {
                bool styleFound = false;
                List<ThingStyleCategoryWithPriority> listStyles = Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.thingStyleCategories;
                foreach (ThingStyleCategoryWithPriority listStyle in listStyles)
                {
                    if (listStyle.category == Props.style)
                    {
                        styleFound = true;
                    }

                }

                Pawn pawn = this.parent as Pawn;
                if (this.pawn_renderer == null)
                {
                    this.pawn_renderer = pawn.Drawer.renderer;

                }

                Vector2 vector = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;

                if (styleFound) {
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                    if (this.pawn_renderer != null)
                    {
                        try
                        {
                                var data = new GraphicData();
                                data.shadowData = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.shadowData;
                                Graphic_Multi nakedGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(Props.newImagePath, 
                                    ShaderDatabase.Cutout, vector, Color.white, Color.white, data, maskPath: Props.maskPath);
                                if (Props.changeDesiccatedGraphic)
                                {
                                    this.pawn_renderer.graphics.dessicatedGraphic = dessicatedGraphic;
                                }
                                this.pawn_renderer.graphics.ResolveAllGraphics();
                                this.pawn_renderer.graphics.nakedGraphic = nakedGraphic;
                            }
                            catch (NullReferenceException) { }
                        }

                    });

                }
                else {
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        if (this.pawn_renderer != null)
                        {

                            try
                            {
                                Graphic nakedGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(pawn.ageTracker.CurKindLifeStage.bodyGraphicData.texPath, ShaderDatabase.Cutout, vector, Color.white);
                                if (Props.changeDesiccatedGraphic)
                                {
                                    this.pawn_renderer.graphics.dessicatedGraphic = pawn.ageTracker.CurKindLifeStage.dessicatedBodyGraphicData.Graphic;

                                }                               
                                this.pawn_renderer.graphics.ResolveAllGraphics();
                                this.pawn_renderer.graphics.nakedGraphic = nakedGraphic;
                                (this.pawn_renderer.graphics.nakedGraphic.data = new GraphicData()).shadowData = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.shadowData;

                            }
                            catch (NullReferenceException) { }
                        }

                    });
                }
                




            }




        }


    }
}
