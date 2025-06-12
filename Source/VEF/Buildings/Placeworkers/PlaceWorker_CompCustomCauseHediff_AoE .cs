﻿using UnityEngine;
using Verse;

namespace VEF.Buildings;

public class PlaceWorker_CompCustomCauseHediff_AoE : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var props = def.GetCompProperties<CompProperties_CustomCauseHediff_AoE>();
        if (props == null)
            return;

        var map = Find.CurrentMap;
        var room = RegionAndRoomQuery.RoomAt(center, map);

        if ((props.worksInside && props.worksOutside) || (props.worksInside && room is { PsychologicallyOutdoors: false }) || (props.worksOutside && (room == null || room.PsychologicallyOutdoors)))
        {
            switch (props.range, props.sameRoomOnly, room is { AnyPassable: true })
            {
                case ( > 0, true, true):
                    GenDraw.DrawRadiusRing(center, props.range, Color.white, cell => room == cell.GetRoom(map));
                    break;
                case ( > 0, _, _):
                    GenDraw.DrawRadiusRing(center, props.range);
                    break;
                case (_, true, true):
                    GenDraw.DrawRadiusRing(center, GenRadial.MaxRadialPatternRadius - 1f, Color.white, cell => room == cell.GetRoom(map));
                    break;
                case (_, _, _):
                    break;
            }
        }
    }
}