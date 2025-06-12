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
    public class CompGraphicByHediff : ThingComp
    {
        private PawnRenderer pawn_renderer;
        public Graphic dessicatedGraphic;
        public int changeGraphicsCounter = 0;

        public CompProperties_GraphicByHediff Props
        {
            get
            {
                return (CompProperties_GraphicByHediff) this.props;
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

            if (Props.changeDesiccatedGraphic)
            {
                GraphicData dessicatedgraphicdata = new GraphicData();
                dessicatedgraphicdata.texPath = Props.dessicatedTxt;
                dessicatedGraphic = dessicatedgraphicdata.Graphic;
            }

            this.ChangeTheGraphics();
        }

        public void ChangeTheGraphics()
        {
            if (this.parent.Map != null && AnimalBehaviours_Settings.flagGraphicChanging)
            {
                Pawn pawn = this.parent as Pawn;
                if (this.pawn_renderer == null)
                {
                    this.pawn_renderer = pawn.Drawer.renderer;
                }

                bool hediffFound = false;
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef, false);
                if (hediff != null)
                {
                    hediffFound = true;
                }

                //Uses drawsize, color, colorTwo and ShadowData of the original bodyGraphicData
                Vector2 vector = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                Color color = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.color;
                Color color2 = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.color;
                var shader = ShaderDatabase.Cutout;
                if (Props.useCutoutComplex)
                {
                    shader = ShaderDatabase.CutoutComplex;
                }
                var data = new GraphicData();
                data.shadowData = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.shadowData;



                if (hediffFound)
                {
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        if (this.pawn_renderer != null)
                        {
                            try
                            {
                                Graphic_Multi nakedGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(Props.newImagePath, shader, vector, color, color2, data, maskPath: Props.maskPath);
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
                else
                {
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        if (this.pawn_renderer != null)
                        {
                            try
                            {
                                Graphic_Multi nakedGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(pawn.ageTracker.CurKindLifeStage.bodyGraphicData.texPath, shader, vector, color, color2, data, maskPath: pawn.ageTracker.CurKindLifeStage.bodyGraphicData.maskPath);
                                if (Props.changeDesiccatedGraphic)
                                {
                                    this.pawn_renderer.graphics.dessicatedGraphic = pawn.ageTracker.CurKindLifeStage.dessicatedBodyGraphicData.Graphic;
                                }
                                this.pawn_renderer.graphics.ResolveAllGraphics();
                                this.pawn_renderer.graphics.nakedGraphic = nakedGraphic;
                            }
                            catch (NullReferenceException) { }
                        }
                    });
                }
            }
        }
    }
}
