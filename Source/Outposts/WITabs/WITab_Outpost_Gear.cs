using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Outposts
{
    public class WITab_Outpost_Gear : WITab
    {
        private static readonly List<Apparel> tmpApparel = new();

        private static readonly List<ThingWithComps> tmpExistingEquipment = new();

        private static readonly List<Apparel> tmpExistingApparel = new();

        private List<Thing> allThings;
        private Thing draggedItem;

        private Vector2 draggedItemPosOffset;
        private bool droppedDraggedItem;
        private Vector2 leftPaneScrollPosition;
        private float leftPaneScrollViewHeight;
        private float leftPaneWidth;
        private Vector2 rightPaneScrollPosition;

        private float rightPaneScrollViewHeight;

        private float rightPaneWidth;

        public WITab_Outpost_Gear() => labelKey = "TabCaravanGear";

        public Outpost SelOutpost => SelObject as Outpost;

        private List<Pawn> Pawns => SelOutpost.AllPawns.Where(p => p.apparel is not null && p.equipment is not null && p.health is not null && p.guest is not null).ToList();

        public override void UpdateSize()
        {
            base.UpdateSize();
            leftPaneWidth = 469f;
            rightPaneWidth = 345f;
            size.x = leftPaneWidth + rightPaneWidth;
            size.y = Mathf.Min(550f, PaneTopY - 30f);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            draggedItem = null;
        }

        private void DoLeftPane()
        {
            var rect = new Rect(0f, 0f, leftPaneWidth, size.y).ContractedBy(10f);
            var rect2 = new Rect(0f, 0f, rect.width - 16f, leftPaneScrollViewHeight);
            var num = 0f;
            Widgets.BeginScrollView(rect, ref leftPaneScrollPosition, rect2);
            DoPawnRows(ref num, rect2, rect);
            if (Event.current.type == EventType.Layout) leftPaneScrollViewHeight = num + 30f;

            Widgets.EndScrollView();
        }

        private void DoPawnRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
        {
            var num = leftPaneScrollPosition.y - 40f;
            var num2 = leftPaneScrollPosition.y + scrollOutRect.height;
            if (curY > num && curY < num2) DoPawnRow(new Rect(0f, curY, viewRect.width, 40f), p);

            curY += 40f;
        }

        private void DoPawnRow(Rect rect, Pawn p)
        {
            GUI.BeginGroup(rect);
            var rect2 = rect.AtZero();
            Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
            rect2.width -= 24f;
            var flag = draggedItem != null && rect2.Contains(Event.current.mousePosition) && CurrentWearerOf(draggedItem) != p;
            if (Mouse.IsOver(rect2) && draggedItem == null || flag) Widgets.DrawHighlight(rect2);

            if (flag && droppedDraggedItem)
            {
                TryEquipDraggedItem(p);
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            }

            var rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
            Widgets.ThingIcon(rect3, p);
            var bgRect = new Rect(rect3.xMax + 4f, 11f, 100f, 18f);
            GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, false, false);
            var xMax = bgRect.xMax;
            if (p.equipment != null)
            {
                var allEquipmentListForReading = p.equipment.AllEquipmentListForReading;
                for (var i = 0; i < allEquipmentListForReading.Count; i++) DoEquippedGear(allEquipmentListForReading[i], p, ref xMax);
            }

            if (p.apparel != null)
            {
                tmpApparel.Clear();
                tmpApparel.AddRange(p.apparel.WornApparel);
                tmpApparel.SortBy(x => x.def.apparel.LastLayer.drawOrder, x => -x.def.apparel.HumanBodyCoverage);
                for (var j = 0; j < tmpApparel.Count; j++) DoEquippedGear(tmpApparel[j], p, ref xMax);
            }

            if (p.Downed)
            {
                GUI.color = new Color(1f, 0f, 0f, 0.5f);
                Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
                GUI.color = Color.white;
            }

            GUI.EndGroup();
        }

        private void DoInventoryRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
        {
            Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanWeaponsAndApparel".Translate());
            var flag = false;
            for (var i = 0; i < allThings.Count; i++)
            {
                var thing = allThings[i];
                if (IsVisibleWeapon(thing.def))
                {
                    if (!flag) flag = true;

                    DoInventoryRow(ref curY, scrollViewRect, scrollOutRect, thing);
                }
            }

            var flag2 = false;
            for (var j = 0; j < allThings.Count; j++)
            {
                var thing2 = allThings[j];
                if (thing2.def.IsApparel)
                {
                    if (!flag2) flag2 = true;

                    DoInventoryRow(ref curY, scrollViewRect, scrollOutRect, thing2);
                }
            }

            if (!flag && !flag2) Widgets.NoneLabel(ref curY, scrollViewRect.width);
        }

        private void DoEquippedGear(Thing t, Pawn p, ref float curX)
        {
            var rect = new Rect(curX, 4f, 32f, 32f);
            var flag = Mouse.IsOver(rect);
            float alpha;
            if (t == draggedItem)
                alpha = 0.2f;
            else if (flag && draggedItem == null)
                alpha = 0.75f;
            else
                alpha = 1f;

            Widgets.ThingIcon(rect, t, alpha);
            curX += 32f;
            if (Mouse.IsOver(rect)) TooltipHandler.TipRegion(rect, t.LabelCap);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && flag)
            {
                draggedItem = t;
                droppedDraggedItem = false;
                draggedItemPosOffset = Event.current.mousePosition - rect.position;
                Event.current.Use();
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }

        private void CheckDraggedItemStillValid()
        {
            if (draggedItem == null) return;
            
            if (draggedItem.Destroyed)
            {
                draggedItem = null;
                return;
            }
            
            if (CurrentWearerOf(draggedItem) != null) return;

            if (allThings.Contains(draggedItem)) return;

            draggedItem = null;
        }

        private void CheckDropDraggedItem()
        {
            if (draggedItem == null) return;

            if (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseUp) droppedDraggedItem = true;
        }

        private void TryEquipDraggedItem(Pawn p)
        {
            droppedDraggedItem = false;
            if (!EquipmentUtility.CanEquip(draggedItem, p, out var str))
            {
                Messages.Message("MessageCantEquipCustom".Translate(str.CapitalizeFirst()), p, MessageTypeDefOf.RejectInput, false);
                draggedItem = null;
                return;
            }

            if (draggedItem.def.IsWeapon)
            {
                if (p.guest.IsPrisoner)
                {
                    Messages.Message("MessageCantEquipCustom".Translate("MessagePrisonerCannotEquipWeapon".Translate(p.Named("PAWN"))), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (p.WorkTagIsDisabled(WorkTags.Violent))
                {
                    Messages.Message("MessageCantEquipIncapableOfViolence".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (p.WorkTagIsDisabled(WorkTags.Shooting) && draggedItem.def.IsRangedWeapon)
                {
                    Messages.Message("MessageCantEquipIncapableOfShooting".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    Messages.Message("MessageCantEquipIncapableOfManipulation".Translate(), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }
            }

            if (draggedItem is Apparel apparel && p.apparel != null)
            {
                if (!ApparelUtility.HasPartsToWear(p, apparel.def))
                {
                    Messages.Message("MessageCantWearApparelMissingBodyParts".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (CurrentWearerOf(apparel) != null && CurrentWearerOf(apparel).apparel.IsLocked(apparel))
                {
                    Messages.Message("MessageCantUnequipLockedApparel".Translate(), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (p.apparel.WouldReplaceLockedApparel(apparel))
                {
                    Messages.Message("MessageWouldReplaceLockedApparel".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                tmpExistingApparel.Clear();
                tmpExistingApparel.AddRange(p.apparel.WornApparel);
                for (var i = 0; i < tmpExistingApparel.Count; i++)
                    if (!ApparelUtility.CanWearTogether(apparel.def, tmpExistingApparel[i].def, p.RaceProps.body))
                    {
                        p.apparel.Remove(tmpExistingApparel[i]);
                        SelOutpost.AddItem(tmpExistingApparel[i]);
                    }

                p.apparel.Wear((Apparel) SelOutpost.TakeItem(apparel), false);
                p.outfits?.forcedHandler.SetForced(apparel, true);
            }
            else if (draggedItem is ThingWithComps thingWithComps && p.equipment != null)
            {
                var personaWeaponConfirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText(draggedItem, p);
                if (!personaWeaponConfirmationText.NullOrEmpty())
                {
                    var thing = draggedItem;
                    Find.WindowStack.Add(new Dialog_MessageBox(personaWeaponConfirmationText, "Yes".Translate(), delegate { TryEquipDraggedItem_Equipment(p, thingWithComps); },
                        "No".Translate()));
                    draggedItem = null;
                    return;
                }

                TryEquipDraggedItem_Equipment(p, thingWithComps);
            }
            else
                Log.Warning(string.Concat("Could not make ", p, " equip or wear ", draggedItem));

            draggedItem = null;
        }

        private void TryEquipDraggedItem_Equipment(Pawn p, ThingWithComps eq)
        {
            if (!EquipmentUtility.CanEquip(draggedItem, p, out var str))
            {
                Messages.Message("MessageCantEquipCustom".Translate(str.CapitalizeFirst()), p, MessageTypeDefOf.RejectInput, false);
                draggedItem = null;
                return;
            }

            if (eq.def.IsWeapon)
            {
                if (p.guest.IsPrisoner)
                {
                    Messages.Message("MessageCantEquipCustom".Translate("MessagePrisonerCannotEquipWeapon".Translate(p.Named("PAWN"))), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (p.WorkTagIsDisabled(WorkTags.Violent))
                {
                    Messages.Message("MessageCantEquipIncapableOfViolence".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (p.WorkTagIsDisabled(WorkTags.Shooting) && draggedItem.def.IsRangedWeapon)
                {
                    Messages.Message("MessageCantEquipIncapableOfShooting".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }

                if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    Messages.Message("MessageCantEquipIncapableOfManipulation".Translate(), p, MessageTypeDefOf.RejectInput, false);
                    draggedItem = null;
                    return;
                }
            }
            //This was just duplicating everything lol            
            tmpExistingEquipment.Clear();
            tmpExistingEquipment.AddRange(p.equipment.AllEquipmentListForReading);

            for (var i = 0; i < tmpExistingEquipment.Count; i++)
            {
                
                p.equipment.Remove(tmpExistingEquipment[i]);
                SelOutpost.AddItem(tmpExistingEquipment[i]);
            }

            p.equipment.AddEquipment((ThingWithComps) SelOutpost.TakeItem(eq));

            draggedItem = null;
        }

        private static bool IsVisibleWeapon(ThingDef t) => t.IsWeapon && t != ThingDefOf.WoodLog && t != ThingDefOf.Beer;

        private static Pawn CurrentWearerOf(Thing t)
        {
            var parentHolder = t.ParentHolder;
            if (parentHolder is Pawn_EquipmentTracker or Pawn_ApparelTracker) return (Pawn) parentHolder.ParentHolder;

            return null;
        }

        private void MoveDraggedItemToInventory()
        {
            droppedDraggedItem = false;
            var apparel = draggedItem as Apparel;
            var pawn = CurrentWearerOf(draggedItem);
            if (pawn is not null)
            {
                if (apparel is not null)
                {
                    if (pawn.apparel.IsLocked(apparel))
                    {
                        Messages.Message("MessageCantUnequipLockedApparel".Translate(), CurrentWearerOf(apparel), MessageTypeDefOf.RejectInput, false);
                        draggedItem = null;
                        return;
                    }

                    pawn.apparel.Remove(apparel);
                }
                else
                    pawn.equipment.Remove((ThingWithComps) draggedItem);
            }

            SelOutpost.AddItem(draggedItem);

            draggedItem = null;
        }

        private void DoInventoryRow(ref float curY, Rect viewRect, Rect scrollOutRect, Thing t)
        {
            var num = rightPaneScrollPosition.y - 30f;
            var num2 = rightPaneScrollPosition.y + scrollOutRect.height;
            if (curY > num && curY < num2) DoInventoryRow(new Rect(0f, curY, viewRect.width, 30f), t);

            curY += 30f;
        }

        private void DoInventoryRow(Rect rect, Thing t)
        {
            GUI.BeginGroup(rect);
            var rect2 = rect.AtZero();
            Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, t);
            rect2.width -= 24f;
            if (draggedItem == null && Mouse.IsOver(rect2)) Widgets.DrawHighlight(rect2);

            var num = t == draggedItem ? 0.5f : 1f;
            var rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
            Widgets.ThingIcon(rect3, t, num);
            GUI.color = new Color(1f, 1f, 1f, num);
            var rect4 = new Rect(rect3.xMax + 4f, 0f, 250f, 30f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Widgets.Label(rect4, t.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.color = Color.white;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect2))
            {
                draggedItem = t;
                droppedDraggedItem = false;
                draggedItemPosOffset = new Vector2(16f, 16f);
                Event.current.Use();
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            GUI.EndGroup();
        }

        private void DoPawnRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
        {
            var pawns = Pawns;
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(new Rect(135f, curY + 6f, 200f, 100f), "DragToRearrange".Translate());
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
            for (var i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn.IsColonist) DoPawnRow(ref curY, scrollViewRect, scrollOutRect, pawn);
            }

            var flag = false;
            for (var j = 0; j < pawns.Count; j++)
            {
                var pawn2 = pawns[j];
                if (pawn2.IsPrisoner)
                {
                    if (!flag)
                    {
                        Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanPrisoners".Translate());
                        flag = true;
                    }

                    DoPawnRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
                }
            }
        }

        public override void ExtraOnGUI()
        {
            base.ExtraOnGUI();
            if (draggedItem != null)
            {
                var mousePosition = Event.current.mousePosition;
                var rect = new Rect(mousePosition.x - draggedItemPosOffset.x, mousePosition.y - draggedItemPosOffset.y, 32f, 32f);
                Find.WindowStack.ImmediateWindow(1283641090, rect, WindowLayer.Super, delegate
                {
                    if (draggedItem == null) return;

                    Widgets.ThingIcon(rect.AtZero(), draggedItem);
                }, false, false, 0f);
            }

            CheckDropDraggedItem();
        }

        private void DoRightPane()
        {
            var rect = new Rect(0f, 0f, rightPaneWidth, size.y).ContractedBy(10f);
            var rect2 = new Rect(0f, 0f, rect.width - 16f, rightPaneScrollViewHeight);
            if (draggedItem != null && rect.Contains(Event.current.mousePosition) && CurrentWearerOf(draggedItem) != null)
            {
                Widgets.DrawHighlight(rect);
                if (droppedDraggedItem)
                {
                    MoveDraggedItemToInventory();
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                }
            }

            var num = 0f;
            Widgets.BeginScrollView(rect, ref rightPaneScrollPosition, rect2);
            DoInventoryRows(ref num, rect2, rect);
            if (Event.current.type == EventType.Layout) rightPaneScrollViewHeight = num + 30f;

            Widgets.EndScrollView();
        }

        public override void FillTab()
        {
            allThings ??= new List<Thing>(SelOutpost.Things.Count());
            allThings.Clear();
            allThings.AddRange(SelOutpost.Things);
            Text.Font = GameFont.Small;
            CheckDraggedItemStillValid();
            CheckDropDraggedItem();
            var position = new Rect(0f, 0f, leftPaneWidth, size.y);
            GUI.BeginGroup(position);
            DoLeftPane();
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(position.xMax, 0f, rightPaneWidth, size.y));
            DoRightPane();
            GUI.EndGroup();
            if (draggedItem != null && droppedDraggedItem)
            {
                droppedDraggedItem = false;
                draggedItem = null;
            }
        }
    }
}