using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Outposts
{
    public class WITab_Outpost_Needs : WITab
    {
        private static readonly List<Need> needsToDisplay = new();

        private static readonly List<Thought> thoughtGroupsPresent = new();

        private static readonly List<Thought> thoughtGroup = new();
        private bool doNeeds;

        private Vector2 scrollPosition;

        private float scrollViewHeight;

        private Pawn specificNeedsTabForPawn;

        private Vector2 thoughtScrollPosition;

        public WITab_Outpost_Needs() => labelKey = "TabCaravanNeeds";
        public Outpost SelOutpost => SelObject as Outpost;

        private float SpecificNeedsTabWidth => specificNeedsTabForPawn.DestroyedOrNull() ? 0f : NeedsCardUtility.GetSize(specificNeedsTabForPawn).x;
        private List<Pawn> Pawns => SelOutpost.AllPawns.ToList();

        public override void Notify_ClearingAllMapsMemory()
        {
            base.Notify_ClearingAllMapsMemory();
            specificNeedsTabForPawn = null;
        }

        public override void UpdateSize()
        {
            EnsureSpecificNeedsTabForPawnValid();
            base.UpdateSize();
            size = CaravanNeedsTabUtility.GetSize(Pawns, PaneTopY);
            if (size.x + SpecificNeedsTabWidth > UI.screenWidth)
            {
                doNeeds = false;
                size = CaravanNeedsTabUtility.GetSize(Pawns, PaneTopY, false);
            }
            else
                doNeeds = true;

            size.y = Mathf.Max(size.y, NeedsCardUtility.FullSize.y);
        }

        public override void ExtraOnGUI()
        {
            EnsureSpecificNeedsTabForPawnValid();
            base.ExtraOnGUI();
            var localSpecificNeedsTabForPawn = specificNeedsTabForPawn;
            if (localSpecificNeedsTabForPawn == null) return;
            var tabRect = TabRect;
            var specificNeedsTabWidth = SpecificNeedsTabWidth;
            var rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificNeedsTabWidth, tabRect.height);
            Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
            {
                if (localSpecificNeedsTabForPawn.DestroyedOrNull()) return;

                NeedsCardUtility.DoNeedsMoodAndThoughts(rect.AtZero(), localSpecificNeedsTabForPawn, ref thoughtScrollPosition);
                if (Widgets.CloseButtonFor(rect.AtZero()))
                {
                    specificNeedsTabForPawn = null;
                    SoundDefOf.TabClose.PlayOneShotOnCamera();
                }
            });
        }

        private void EnsureSpecificNeedsTabForPawnValid()
        {
            if (specificNeedsTabForPawn != null && (specificNeedsTabForPawn.Destroyed || !SelOutpost.Has(specificNeedsTabForPawn))) specificNeedsTabForPawn = null;
        }

        public override void FillTab()
        {
            EnsureSpecificNeedsTabForPawnValid();
            DoRows(size, Pawns);
        }

        private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn pawn)
        {
            var num = scrollPosition.y - 40f;
            var num2 = scrollPosition.y + scrollOutRect.height;
            if (curY > num && curY < num2) DoRow(new Rect(0f, curY, viewRect.width, 40f), pawn);

            curY += 40f;
        }

        private void DoRows(Vector2 size, List<Pawn> pawns)
        {
            if (specificNeedsTabForPawn != null && (!pawns.Contains(specificNeedsTabForPawn) || specificNeedsTabForPawn.Dead)) specificNeedsTabForPawn = null;

            Text.Font = GameFont.Small;
            var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            var viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            var num = 0f;
            var flag = false;
            for (var i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn.IsColonist)
                {
                    if (!flag)
                    {
                        Widgets.ListSeparator(ref num, viewRect.width, "CaravanColonists".Translate());
                        flag = true;
                    }

                    DoRow(ref num, viewRect, rect, pawn);
                }
            }

            var flag2 = false;
            for (var j = 0; j < pawns.Count; j++)
            {
                var pawn2 = pawns[j];
                if (!pawn2.IsColonist)
                {
                    if (!flag2)
                    {
                        Widgets.ListSeparator(ref num, viewRect.width, "CaravanPrisonersAndAnimals".Translate());
                        flag2 = true;
                    }

                    DoRow(ref num, viewRect, rect, pawn2);
                }
            }

            if (Event.current.type == EventType.Layout) scrollViewHeight = num + 30f;

            Widgets.EndScrollView();
        }

        private static void GetNeedsToDisplay(Pawn p)
        {
            needsToDisplay.Clear();
            var allNeeds = p.needs.AllNeeds;
            for (var i = 0; i < allNeeds.Count; i++)
            {
                var need = allNeeds[i];
                if (need.def.showForCaravanMembers) needsToDisplay.Add(need);
            }

            PawnNeedsUIUtility.SortInDisplayOrder(needsToDisplay);
        }

        private void DoRow(Rect rect, Pawn pawn)
        {
            GUI.BeginGroup(rect);
            var rect2 = rect.AtZero();
            Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, pawn);
            rect2.width -= 24f;
            if (!pawn.Dead)
            {
                CaravanThingsTabUtility.DoOpenSpecificTabButton(rect2, pawn, ref specificNeedsTabForPawn);
                rect2.width -= 24f;
                CaravanThingsTabUtility.DoOpenSpecificTabButtonInvisible(rect2, pawn, ref specificNeedsTabForPawn);
            }

            Widgets.DrawHighlightIfMouseover(rect2);
            var rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
            Widgets.ThingIcon(rect3, pawn);
            var bgRect = new Rect(rect3.xMax + 4f, 11f, 100f, 18f);
            GenMapUI.DrawPawnLabel(pawn, bgRect, 1f, 100f, null, GameFont.Small, false, false);
            if (doNeeds)
            {
                GetNeedsToDisplay(pawn);
                var xMax = bgRect.xMax;
                for (var i = 0; i < needsToDisplay.Count; i++)
                {
                    var need = needsToDisplay[i];
                    var maxThresholdMarkers = 0;
                    var doTooltip = true;
                    var rect4 = new Rect(xMax, 0f, 100f, 40f);
                    if (need is Need_Mood mood)
                    {
                        maxThresholdMarkers = 1;
                        doTooltip = false;
                        if (Mouse.IsOver(rect4)) TooltipHandler.TipRegion(rect4, new TipSignal(() => CustomMoodNeedTooltip(mood), rect4.GetHashCode()));
                    }

                    var rect5 = rect4;
                    rect5.yMin -= 5f;
                    rect5.yMax += 5f;
                    need.DrawOnGUI(rect5, maxThresholdMarkers, 10f, false, doTooltip, rect4);
                    xMax = rect4.xMax;
                }
            }

            if (pawn.Downed)
            {
                GUI.color = new Color(1f, 0f, 0f, 0.5f);
                Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
                GUI.color = Color.white;
            }

            GUI.EndGroup();
        }

        private static string CustomMoodNeedTooltip(Need_Mood mood)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(mood.GetTipString());
            PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder(mood, thoughtGroupsPresent);
            var flag = false;
            for (var i = 0; i < thoughtGroupsPresent.Count; i++)
            {
                var group = thoughtGroupsPresent[i];
                mood.thoughts.GetMoodThoughts(group, thoughtGroup);
                var leadingThoughtInGroup = PawnNeedsUIUtility.GetLeadingThoughtInGroup(thoughtGroup);
                if (leadingThoughtInGroup.VisibleInNeedsTab)
                {
                    if (!flag)
                    {
                        flag = true;
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.Append(leadingThoughtInGroup.LabelCap);
                    if (thoughtGroup.Count > 1)
                    {
                        stringBuilder.Append(" x");
                        stringBuilder.Append(thoughtGroup.Count);
                    }

                    stringBuilder.Append(": ");
                    stringBuilder.AppendLine(mood.thoughts.MoodOffsetOfGroup(group).ToString("##0"));
                }
            }

            return stringBuilder.ToString();
        }
    }
}