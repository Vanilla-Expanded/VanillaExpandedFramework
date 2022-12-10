using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;


namespace VanillaFurnitureExpanded
{

    //A simple comp class that changes a building's graphic by using reflection

    public class CompRandomBuildingGraphic : ThingComp
    {
        public Thing thingToGrab;
        public Graphic_Multi newGraphic;
        public Graphic_Single newGraphicSingle;
        public string newGraphicPath = "";
        public string newGraphicSinglePath = "";
        public bool reloading = false;

        public CompProperties_RandomBuildingGraphic Props
        {
            get
            {
                return (CompProperties_RandomBuildingGraphic)props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            thingToGrab = parent;
            reloading = true;
            //Using LongEventHandler to avoid having to create a GraphicCache
            LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(true,0); });


        }

        public void ChangeGraphic(bool random, int index)
        {
            try
            {
                Vector2 sizeVector = parent.Graphic.drawSize;
                Color objectColour = parent.Graphic.color;
                ShaderTypeDef shaderUsed = parent.def.graphicData.shaderType;

                if (parent.Faction != null && parent.Faction.IsPlayer)
                {
                    if (parent.def.graphicData.graphicClass == typeof(Graphic_Multi))
                    {
                        if (!random)
                        {
                            newGraphicPath = Props.randomGraphics[index];
                            newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);



                        }
                        else if (newGraphicPath == "")
                        {
                            if (!VFECore.VFEGlobal.settings.randomStartsAsRandom)
                            {
                                newGraphicPath = Props.randomGraphics.RandomElement();
                            }
                            else
                            {
                                newGraphicPath = Props.randomGraphics[0];
                            }
                          
                            newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);
                        }
                        else
                        {
                            newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);
                        }
                        ReflectionCache.buildingGraphic(thingToGrab) = newGraphic;


                    }
                    else if (parent.def.graphicData.graphicClass == typeof(Graphic_Single))
                    {

                        if (!random)
                        {
                            newGraphicSinglePath = Props.randomGraphics[index];
                            newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, shaderUsed.Shader, sizeVector, objectColour);



                        }
                       
                        else
                        if (newGraphicSinglePath == "")
                        {
                            if (!VFECore.VFEGlobal.settings.randomStartsAsRandom)
                            {
                                newGraphicSinglePath = Props.randomGraphics.RandomElement();
                            }
                            else
                            {
                                newGraphicSinglePath = Props.randomGraphics[0];
                            }
                           
                            newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, shaderUsed.Shader, sizeVector, objectColour);
                        }
                        else
                        {
                            newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, shaderUsed.Shader, sizeVector, objectColour);
                        }
                        if (!parent.def.graphicData.drawRotated)
                        {
                            newGraphicSingle.data = new GraphicData();
                            newGraphicSingle.data.drawRotated = false;
                        }
                        ReflectionCache.buildingGraphic(thingToGrab) = newGraphicSingle;
                        
                    }

                }
            }
            catch (Exception) { Log.Message("The variations mod has probably been added to a running save. Ignoring load error."); }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<string>(ref newGraphicPath, "newGraphicPath");
            Scribe_Values.Look<string>(ref newGraphicSinglePath, "newGraphicSinglePath");
            Scribe_Values.Look<bool>(ref reloading, "reloading", false);

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction != null && parent.Faction.IsPlayer && parent.StyleDef == null && !VFECore.VFEGlobal.settings.hideRandomizeButtons && !Props.disableAllButtons)
            {
                if (!Props.disableRandomButton) {

                    yield return new Command_Action
                    {
                        defaultLabel = "VFE_ChangeGraphic".Translate(),
                        defaultDesc = "VFE_ChangeGraphicDesc".Translate(),

                        icon = ContentFinder<Texture2D>.Get("UI/VEF_ChangeGraphic", true),
                        action = delegate ()
                        {

                            newGraphicPath = "";
                            newGraphicSinglePath = "";

                            LongEventHandler.ExecuteWhenFinished(delegate { ChangeGraphic(true, 0); });
                            parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
                        }
                    };

                }

                if (!Props.disableGraphicChoosingButton) {
                    yield return new Command_Action
                    {
                        defaultLabel = "VFE_ChooseGraphic".Translate(),
                        defaultDesc = "VFE_ChooseGraphicDesc".Translate(),

                        icon = ContentFinder<Texture2D>.Get("UI/VEF_ChooseGraphic", true),
                        action = delegate ()
                        {
                            Dialog_ChooseGraphic window = new Dialog_ChooseGraphic(this.parent, Props.randomGraphics);
                            Find.WindowStack.Add(window);
                        }
                    };
                }
                

            }



            yield break;
        }
    }
}
