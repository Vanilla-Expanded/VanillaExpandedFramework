using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Things;

public static class CustomFireUtility
{
    private static readonly List<CustomFire> fireList = [];

    public static bool IsCustomFireFlammableNow(this Thing target, CustomFireDef customFireDef)
        => IsCustomFireFlammableNow(target, customFireDef?.flammabilityStatDef);

    public static bool IsCustomFireFlammableNow(this Thing target, StatDef customFireFlammabilityStat)
    {
        // No target, not flammable
        if (target == null)
            return false;

        // Vanilla flammability check
        if (customFireFlammabilityStat == null || customFireFlammabilityStat == StatDefOf.Flammability)
            return target.FlammableNow;

        // Identical check as vanilla FlammableNow, except using our stat for it.
        // We leave vanilla check for normal flammability in case it's harmony patched.
        if (target.GetStatValue(customFireFlammabilityStat) < 0.01f)
            return false;

        if (target.Spawned && !target.FireBulwark)
        {
            var thingList = target.Position.GetThingList(target.Map);
            if (thingList != null)
            {
                for (int i = 0; i < thingList.Count; ++i)
                {
                    if (thingList[i].FireBulwark)
                        return false;
                }
            }
        }
        return true;
    }

    public static bool IsBurningVanillaOrCustomFire(this TargetInfo t)
    {
        if (t.HasThing)
            return t.Thing.IsBurningVanillaOrCustomFire();
        return t.Cell.ContainsStaticVanillaOrCustomFire(t.Map);
    }

    public static bool IsBurningVanillaOrCustomFire(this Thing t) => t.IsBurning() || t.IsBurningCustomFire();

    public static bool ContainsStaticVanillaOrCustomFire(this IntVec3 c, Map map) => c.ContainsStaticFire(map) || c.ContainsStaticCustomFire(map);

    public static bool IsBurningCustomFire(this TargetInfo t, CustomFireDef customFireDef = null)
    {
        if (t.HasThing)
            return t.Thing.IsBurningCustomFire(customFireDef);
        return t.Cell.ContainsStaticCustomFire(t.Map, customFireDef);
    }

    public static bool IsBurningCustomFire(this Thing t, CustomFireDef customFireDef = null)
    {
        if (t.Destroyed || !t.Spawned)
            return false;

        if (t.def.size == IntVec2.One)
        {
            if (t is Pawn pawn)
            {
                var attachComp = pawn.GetComp<CompAttachBase>();
                if (attachComp?.attachments == null)
                    return false;
                // If looking for a specific fire type - check directly for it
                if (customFireDef != null)
                    return attachComp.HasAttachment(customFireDef);

                // If looking for any - check for any attachment with CustomFire class
                for (var i = 0; i < attachComp.attachments.Count; i++)
                {
                    if (attachComp.attachments[i] is CustomFire)
                        return true;
                }
            }

            return t.Position.ContainsStaticCustomFire(t.Map, customFireDef);
        }

        foreach (var item in t.OccupiedRect())
        {
            if (item.ContainsStaticCustomFire(t.Map, customFireDef))
                return true;
        }

        return false;
    }

