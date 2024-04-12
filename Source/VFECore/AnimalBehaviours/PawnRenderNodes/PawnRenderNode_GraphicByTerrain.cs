using RimWorld;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class PawnRenderNode_GraphicByTerrain : PawnRenderNode_AnimalPart
    {
        public PawnRenderNode_GraphicByTerrain(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn.TryGetComp<CompGraphicByTerrain>(out var comp) && comp.currentName != "")
            {
                Graphic graphic = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
                if (comp.terrainName == "Normal")
                {
                    return base.GraphicFor(pawn);
                }
                if (comp.terrainName == "Water") {                    
                    return GraphicDatabase.Get<Graphic_Multi>(graphic.path + comp.Props.waterSuffix, ShaderDatabase.Cutout, graphic.drawSize, Color.white);
                }
                if (comp.terrainName == "Cold")
                {                 
                    return GraphicDatabase.Get<Graphic_Multi>(graphic.path + comp.Props.lowTemperatureSuffix, ShaderDatabase.Cutout, graphic.drawSize, Color.white);
                }
                if (comp.terrainName == "Snowy")
                {                 
                    return GraphicDatabase.Get<Graphic_Multi>(graphic.path + comp.Props.snowySuffix, ShaderDatabase.Cutout, graphic.drawSize, Color.white);
                }
                return GraphicDatabase.Get<Graphic_Multi>(graphic.path + comp.Props.suffix[comp.indexTerrain], ShaderDatabase.Cutout, graphic.drawSize, Color.white);


            }
            return base.GraphicFor(pawn);
        }
    }
}
