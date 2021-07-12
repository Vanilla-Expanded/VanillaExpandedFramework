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
                if (CGO.useCustomWindowContent)
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

                    if (CGO.currentGenStep == null || CGO.currentGenStep == "")
                        CGO.currentGenStep = "Generating";
                    if (CGO.currentGenStepMoreInfo == null || CGO.currentGenStepMoreInfo == "")
                        CGO.currentGenStepMoreInfo = "...";

                    Text.Anchor = TextAnchor.LowerCenter;
                    Widgets.Label(loadingRect.TopHalf().TopHalf(), CGO.currentGenStep + GenText.MarchingEllipsis(0f));
                    Widgets.Label(loadingRect.TopHalf().BottomHalf(), string.Format("<i>{0}</i>", CGO.currentGenStepMoreInfo));
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(loadingRect.BottomHalf(), (DateTime.Now - CGO.dateTime).Duration().TotalSeconds.ToString("00.00") + "s");
                    Text.Anchor = TextAnchor.UpperLeft;

                    // grid
                    Rect gridRect = new Rect(screenCenter.x - WidthOffest, gridRectYStart + innerBorderSize, windowWidth, (UI.screenHeight / 2) - innerBorderSize * 2);
                    Widgets.DrawShadowAround(gridRect);
                    Widgets.DrawWindowBackground(gridRect);

                    if (CGO.useStructureLayout)
                    {
                        if (structure == null && CGO.structureLayoutDef != null)
                        {
                            int[][] tempStructure = new int[CGO.structureLayoutDef.layouts[0][0].Split(',').Length][];
                            int x = CGO.structureLayoutDef.layouts[0].Count;
                            for (int i = 0; i < tempStructure.Length; i++)
                            {
                                tempStructure[i] = new int[x];
                            }

                            for (int layoutN = 0; layoutN < CGO.structureLayoutDef.layouts.Count; layoutN++)
                            {
                                for (int layoutLine = CGO.structureLayoutDef.layouts[layoutN].Count - 1; layoutLine >= 0; layoutLine--)
                                {
                                    string[] splitLine = CGO.structureLayoutDef.layouts[layoutN][layoutLine].Split(',');
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
                    else if (CGO.grid != null)
                    {
                        float smallRectWidth = gridRect.width / CGO.grid[0].Length;
                        float smallRectHeight = gridRect.height / CGO.grid.Length;

                        float rSize = Math.Min(smallRectWidth, smallRectHeight);
                        float x = (gridRect.width - (CGO.grid.Length * rSize)) / 2;
                        float y = (gridRect.height - (CGO.grid[0].Length * rSize)) / 2;

                        for (int i = 0; i < CGO.grid.Length; i++)
                        {
                            for (int j = 0; j < CGO.grid[0].Length; j++)
                            {
                                Rect rect = new Rect(gridRect.x + x + (i * rSize), gridRect.y + y + (j * rSize), rSize, rSize);
                                CellType type = CGO.grid[i][j].Type;
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

                    if (CGO.tipAvailable && (currentTip == null || DateTime.Now.Ticks - lastTipShownTick >= 60000000) && CGO.allTip.Count > 1)
                    {
                        currentTip = CGO.allTip.RandomElement();
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