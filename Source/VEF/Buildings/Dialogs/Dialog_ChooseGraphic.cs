using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings
{
    public class Dialog_ChooseGraphic : Window
    {

        private static readonly Color borderColor = new Color(0.13f, 0.13f, 0.13f);
        private static readonly Color fillColor = new Color(0, 0, 0,0.1f);


        public Thing thingToChange;
        private Vector2 scrollPosition = new Vector2(0, 0);
        public int columnCount = 4;
        List<string> buildingGraphics;
        CompProperties_RandomBuildingGraphic Props;



        public Dialog_ChooseGraphic(Thing thing, CompProperties_RandomBuildingGraphic Props)
        {
            this.thingToChange = thing;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            this.buildingGraphics = Props.randomGraphics;
            this.Props = Props;




        }

        public override Vector2 InitialSize => new Vector2(620f, 500f);



        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            var outRect = new Rect(inRect);
            outRect.yMin += 30f;
            outRect.yMax -= 40f;


            if (buildingGraphics.Count > 0)
            {

                Widgets.Label(new Rect(0, 10, 300f, 30f), "VFE_ChooseGraphic".Translate());

                var viewRect = new Rect(0f, 30f, outRect.width - 16f, (buildingGraphics.Count / 4) * 128f + 256f);

                Color color = thingToChange.Graphic.Color;
                if (thingToChange.Stuff != null)
                {
                    color = thingToChange.def.GetColorForStuff(thingToChange.Stuff);
                }

                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);



                for (var i = 0; i < buildingGraphics.Count; i++)
                {

                    string availableTexture = buildingGraphics[i];
                    string description = !Props.optionalNames.NullOrEmpty() && Props.optionalNames.Count >= i ? Props.optionalNames[i] : buildingGraphics[i];

                    if (thingToChange.def.graphicData.graphicClass == typeof(Graphic_Multi))
                    {
                        if (Props.useSouthOrientation)
                        {
                            availableTexture = availableTexture + "_south";
                        }
                        else
                        availableTexture = availableTexture + "_north";
                    }
                    Rect rectIcon = new Rect((128 * (i % columnCount)) + 10* (i % columnCount), viewRect.y+(128 * (i / columnCount)+20 * ((i / columnCount)+1)), 128f, 128f);
                    
                    Widgets.DrawBoxSolidWithOutline(rectIcon,fillColor, borderColor, 2);
                    Rect rectIconInside = rectIcon.ContractedBy(2);


                    GUI.DrawTexture(rectIconInside, ContentFinder<Texture2D>.Get(availableTexture, true), ScaleMode.ScaleToFit, alphaBlend: true, 0f, color, 0f, 0f);
                    if (Widgets.ButtonInvisible(rectIcon))
                    {
                        foreach (object obj in Find.Selector.SelectedObjects)
                        {
                            Thing thing = obj as Thing;
                            if (thing != null && thing.def == thingToChange.def)
                            {
                                LongEventHandler.ExecuteWhenFinished(delegate { thing.TryGetComp<CompRandomBuildingGraphic>().ChangeGraphic(false, i); });
                            }


                        }


                        
                        thingToChange.DirtyMapMesh(thingToChange.Map);
                        Close();
                    }

                   

                    TooltipHandler.TipRegion(rectIcon, description);



                }



                Widgets.EndScrollView();

            }
            else
            {
                Widgets.Label(new Rect(0, 10, 300f, 30f), "VFE_NoGraphics".Translate());
            }


        }
    }
}