using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace VanillaFurnitureExpanded
{

    //A simple comp class that changes a building's graphic by using reflection

    public class CompRandomBuildingGraphic : ThingComp
    {
        public Thing thingToGrab;
        public Vector2 sizeVector;
        public Graphic_Multi newGraphic;
        public Graphic_Single newGraphicSingle;
        public Color objectColour;
        public string newGraphicPath = "";
        public string newGraphicSinglePath = "";

        public CompProperties_RandomBuildingGraphic Props
        {
            get
            {
                return (CompProperties_RandomBuildingGraphic)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            thingToGrab = (Thing)this.parent;            
            //Using LongEventHandler to avoid having to create a GraphicCache
            LongEventHandler.ExecuteWhenFinished(ChangeGraphic);

        }

        public void ChangeGraphic()
        {
            sizeVector = this.parent.Graphic.drawSize;
            objectColour = this.parent.Graphic.color;
            if (this.parent.Faction != null && this.parent.Faction.IsPlayer)
            {
                if (this.parent.def.graphicData.graphicClass == typeof(Graphic_Multi))
                {
                    if (newGraphicPath == "")
                    {
                        newGraphicPath = Props.randomGraphics.RandomElement();
                        newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, ShaderDatabase.Cutout, sizeVector, objectColour);
                        Type typ = typeof(Thing);
                        FieldInfo type = typ.GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic);
                        type.SetValue(thingToGrab, newGraphic);
                    }
                    else
                    {
                        newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, ShaderDatabase.Cutout, sizeVector, objectColour);
                        Type typ = typeof(Thing);
                        FieldInfo type = typ.GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic);
                        type.SetValue(thingToGrab, newGraphic);
                    }

                }
                else if (this.parent.def.graphicData.graphicClass == typeof(Graphic_Single))
                {
                    if (newGraphicSinglePath == "")
                    {
                        newGraphicSinglePath = Props.randomGraphics.RandomElement();
                        newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, ShaderDatabase.Cutout, sizeVector, objectColour);
                        Type typ = typeof(Thing);
                        FieldInfo type = typ.GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic);
                        type.SetValue(thingToGrab, newGraphicSingle);
                    }
                    else
                    {
                        newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, ShaderDatabase.Cutout, sizeVector, objectColour);
                        Type typ = typeof(Thing);
                        FieldInfo type = typ.GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic);
                        type.SetValue(thingToGrab, newGraphicSingle);
                    }
                }

            }





        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<string>(ref this.newGraphicPath, "newGraphicPath");
            Scribe_Values.Look<string>(ref this.newGraphicSinglePath, "newGraphicSinglePath");
        }
    }
}
