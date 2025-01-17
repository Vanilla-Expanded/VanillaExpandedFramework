using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(MainTabWindow_Quests), "DoRow")]
    public static class MainTabWindow_Quests_DoRow_Patch
    {
        public static void Postfix(Rect rect, Quest quest)
        {
            var extension = quest.root.GetModExtension<QuestChainExtension>();
            if (extension != null)
            {
                bool isCharity = ModsConfig.IdeologyActive && quest.charity && !quest.Historical && !quest.dismissed;
                Rect rect2 = rect;
                rect2.width -= 95f;
                Rect rect3 = rect;
                rect3.xMax -= 4f;
                rect3.xMin = rect3.xMax - 35f;
                Rect rect4 = rect;
                rect4.xMax = rect3.xMin;
                rect4.xMin = rect4.xMax - 60f;
                if (isCharity)
                {
                    rect4.x -= 15f;
                }
                Rect rect7 = new Rect(rect4.x - 15f, rect4.y + rect4.height / 2f - 7f, 15f, 15f);
                GUI.DrawTexture(rect7, extension.questChainDef.icon);
                Rect rect5 = new Rect(rect2.x + 4f, rect2.y, rect2.width - 4f, rect2.height);
                rect7.height = rect5.height;
                rect7.y = rect5.y;
                if (Mouse.IsOver(rect7))
                {
                    TooltipHandler.TipRegion(rect7, "VEF.QuestChainTooltip".Translate(extension.questChainDef.label, extension.questChainDef.Worker.GetDescription()));
                    Widgets.DrawHighlight(rect7);
                }
            }
        }
    }
}