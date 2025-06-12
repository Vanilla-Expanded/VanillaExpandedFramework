﻿using RimWorld;
using Verse;

namespace VEF.Buildings;

// Comp roughly based off of CompCauseHediff_AoE, however it's changed to fit our needs,
// as well as making it possible to modify if further with subclasses.
// Due to not needing those at the time, some features weren't included:
// - Sustainer when active
// - No support for HediffComp_Link
// - No support for drawing lines between the pawn and the building
public class CompCustomCauseHediff_AoE : ThingComp
{
    protected static Room tempWorkingRoom = null;

    protected CompPowerTrader powerTrader;

    protected CompProperties_CustomCauseHediff_AoE Props => (CompProperties_CustomCauseHediff_AoE)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        powerTrader = parent.GetComp<CompPowerTrader>();
    }

    public override void CompTick()
    {
        if (!parent.IsHashIntervalTick(Props.checkInterval))
            return;

        // Make sure the power is on
        if (powerTrader is not { PowerOn: true })
            return;

        if (!parent.SpawnedOrAnyParentSpawned)
            return;

        try
        {
            switch (Props.worksInside, Props.worksOutside)
            {
                case (false, false):
                    return;
                case (true, false):
                    tempWorkingRoom = parent.GetRoom();
                    if (tempWorkingRoom == null || tempWorkingRoom.PsychologicallyOutdoors)
                        return;
                    break;
                case (false, true):
                    tempWorkingRoom = parent.GetRoom();
                    if (tempWorkingRoom is { PsychologicallyOutdoors: false })
                        return;
                    break;
            }

            var pawnList = parent.MapHeld.mapPawns.AllPawnsSpawned;
            for (var index = 0; index < pawnList.Count; index++)
            {
                var pawn = pawnList[index];
                if (IsPawnAffectedAndInRange(pawn, true))
                    GiveOrUpdateHediff(pawn);
                if (pawn.carryTracker.CarriedThing is Pawn otherPawn && IsPawnAffectedAndInRange(otherPawn, true))
                    GiveOrUpdateHediff(otherPawn);
            }
        }
        finally
        {
            tempWorkingRoom = null;
        }
    }

    // The return value is unused here, but is planned to be used by subtypes 
    // ReSharper disable once UnusedMethodReturnValue.Global
    protected virtual Hediff GiveOrUpdateHediff(Pawn target)
    {
        var hediff = target.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
        if (hediff == null)
        {
            hediff = target.health.AddHediff(Props.hediff);
            hediff.Severity = Props.startingSeverity;
        }

        if (Props.hediffDuration > 0)
        {
            if (hediff is HediffWithComps hediffWithComps)
            {
                var disappears = hediffWithComps.GetComp<HediffComp_Disappears>();
                if (disappears == null)
                    Log.ErrorOnce($"{parent.def.defName} has {nameof(CompCustomCauseHediff_AoE)} with positive {nameof(Props.hediffDuration)} and has a hediff in props which does not have a {nameof(HediffComp_Disappears)}", Gen.HashCombineInt(808055567, hediff.def.shortHash));
                else
                    disappears.ticksToDisappear = Props.hediffDuration;
            }
            else
                Log.ErrorOnce($"{parent.def.defName} has {nameof(CompCustomCauseHediff_AoE)} with positive {nameof(Props.hediffDuration)} and has a hediff which is not {nameof(HediffWithComps)}", Gen.HashCombineInt(-837742526, hediff.def.shortHash));
        }

        return hediff;
    }

    // Primarily to be used outside of this type or its subtypes
    public bool IsPawnAffectedAndInRange(Pawn pawn) => IsPawnAffected(pawn) && IsPositionInRange(pawn.PositionHeld, false);

    protected bool IsPawnAffectedAndInRange(Pawn pawn, bool cacheRoom) => IsPawnAffected(pawn) && IsPositionInRange(pawn.PositionHeld, cacheRoom);

    public virtual bool IsPawnAffected(Pawn target)
    {
        // Make sure the pawn is alive and can receive a hediff
        if (target.health == null || target.Dead)
            return false;

        if (!IsAllowedPawnType(target))
            return false;

        // Make sure pawn is awake
        if (Props.mustBeAwake && !target.Awake())
            return false;

        var capacities = Props.requiredCapacities;
        if (capacities != null)
        {
            // Make sure that the pawn has required capacities
            for (var i = 0; i < capacities.Count; i++)
            {
                var capacity = capacities[i];
                if (!target.health.capacities.CapableOf(capacity))
                    return false;
            }
        }

        return true;
    }

    // Primarily to be used outside of this type or its subtypes
    public bool IsPositionInRange(IntVec3 position) => IsPositionInRange(position, false);

    protected virtual bool IsPositionInRange(IntVec3 position, bool cacheRoom)
    {
        var props = Props;
        // Make sure position is in range
        // Using squared distance is faster than calculating a square root, and can be safely used to compare distances.
        if (props.range > 0 && position.DistanceToSquared(parent.PositionHeld) > props.range * props.range)
            return false;

        if (!props.sameRoomOnly)
            return true;

        // Make sure the position is in the same room
        if (cacheRoom)
            return position.GetRoom(parent.MapHeld) == (tempWorkingRoom ??= parent.GetRoom());
        return position.GetRoom(parent.MapHeld) == (tempWorkingRoom ?? parent.GetRoom());
    }

    protected virtual bool IsAllowedPawnType(Pawn target)
    {
        var raceProps = target.RaceProps;

        // Make sure Pawn type is allowed 
        if (raceProps.Humanlike)
            return Props.allowHumanlike;
        if (raceProps.Dryad)
            return Props.allowDryads;
        if (raceProps.Insect)
            return Props.allowInsects;
        if (raceProps.Animal)
            return Props.allowAnimals;
        if (raceProps.IsMechanoid)
            return Props.allowMechanoids;
        if (raceProps.IsAnomalyEntity)
            return Props.allowEntities;

        // Other/unsupported race type.
        return false;
    }
}