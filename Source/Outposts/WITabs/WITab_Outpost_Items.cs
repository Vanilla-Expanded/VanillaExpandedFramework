using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    public class WITab_Outpost_Items : WITab
    {
        private const float SortersSpace = 25f;
        private List<TransferableImmutable> cachedItems = new();
        private int cachedItemsCount;

        private int cachedItemsHash;
        private Vector2 scrollPosition;

        private float scrollViewHeight;
        private TransferableSorterDef sorter1;

        private TransferableSorterDef sorter2;

        public WITab_Outpost_Items() => labelKey = "TabCaravanItems";

        public Outpost SelOutpost => SelObject as Outpost;

        public override void UpdateSize()
        {
            base.UpdateSize();
            CheckCacheItems();
            size = CaravanItemsTabUtility.GetSize(cachedItems, PaneTopY) - new Vector2(0f, 25f);
        }

        public override void FillTab()
        {
            CheckCreateSorters();
            var rect = new Rect(0f, 0f, size.x, size.y);
            GUI.BeginGroup(rect.ContractedBy(10f));
            TransferableUIUtility.DoTransferableSorters(sorter1, sorter2, delegate(TransferableSorterDef x)
            {
                sorter1 = x;
                CacheItems();
            }, delegate(TransferableSorterDef x)
            {
                sorter2 = x;
                CacheItems();
            });
            GUI.EndGroup();
            rect.yMin += SortersSpace;
            GUI.BeginGroup(rect);
            CheckCacheItems();
            DoRows(rect.size);
            GUI.EndGroup();
        }

        private void CheckCacheItems()
        {
            var list = SelOutpost.Things.ToList();
            if (list.Count != cachedItemsCount)
            {
                CacheItems();
                return;
            }

            var num = 0;
            for (var i = 0; i < list.Count; i++) num = Gen.HashCombineInt(num, list[i].GetHashCode());

            if (num != cachedItemsHash) CacheItems();
        }

        private void CacheItems()
        {
            CheckCreateSorters();
            cachedItems.Clear();
            var list = SelOutpost.Things.ToList();
            var seed = 0;
            for (var i = 0; i < list.Count; i++)
            {
                var transferableImmutable = TransferableUtility.TransferableMatching(list[i], cachedItems, TransferAsOneMode.Normal);
                if (transferableImmutable == null)
                {
                    transferableImmutable = new TransferableImmutable();
                    cachedItems.Add(transferableImmutable);
                }

                transferableImmutable.things.Add(list[i]);
                seed = Gen.HashCombineInt(seed, list[i].GetHashCode());
            }

            cachedItems = cachedItems.OrderBy(tr => tr, sorter1.Comparer).ThenBy(tr => tr, sorter2.Comparer)
                .ThenBy(TransferableUIUtility.DefaultListOrderPriority).ToList();
            cachedItemsCount = list.Count;
            cachedItemsHash = seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckCreateSorters()
        {
            sorter1 ??= TransferableSorterDefOf.Category;
            sorter2 ??= TransferableSorterDefOf.MarketValue;
        }

        private void DoRows(Vector2 size)
        {
            Text.Font = GameFont.Small;
            var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            var viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            var num = 0f;
            Widgets.ListSeparator(ref num, viewRect.width, "CaravanItems".Translate());
            if (cachedItems.Any())
                for (var i = 0; i < cachedItems.Count; i++)
                    DoRow(ref num, viewRect, rect, cachedItems[i]);
            else
                Widgets.NoneLabel(ref num, viewRect.width);

            if (Event.current.type == EventType.Layout) scrollViewHeight = num + 30f;

            Widgets.EndScrollView();
        }

        private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, TransferableImmutable thing)
        {
            var num = scrollPosition.y - 30f;
            var num2 = scrollPosition.y + scrollOutRect.height;
            if (curY > num && curY < num2) DoRow(new Rect(0f, curY, viewRect.width, 30f), thing);

            curY += 30f;
        }

        private void DoRow(Rect rect, TransferableImmutable thing)
        {
            GUI.BeginGroup(rect);
            var rect2 = rect.AtZero();
            Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, thing.AnyThing);
            rect2.width -= 24f;
            var rect3 = rect2;
            rect3.xMin = rect3.xMax - 60f;
            CaravanThingsTabUtility.DrawMass(thing, rect3);
            rect2.width -= 60f;
            Widgets.DrawHighlightIfMouseover(rect2);
            var rect4 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
            Widgets.ThingIcon(rect4, thing.AnyThing);
            var rect5 = new Rect(rect4.xMax + 4f, 0f, 300f, 30f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Widgets.Label(rect5, thing.LabelCapWithTotalStackCount.Truncate(rect5.width));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.EndGroup();
        }
    }
}