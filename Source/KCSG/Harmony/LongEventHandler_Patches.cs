using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class LongEventHandler_Patches
    {
        [StaticConstructorOnStartup]
        [HarmonyPatch(typeof(LongEventHandler))]
        [HarmonyPatch("LongEventsOnGUI", MethodType.Normal)]
        public class LongEventsOnGUI_Prefix
        {
            // Rect
            public static float windowWidth = Math.Min(600f, UI.screenWidth);

            public static float WidthOffest => windowWidth / 2;
            public static float innerBorderSize = 35f;
            public static float outerBorderSize = 100f;

            public static float loadingRectYStart = 0;
            public static float gridRectYStart = (UI.screenHeight / 4);
            public static float tipsRectYStart = 3 * (UI.screenHeight / 4);

            // Tip util
            public static string currentTip;

            public static long lastTipShownTick;

            // Grid
            public static int[][] structure;

            [HarmonyPrefix]
            public static bool Prefix()
            {
                if (GenOption.useCustomWindowContent)
                {
                    UIMenuBackgroundManager.background = new UI_BackgroundMain();
                    UIMenuBackgroundManager.background.BackgroundOnGUI();
                    // Fill the whole screen
                    Rect fullRect = new Rect(0, 0, UI.screenWidth, UI.screenHeight);
                    // Content
                    Vector2 screenCenter = fullRect.center;

                    // Loading infos
                    Rect loadingRect = new Rect(screenCenter.x - WidthOffest, loadingRectYStart + outerBorderSize, windowWidth, (UI.screenHeight / 4) - outerBorderSize);
                    Widgets.DrawShadowAround(loadingRect);
                    Widgets.DrawWindowBackground(loadingRect);

                    if (GenOption.currentGenStep == null || GenOption.currentGenStep == "")
                        GenOption.currentGenStep = "Generating";
                    if (GenOption.currentGenStepMoreInfo == null || GenOption.currentGenStepMoreInfo == "")
                        GenOption.currentGenStepMoreInfo = "...";

                    Text.Anchor = TextAnchor.LowerCenter;
                    Widgets.Label(loadingRect.TopHalf().TopHalf(), GenOption.currentGenStep + GenText.MarchingEllipsis(0f));
                    Widgets.Label(loadingRect.TopHalf().BottomHalf(), string.Format("<i>{0}</i>", GenOption.currentGenStepMoreInfo));
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(loadingRect.BottomHalf(), (DateTime.Now - GenOption.dateTime).Duration().TotalSeconds.ToString("00.00") + "s");
                    Text.Anchor = TextAnchor.UpperLeft;

                    // grid
                    Rect gridRect = new Rect(screenCenter.x - WidthOffest, gridRectYStart + innerBorderSize, windowWidth, (UI.screenHeight / 2) - innerBorderSize * 2);
                    Widgets.DrawShadowAround(gridRect);
                    Widgets.DrawWindowBackground(gridRect);

                    if (GenOption.useStructureLayout)
                    {
                        if (structure == null && GenOption.structureLayoutDef != null)
                        {
                            int[][] tempStructure = new int[GenOption.structureLayoutDef.layouts[0][0].Split(',').Length][];
                            int x = GenOption.structureLayoutDef.layouts[0].Count;
                            for (int i = 0; i < tempStructure.Length; i++)
                            {
                                tempStructure[i] = new int[x];
                            }

                            for (int layoutN = 0; layoutN < GenOption.structureLayoutDef.layouts.Count; layoutN++)
                            {
                                for (int layoutLine = GenOption.structureLayoutDef.layouts[layoutN].Count - 1; layoutLine >= 0; layoutLine--)
                                {
                                    string[] splitLine = GenOption.structureLayoutDef.layouts[layoutN][layoutLine].Split(',');
                                    for (int splitN = 0; splitN < splitLine.Length; splitN++)
                                    {
                                        if (splitLine[splitN] != ".")
                                        {
                                            tempStructure[splitN][layoutLine] = 1;
                                        }
                                        else
                                        {
                                            tempStructure[splitN][layoutLine] = 0;
                                        }
                                    }
                                }
                            }
                            structure = tempStructure;
                        }
                        else
                        {
                            float smallRectWidth = gridRect.width / structure.Length;
                            float smallRectHeight = gridRect.height / structure[0].Length;

                            float rSize = Math.Min(smallRectWidth, smallRectHeight);
                            float x = (gridRect.width - (structure.Length * rSize)) / 2;
                            float y = (gridRect.height - (structure[0].Length * rSize)) / 2;

                            for (int i = 0; i < structure.Length; i++)
                            {
                                for (int j = 0; j < structure[0].Length; j++)
                                {
                                    if (structure[i][j] == 1)
                                    {
                                        Rect rect = new Rect(gridRect.x + x + (i * rSize), gridRect.y + y + (j * rSize), rSize, rSize);
                                        GUI.DrawTexture(rect, BaseContent.WhiteTex);
                                    }
                                }
                            }
                        }
                    }
                    else if (GenOption.grid != null)
                    {
                        float smallRectWidth = gridRect.width / GenOption.grid[0].Length;
                        float smallRectHeight = gridRect.height / GenOption.grid.Length;

                        float rSize = Math.Min(smallRectWidth, smallRectHeight);
                        float x = (gridRect.width - (GenOption.grid.Length * rSize)) / 2;
                        float y = (gridRect.height - (GenOption.grid[0].Length * rSize)) / 2;

                        for (int i = 0; i < GenOption.grid.Length; i++)
                        {
                            for (int j = 0; j < GenOption.grid[0].Length; j++)
                            {
                                Rect rect = new Rect(gridRect.x + x + (i * rSize), gridRect.y + y + (j * rSize), rSize, rSize);
                                CellType type = GenOption.grid[i][j].Type;
                                GUI.color = type == CellType.BUILDING || type == CellType.BUILDINGPASSABLE ? Color.black :
                                    (type == CellType.DOOR ? Color.black :
                                    (type == CellType.MAINROAD || type == CellType.ROAD ? Color.grey :
                                    (type == CellType.WATER ? Color.blue :
                                    (type == CellType.FIELD ? Color.green :
                                    Color.clear))));
                                GUI.DrawTexture(rect, BaseContent.WhiteTex);
                                GUI.color = Color.white;
                            }
                        }
                    }

                    // tip
                    Rect tipRect = new Rect(screenCenter.x - WidthOffest, tipsRectYStart, windowWidth, (UI.screenHeight / 4) - outerBorderSize);
                    Widgets.DrawShadowAround(tipRect);
                    Widgets.DrawWindowBackground(tipRect);

                    if (GenOption.tipAvailable && (currentTip == null || DateTime.Now.Ticks - lastTipShownTick >= 60000000) && GenOption.allTip.Count > 1)
                    {
                        currentTip = GenOption.allTip.RandomElement();
                        lastTipShownTick = DateTime.Now.Ticks;
                    }
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(tipRect, string.Format("<size=16>{0}</size>", currentTip));
                    Text.Anchor = TextAnchor.UpperLeft;
                    return false;
                }
                return true;
            }
        }
    }
}