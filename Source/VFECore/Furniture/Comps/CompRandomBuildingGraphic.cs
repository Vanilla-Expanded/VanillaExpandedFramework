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
            LongEventHandler.ExecuteWhenFinished(ChangeGraphic);


        }

        public void ChangeGraphic()
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
                        if (!VFECore.VFEGlobal.settings.isRandomGraphic)
                        {
                            if (!reloading)
                            {
                                int newGraphicPathIndex = Props.randomGraphics.IndexOf(newGraphicPath);
                                if (newGraphicPathIndex + 1 > Props.randomGraphics.Count - 1)
                                {
                                    newGraphicPathIndex = 0;
                                }
                                else newGraphicPathIndex++;
                                newGraphicPath = Props.randomGraphics[newGraphicPathIndex];
                                newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);
                            }
                            else
                            {
                                if (newGraphicPath == "")
                                {
                                    newGraphicPath = Props.randomGraphics[0];
                                    newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);
                                }
                                else
                                {
                                    newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);
                                }
                                reloading = false;

                            }



                        }
                        else if (newGraphicPath == "")
                        {
                            newGraphicPath = Props.randomGraphics.RandomElement();
                            newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);
                        }
                        else
                        {
                            newGraphic = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(newGraphicPath, shaderUsed.Shader, sizeVector, objectColour);
                        }
                        Type typ = typeof(Thing);
                        FieldInfo type = typ.GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic);
                        type.SetValue(thingToGrab, newGraphic);

                    }
                    else if (parent.def.graphicData.graphicClass == typeof(Graphic_Single))
                    {
                        if (!VFECore.VFEGlobal.settings.isRandomGraphic)
                        {
                            if (!reloading)
                            {
                                int newGraphicPathIndex = Props.randomGraphics.IndexOf(newGraphicSinglePath);
                                if (newGraphicPathIndex + 1 > Props.randomGraphics.Count - 1)
                                {
                                    newGraphicPathIndex = 0;
                                }
                                else newGraphicPathIndex++;
                                newGraphicSinglePath = Props.randomGraphics[newGraphicPathIndex];
                                newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, shaderUsed.Shader, sizeVector, objectColour);
                            }
                            else
                            {
                                if (newGraphicSinglePath == "")
                                {
                                    newGraphicSinglePath = Props.randomGraphics[0];
                                    newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, shaderUsed.Shader, sizeVector, objectColour);
                                }
                                else
                                {
                                    newGraphicSingle = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(newGraphicSinglePath, shaderUsed.Shader, sizeVector, objectColour);
                                }
                                reloading = false;
                            }
                            



                        }
                        else
                        if (newGraphicSinglePath == "")
                        {
                            newGraphicSinglePath = Props.randomGraphics.RandomElement();
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
                        Type typ = typeof(Thing);
                        FieldInfo type = typ.GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic);
                        type.SetValue(thingToGrab, newGraphicSingle);
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
            if (parent.Faction != null && parent.Faction.IsPlayer && parent.StyleDef == null && !VFECore.VFEGlobal.settings.hideRandomizeButton)
            {
                yield return new Command_Action
                {
                    defaultLabel = "VFE_ChangeGraphic".Translate(),
                    defaultDesc = "VFE_ChangeGraphicDesc".Translate(),

                    icon = ContentFinder<Texture2D>.Get("UI/VEF_ChangeGraphic", true),
                    action = delegate ()
                    {
                        if (VFECore.VFEGlobal.settings.isRandomGraphic)
                        {
                            newGraphicPath = "";
                            newGraphicSinglePath = "";
                        }
                        LongEventHandler.ExecuteWhenFinished(ChangeGraphic);
                        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
                    }
                };

            }



            yield break;
        }
    }
}
