namespace VFECore.Misc
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [StaticConstructorOnStartup]
    public static class HireableSystemStaticInitialization
    {
        public static List<Hireable> Hireables;

        static HireableSystemStaticInitialization()
        {
            Hireables = DefDatabase<HireableFactionDef>.AllDefs.GroupBy(def => def.commTag).Select(group => new Hireable(group.Key, group.ToList())).ToList();
            if (Hireables.Any())
            {
                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(Building_CommsConsole), nameof(Building_CommsConsole.GetCommTargets)),
                                              postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(GetCommTargets_Postfix)));

                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(LoadedObjectDirectory), "Clear"),
                                              postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(AddHireablesToLoadedObjectDirectory)));
            }
        }

        public static IEnumerable<ICommunicable> GetCommTargets_Postfix(IEnumerable<ICommunicable> communicables) => communicables.Concat(Hireables);

        public static void AddHireablesToLoadedObjectDirectory(LoadedObjectDirectory __instance)
        {
            foreach (Hireable hireable in Hireables)
                __instance.RegisterLoaded(hireable);
        }
    }

    public class Hireable : IGrouping<string, HireableFactionDef>, ICommunicable, ILoadReferenceable
    {
        private static readonly AccessTools.FieldRef<CrossRefHandler, LoadedObjectDirectory> loadedObjectInfo =
            AccessTools.FieldRefAccess<CrossRefHandler, LoadedObjectDirectory>("loadedObjectDirectory");

        private readonly List<HireableFactionDef> factions;

        public Hireable(string label, List<HireableFactionDef> list)
        {
            this.Key      = label;
            this.factions = list;

            loadedObjectInfo(Scribe.loader.crossRefs).RegisterLoaded(this);
        }

        public string GetCallLabel() => "VEF.Hire".Translate(this.Key.CapitalizeFirst());

        public string GetInfoText() => "VEF.HireDesc".Translate(this.Key.CapitalizeFirst());

        public void TryOpenComms(Pawn negotiator)
        {
            Find.WindowStack.Add(new Dialog_Hire(negotiator, this));
        }

        public Faction GetFaction() => null;

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator) => FloatMenuUtility.DecoratePrioritizedTask(
         new FloatMenuOption(this.GetCallLabel(), () => console.GiveUseCommsJob(negotiator, this), MenuOptionPriority.InitiateSocial), negotiator, console);

        public IEnumerator<HireableFactionDef> GetEnumerator() => this.factions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public string Key               { get; }
        public string GetUniqueLoadID() => $"{nameof(Hireable)}_{this.Key}";
    }
}