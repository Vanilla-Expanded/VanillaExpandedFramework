using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class PawnRenderNode_GraphicByStyle : PawnRenderNode_AnimalPart
    {
        public PawnRenderNode_GraphicByStyle(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn.TryGetComp<CompGraphicByStyle>(out var comp))
            {
                Graphic graphic = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
                StyleCategoryDef style = null;
                List<ThingStyleCategoryWithPriority> listOfPlayerStyleCategoriesWithPriority = Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.thingStyleCategories;
                List<StyleCategoryDef> listOfPlayerStyles = new List<StyleCategoryDef>();
                List<StyleCategoryDef> listOfCompStyles = new List<StyleCategoryDef>();
                if(listOfPlayerStyleCategoriesWithPriority.Count > 0)
                {
                    foreach (ThingStyleCategoryWithPriority listStyle in listOfPlayerStyleCategoriesWithPriority)
                    {
                        listOfPlayerStyles.Add(listStyle.category);
                    }
                    foreach (StyleGraphics styleGraphics in comp.Props.styleGraphics)
                    {
                        listOfCompStyles.Add(styleGraphics.style);
                    }

                    foreach (StyleCategoryDef playerStyle in listOfPlayerStyles)
                    {
                        foreach (StyleCategoryDef compStyle in listOfCompStyles)
                        {
                            if (playerStyle == compStyle)
                            {
                                style = compStyle;
                                break;
                            }
                        }
                    }

                    if (style != null)
                    {

                        StyleGraphics styleGraphic = comp.Props.styleGraphics.Where(x => x.style == style).FirstOrDefault();
                        return GraphicDatabase.Get<Graphic_Multi>(graphic.path + styleGraphic.styleImageSuffix, ShaderDatabase.Cutout, graphic.drawSize, Color.white);
                    }
                }   

            }
            return base.GraphicFor(pawn);
        }
    }
}
