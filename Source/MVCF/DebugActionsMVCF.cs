using System.Linq;
using LudeonTK;
using MVCF.Comps;
using MVCF.Utilities;
using Verse;

namespace MVCF;

public static class DebugActionsMVCF
{
    [DebugAction("Pawns", "Log MVCF ManagedVerbs", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void LogManagedVerbs(Pawn p)
    {
        var man = p.Manager(false);
        if (man == null) return;
        DebugActionsUtility.DustPuffFrom(p);
        Log.Message("All ManagedVerbs for " + p.LabelCap);
        foreach (var verb in man.ManagedVerbs)
        {
            Log.Message(
                $"  {verb.Verb} ({verb.Verb.loadID}, {verb.GetUniqueLoadID()}): {(verb.Enabled ? "Enabled" : "Disabled")}{(verb.GetComps().Any() ? " Comps:" : "")}");
            foreach (var comp in verb.GetComps()) Log.Message($"    {comp}");
        }
    }

    [DebugAction("General", "Log Verbs", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void LogVerbs()
    {
        var c = UI.MouseCell();
        foreach (var thing in c.GetThingList(Find.CurrentMap).OfType<ThingWithComps>())
        {
            Log.Message($"All Verbs for {thing.LabelCap}");
            foreach (var verb in thing.AllComps.SelectMany(comp => comp switch
                     {
                         CompEquippable eq => eq.AllVerbs,
                         Comp_VerbGiver giver => giver.VerbTracker.AllVerbs,
                         _ => Enumerable.Empty<Verb>()
                     }))
                Log.Message($"  {verb} ({verb.loadID}, {verb.GetUniqueLoadID()})");
        }
    }
}
