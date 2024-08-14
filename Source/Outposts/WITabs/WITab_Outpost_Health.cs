using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Outposts
{
    [StaticConstructorOnStartup]
    public class WITab_Outpost_Health : WITab
    {
        private static readonly List<PawnCapacityDef> capacitiesToDisplay = new();

        private bool compactMode;

        private Vector2 scrollPosition;

        private float scrollViewHeight;

        private Pawn specificHealthTabForPawn;

        public WITab_Outpost_Health() => labelKey = "TabCaravanHealth";

        public Outpost SelOutpost => SelObject as Outpost;

        private List<Pawn> Pawns => SelOutpost.AllPawns.Where(p => p.apparel is not null && p.equipment is not null && p.health is not null && p.guest is not null).ToList();

        private float SpecificHealthTabWidth
        {
            get
            {
                EnsureSpecificHealthTabForPawnValid();
                if (specificHealthTabForPawn.DestroyedOrNull()) return 0f;

                return 630f;
            }
        }

        private static List<PawnCapacityDef> CapacitiesToDisplay
        {
            get
            {
                capacitiesToDisplay.Clear();
                var allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
                for (var i = 0; i < allDefsListForReading.Count; i++)
                    if (allDefsListForReading[i].showOnCaravanHealthTab)
                        capacitiesToDisplay.Add(allDefsListForReading[i]);

                capacitiesToDisplay.SortBy(x => x.listOrder);
                return capacitiesToDisplay;
            }
        }

        public override void FillTab()
        {
            EnsureSpecificHealthTabForPawnValid();
            Text.Font = GameFont.Small;
            var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            var rect2 = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
            var num = 0f;
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
            DoColumnHeaders(ref num);
            DoRows(ref num, rect2, rect);
            if (Event.current.type == EventType.Layout) scrollViewHeight = num + 30f;

            Widgets.EndScrollView();
        }

        public override void UpdateSize()
        {
            EnsureSpecificHealthTabForPawnValid();
            base.UpdateSize();
            size = GetRawSize(false);
            if (size.x + SpecificHealthTabWidth > UI.screenWidth)
            {
                compactMode = true;
                size = GetRawSize(true);
                return;
            }

            compactMode = false;
        }

        public override void ExtraOnGUI()
        {
            EnsureSpecificHealthTabForPawnValid();
            base.ExtraOnGUI();
            var localSpecificHealthTabForPawn = specificHealthTabForPawn;
            if (localSpecificHealthTabForPawn != null)
            {
                var tabRect = TabRect;
                var specificHealthTabWidth = SpecificHealthTabWidth;
                var rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificHealthTabWidth, tabRect.height);
                Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
                {
                    if (localSpecificHealthTabForPawn.DestroyedOrNull()) return;

                    HealthCardUtility.DrawPawnHealthCard(new Rect(0f, 20f, rect.width, rect.height - 20f), localSpecificHealthTabForPawn, false, true,
                        localSpecificHealthTabForPawn);
                    if (Widgets.CloseButtonFor(rect.AtZero()))
                    {
                        specificHealthTabForPawn = null;
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                    }
                });
            }
        }

        private void DoColumnHeaders(ref float curY)
        {
            if (!compactMode)
            {
                var num = 135f;
                Text.Anchor = TextAnchor.UpperCenter;
                GUI.color = Widgets.SeparatorLabelColor;
                Widgets.Label(new Rect(num, 3f, 100f, 100f), "Pain".Translate());
                num += 100f;
                var list = CapacitiesToDisplay;
                for (var i = 0; i < list.Count; i++)
                {
                    Widgets.Label(new Rect(num, 3f, 100f, 100f), list[i].LabelCap.Truncate(100f));
                    num += 100f;
                }

                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private void DoRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
        {
            var pawns = Pawns;
            if (specificHealthTabForPawn != null && !pawns.Contains(specificHealthTabForPawn)) specificHealthTabForPawn = null;

            var flag = false;
            for (var i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn.IsColonist)
                {
                    if (!flag)
                    {
                        Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
                        flag = true;
                    }

                    DoRow(ref curY, scrollViewRect, scrollOutRect, pawn);
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
                        Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanPrisonersAndAnimals".Translate());
                        flag2 = true;
                    }

                    DoRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
                }
            }
        }

        private Vector2 GetRawSize(bool compactMode)
        {
            var num = 100f;
            if (!compactMode)
            {
                num += 100f;
                num += CapacitiesToDisplay.Count * 100f;
                num += 40f;
            }

            Vector2 result;
            result.x = 127f + num + 16f;
            result.y = Mathf.Min(550f, PaneTopY - 30f);
            return result;
        }

        private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
        {
            var num = scrollPosition.y - 40f;
            var num2 = scrollPosition.y + scrollOutRect.height;
            if (curY > num && curY < num2) DoRow(new Rect(0f, curY, viewRect.width, 40f), p);

            curY += 40f;
        }

        private void DoRow(Rect rect, Pawn p)
        {
            GUI.BeginGroup(rect);
            var rect2 = rect.AtZero();
            Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
            rect2.width -= 24f;
            CaravanThingsTabUtility.DoOpenSpecificTabButton(rect2, p, ref specificHealthTabForPawn);
            rect2.width -= 24f;
            CaravanThingsTabUtility.DoOpenSpecificTabButtonInvisible(rect2, p, ref specificHealthTabForPawn);
            if (Mouse.IsOver(rect2)) Widgets.DrawHighlight(rect2);
            var rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
            Widgets.ThingIcon(rect3, p);
            var bgRect = new Rect(rect3.xMax + 4f, 11f, 100f, 18f);
            GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, false, false);
            var num = bgRect.xMax;
            if (!compactMode)
            {
                if (p.RaceProps.IsFlesh)
                {
                    var rect4 = new Rect(num, 0f, 100f, 40f);
                    DoPain(rect4, p);
                }

                num += 100f;
                var list = CapacitiesToDisplay;
                for (var i = 0; i < list.Count; i++)
                {
                    var rect5 = new Rect(num, 0f, 100f, 40f);
                    if (p.RaceProps.Humanlike && !list[i].showOnHumanlikes || p.RaceProps.Animal && !list[i].showOnAnimals ||
                        p.RaceProps.IsMechanoid && !list[i].showOnMechanoids || !PawnCapacityUtility.BodyCanEverDoCapacity(p.RaceProps.body, list[i]))
                        num += 100f;
                    else
                    {
                        DoCapacity(rect5, p, list[i]);
                        num += 100f;
                    }
                }
            }

            if (p.Downed)
            {
                GUI.color = new Color(1f, 0f, 0f, 0.5f);
                Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
                GUI.color = Color.white;
            }

            GUI.EndGroup();
        }

        private static void DoPain(Rect rect, Pawn pawn)
        {
            var painLabel = HealthCardUtility.GetPainLabel(pawn);
            if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);

            GUI.color = painLabel.Second;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, painLabel.First);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rect))
            {
                var painTip = HealthCardUtility.GetPainTip(pawn);
                TooltipHandler.TipRegion(rect, painTip);
            }
        }

        private static void DoCapacity(Rect rect, Pawn pawn, PawnCapacityDef capacity)
        {
            var efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, capacity);
            if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);

            GUI.color = efficiencyLabel.Second;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, efficiencyLabel.First);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rect))
            {
                var pawnCapacityTip = HealthCardUtility.GetPawnCapacityTip(pawn, capacity);
                TooltipHandler.TipRegion(rect, pawnCapacityTip);
            }
        }

        public override void Notify_ClearingAllMapsMemory()
        {
            base.Notify_ClearingAllMapsMemory();
            specificHealthTabForPawn = null;
        }

        private void EnsureSpecificHealthTabForPawnValid()
        {
            if (specificHealthTabForPawn != null && (specificHealthTabForPawn.Destroyed || !SelOutpost.Has(specificHealthTabForPawn))) specificHealthTabForPawn = null;
        }
    }
}