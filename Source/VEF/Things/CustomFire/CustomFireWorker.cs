using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VEF.Things;

public class CustomFireWorker
{
    public CustomFireDef fireDef;

    public virtual FloatMenuOption GetExtinguishPawnFloatMenuOption(Pawn clickedPawn, FloatMenuContext context, CustomFire fire)
    {
        return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PrioritizeGeneric".Translate(WorkGiverDefOf.FightFires.gerund, clickedPawn.Label).CapitalizeFirst(), () =>
        {
            var job = JobMaker.MakeJob(fire.Def.beatFireJobDef, fire);
            context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }), context.FirstSelectedPawn, fire);
    }

    public virtual FloatMenuOption GetExtinguishFloatMenuOption(FloatMenuContext context, AcceptanceReport report)
    {
        // Accepted, return a float menu option to extinguish
        if (report.Accepted)
        {
            return new FloatMenuOption("ExtinguishFiresNearby".Translate(), () =>
            {
                foreach (var validSelectedPawn in context.ValidSelectedPawns)
                {
                    if (PawnCanExtinguish(validSelectedPawn, context.ClickedCell).Accepted)
                    {
                        var job = JobMaker.MakeJob(fireDef.extinguishFiresNearbyJobDef);
                        foreach (var target in context.ClickedCell.GetCustomFireNearCell(context.FirstSelectedPawn.Map, fireDef))
                            job.AddQueuedTarget(TargetIndex.A, target);
                        validSelectedPawn.jobs.TryTakeOrderedJob(job);
                    }
                }
            }, fireDef);
        }

        // No reason given (no pawns selected?), don't show a float menu
        if (report.Reason.NullOrEmpty())
            return null;

        return new FloatMenuOption($"{"CannotGenericWorkCustom".Translate((NamedArgument)WorkGiverDefOf.FightFires.label)}: {report.Reason}", null);
    }

    public virtual AcceptanceReport PawnCanExtinguish(Pawn pawn, IntVec3 cell)
    {
        if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Firefighter))
            return $"{"IncapableOf".Translate().CapitalizeFirst()} {WorkTypeDefOf.Firefighter.gerundLabel}";

        if (!pawn.CanReach(cell, PathEndMode.ClosestTouch, Danger.Deadly))
        {
            var thing = cell.GetFirstThing(pawn.Map, fireDef);
            if (thing == null || !pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly))
                return "NoPath".Translate().CapitalizeFirst();
        }

        if (!cell.InAllowedArea(pawn))
            return $"{"CannotPrioritizeForbiddenOutsideAllowedArea".Translate()} ({pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.Label})";

        return true;
    }

    public virtual bool Flammable(TerrainDef terrain)
    {
        // Run vanilla code in case of Harmony patches
        if (fireDef.flammabilityStatDef == StatDefOf.Flammability)
            return terrain.Flammable();
        return terrain.GetStatValueAbstract(fireDef.flammabilityStatDef) > 0.01f;
    }

    public virtual bool TerrainFlammableNow(IntVec3 c, Map map)
    {
        // Run vanilla code in case of Harmony patches
        if (fireDef.flammabilityStatDef == StatDefOf.Flammability)
            return c.TerrainFlammableNow(map);

        var terrain = c.GetTerrain(map);
        var foundation = map.terrainGrid.FoundationAt(c);

        if (!Flammable(terrain) && (foundation == null || !Flammable(terrain)))
            return false;

        var things = c.GetThingList(map);
        for (var i = 0; i < things.Count; i++)
        {
            if (things[i].FireBulwark)
                return false;
        }

        return true;
    }

    public virtual float TerrainFlammability(IntVec3 c, Map map)
    {
        // Run vanilla code in case of Harmony patches
        if (fireDef.flammabilityStatDef == StatDefOf.Flammability)
            return c.TerrainFlammability(map);

        if (!TerrainFlammableNow(c, map))
            return 0f;

        var terrain = c.GetTerrain(map);
        var foundation = map.terrainGrid.FoundationAt(c);

        var flammability = terrain.GetStatValueAbstract(fireDef.flammabilityStatDef);
        if (foundation != null)
            flammability = Mathf.Max(flammability, foundation.GetStatValueAbstract(fireDef.flammabilityStatDef));

        return flammability;
    }

    public virtual float GetEffectiveVacuumForFire(IntVec3 c, Map map)
    {
        // Use vanilla code. Custom workers can override this and use something else.
        return FireUtility.GetEffectiveVacuumForFire(c, map);
    }
}