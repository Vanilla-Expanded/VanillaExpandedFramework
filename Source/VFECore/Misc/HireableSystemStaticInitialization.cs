namespace VFECore.Misc
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using RimWorld.Planet;
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
                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(QuestUtility), nameof(QuestUtility.IsQuestLodger)),
                                              postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(IsQuestLodger_Postfix)));
                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.QuestLodgerCanUnequip)),
                                              postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(QuestLodgerCanUnequip_Postfix)));
                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.AllSendablePawns)),
                                              transpiler: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(CaravanAllSendablePawns_Transpiler)));
            }
        }

        public static IEnumerable<ICommunicable> GetCommTargets_Postfix(IEnumerable<ICommunicable> communicables) =>
            Find.World.GetComponent<HiringContractTracker>().pawns.Any() ? communicables : communicables.Concat(Hireables);

        public static void AddHireablesToLoadedObjectDirectory(LoadedObjectDirectory __instance)
        {
            foreach (Hireable hireable in Hireables)
                __instance.RegisterLoaded(hireable);
        }

        public static void IsQuestLodger_Postfix(Pawn p, ref bool __result)
        {
            __result = __result || Find.World.GetComponent<HiringContractTracker>().pawns.Contains(p);
        }

        public static void QuestLodgerCanUnequip_Postfix(Pawn pawn, ref bool __result)
        {
            __result = __result && !Find.World.GetComponent<HiringContractTracker>().pawns.Contains(pawn);
        }

        public static IEnumerable<CodeInstruction> CaravanAllSendablePawns_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo questLodger = AccessTools.Method(typeof(QuestUtility), nameof(QuestUtility.IsQuestLodger));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(questLodger))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return instruction;
                    yield return CodeInstruction.Call(typeof(HireableSystemStaticInitialization), nameof(CaravanAllSendablePawns_Helper));
                }
                else
                    yield return instruction;
            }
        }

        public static bool CaravanAllSendablePawns_Helper(Pawn pawn, bool questLodger) =>
            questLodger && !Find.World.GetComponent<HiringContractTracker>().pawns.Contains(pawn);
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