    public static bool ContainsStaticCustomFire(this IntVec3 c, Map map, CustomFireDef customFireDef = null)
    {
        var list = map.thingGrid.ThingsListAt(c);

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is CustomFire { parent: null } fire && (customFireDef == null || fire.def == customFireDef))
                return true;
        }

        return false;
    }

    public static CustomFire GetFirstCustomFire(this Pawn pawn)
    {
        var attachComp = pawn?.GetComp<CompAttachBase>();
        if (attachComp?.attachments == null)
            return null;

        for (var i = 0; i < attachComp.attachments.Count; i++)
        {
            if (attachComp.attachments[i] is CustomFire fire)
                return fire;
        }

        return null;
    }

    public static List<CustomFire> GetCustomFireNearCell(this IntVec3 cell, Map map, CustomFireDef customFireDef = null)
    {
        fireList.Clear();

        var room = RegionAndRoomQuery.RoomAt(cell, map);
        if (room == null || room.Dereferenced || room.Fogged || room.IsHuge || room.TouchesMapEdge)
        {
            var region = cell.GetRegion(map);
            if (region == null)
            {
                var list = map.thingGrid.ThingsListAt(cell);
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] is CustomFire { parent: null } fire && (customFireDef == null || fire.def == customFireDef))
                        fireList.Add(fire);
                }
            }
            else if (customFireDef != null)
            {
                var things = region.ListerThings.ThingsOfDef(customFireDef);
                for (var i = 0; i < things.Count; i++)
                {
                    if (things[i] is CustomFire fire)
                        fireList.Add(fire);
                }
            }
            else
            {
                region.ListerThings.GetThingsOfType(fireList);
            }
        }
        else
        {
            var things = room.ContainedAndAdjacentThings;
            for (var i = 0; i < things.Count; i++)
            {
                if (things[i] is CustomFire fire && (customFireDef == null || fire.def == customFireDef))
                    fireList.Add(fire);
            }
        }

        fireList.Shuffle();
        fireList.Swap(0, fireList.FindIndex(0, f => f.Position == cell));

        return fireList;
    }

    public static bool HasAnyFireAttached(CompAttachBase attachComp)
    {
        if (attachComp?.attachments == null)
            return false;

        for (var i = 0; i < attachComp.attachments.Count; i++)
        {
            var attachment = attachComp.attachments[i];
            if (attachment.def == ThingDefOf.Fire || attachment is CustomFire)
                return true;
        }

        return false;
    }

    public static float ChanceToAttachCustomFireFromEvent(Thing t, CustomFireDef customFireDef)
        => ChanceToAttachCustomFireCumulative(t, 60f, customFireDef);

    public static float ChanceToAttachCustomFireCumulative(Thing t, float freqInTicks, CustomFireDef customFireDef)
    {
        if (!t.CanEverAttachFire())
            return 0f;
        if (HasAnyFireAttached(t.TryGetComp<CompAttachBase>()))
            return 0f;

        var num = customFireDef.chanceToCatchFirePerSecondForPawnFromFlammability.Evaluate(t.GetStatValue(customFireDef.flammabilityStatDef));
        return 1f - Mathf.Pow(1f - num, freqInTicks / 60f);
    }

    public static void TryAttachCustomFire(this Thing t, float fireSize, Thing instigator, CustomFireDef customFireDef)
    {
        if (t.CanEverAttachFire() && !HasAnyFireAttached(t.TryGetComp<CompAttachBase>()))
        {
            var fire = (CustomFire)ThingMaker.MakeThing(customFireDef);
            fire.fireSize = fireSize;
            fire.instigator = instigator;
            fire.AttachTo(t);
            GenSpawn.Spawn(fire, t.Position, t.Map, Rot4.North);

            if (t is Pawn pawn)
            {
                pawn.jobs.StopAll();
                pawn.records.Increment(RecordDefOf.TimesOnFire);
            }
        }
    }

    public static float ChanceToStartCustomFireIn(IntVec3 c, Map map, CustomFireDef customFireDef, SimpleCurve flammabilityChanceCurve = null)
    {
        var thingList = c.GetThingList(map);
        var flammability = c.TerrainFlammability(map);

        for (var i = 0; i < thingList.Count; i++)
        {
            var thing = thingList[i];
            if (thing is CustomFire fire && fire.def == customFireDef)
                return 0f;

            if (thing.def.category != ThingCategory.Pawn && thingList[i].IsCustomFireFlammableNow(customFireDef))
                flammability = Mathf.Max(flammability, thing.GetStatValue(customFireDef.flammabilityStatDef));
        }

        if (flammabilityChanceCurve != null)
            flammability = flammabilityChanceCurve.Evaluate(flammability);

        if (flammability > 0f)
        {
            var edifice = c.GetEdifice(map);
            if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
                return 0f;

            var things = c.GetThingList(map);
            for (var i = 0; i < things.Count; i++)
            {
                if (things[i].def.category == ThingCategory.Filth && !things[i].def.filth.allowsFire)
                    return 0f;
            }
        }

        return flammability;
    }

    public static bool TryStartCustomFireIn(IntVec3 c, Map map, float fireSize, Thing instigator, CustomFireDef customFireDef, SimpleCurve flammabilityChanceCurve = null)
    {
        if (ChanceToStartCustomFireIn(c, map, customFireDef, flammabilityChanceCurve) <= 0f)
            return false;

        var obj = (CustomFire)ThingMaker.MakeThing(customFireDef);
        obj.fireSize = fireSize;
        obj.instigator = instigator;
        GenSpawn.Spawn(obj, c, map, Rot4.North);
        return true;
    }

    public static bool Flammable(this TerrainDef terrain, CustomFireDef customFireDef) => customFireDef.Worker.Flammable(terrain);

    public static bool TerrainFlammableNow(this IntVec3 c, Map map, CustomFireDef customFireDef) => customFireDef.Worker.TerrainFlammableNow(c, map);

    public static float TerrainFlammability(this IntVec3 c, Map map, CustomFireDef customFireDef) => customFireDef.Worker.TerrainFlammability(c, map);
}