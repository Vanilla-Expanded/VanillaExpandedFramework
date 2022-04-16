using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore.Misc
{
    [StaticConstructorOnStartup]
    public static class HireableSystemStaticInitialization
    {
        public static List<Hireable> Hireables;

        private static HiringContractTracker cachedTracker      = null;
        private static World                 cachedTrackerWorld = null;

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
                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.CheckAcceptArrest)),
                                              postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(CheckAcceptArrestPostfix)));
                // VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(PawnApparelGenerator), "GenerateWorkingPossibleApparelSetFor"),
                //     new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(Debug)));
                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(BillUtility), nameof(BillUtility.IsSurgeryViolationOnExtraFactionMember)),
                                              postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(IsSurgeryViolation_Postfix)));
                VFECore.harmonyInstance.Patch(AccessTools.Method(typeof(ForbidUtility), nameof(ForbidUtility.CaresAboutForbidden)),
                                              postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(CaresAboutForbidden_Postfix)));
            }
        }

        // public static void Debug(Pawn pawn, float money, List<ThingStuffPair> apparelCandidates)
        // {
        //     if (DebugViewSettings.logApparelGeneration)
        //         Log.Message($"Generating apparel for {pawn} with money ${money} and candidates:\n{apparelCandidates.Select(pair => pair.ToString()).ToLineList("  - ")}");
        // }

        private static HiringContractTracker GetContractTracker(World world)
        {
            if (cachedTrackerWorld != world)
            {
                cachedTracker      = world.GetComponent<HiringContractTracker>();
                cachedTrackerWorld = world;
            }

            return cachedTracker;
        }

        public static IEnumerable<ICommunicable> GetCommTargets_Postfix(IEnumerable<ICommunicable> communicables) =>
            Find.World.GetComponent<HiringContractTracker>().pawns.Any() ? communicables.Concat(Find.World.GetComponent<HiringContractTracker>()) : communicables.Concat(Hireables);

        public static void AddHireablesToLoadedObjectDirectory(LoadedObjectDirectory __instance)
        {
            foreach (var hireable in Hireables)
                __instance.RegisterLoaded(hireable);
        }

        public static void IsQuestLodger_Postfix(Pawn p, ref bool __result)
        {
            __result = __result || GetContractTracker(Find.World).IsHired(p);
        }

        public static void QuestLodgerCanUnequip_Postfix(Pawn pawn, ref bool __result)
        {
            __result = __result && pawn.RaceProps.Humanlike && !GetContractTracker(Find.World).IsHired(pawn);
        }

        public static IEnumerable<CodeInstruction> CaravanAllSendablePawns_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var questLodger = AccessTools.Method(typeof(QuestUtility), nameof(QuestUtility.IsQuestLodger));

            foreach (var instruction in instructions)
                if (instruction.Calls(questLodger))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return instruction;
                    yield return CodeInstruction.Call(typeof(HireableSystemStaticInitialization), nameof(CaravanAllSendablePawns_Helper));
                }
                else
                    yield return instruction;
        }

        public static bool CaravanAllSendablePawns_Helper(Pawn pawn, bool questLodger) =>
            questLodger && !GetContractTracker(Find.World).IsHired(pawn);

        public static void CheckAcceptArrestPostfix(Pawn __instance, ref bool __result)
        {
            var tracker = GetContractTracker(Find.World);
            if (tracker.IsHired(__instance))
            {
                tracker.BreakContract();
                __result = false;
            }
        }

        public static void IsSurgeryViolation_Postfix(Bill_Medical bill, ref bool __result)
        {
            __result = __result || (GetContractTracker(Find.World).IsHired(bill.GiverPawn) && bill.recipe.Worker.IsViolationOnPawn(bill.GiverPawn, bill.Part, Faction.OfPlayer));
        }

        public static void CaresAboutForbidden_Postfix(Pawn pawn, ref bool __result)
        {
            __result = __result && (!GetContractTracker(Find.World).IsHired(pawn) || pawn.CurJobDef != VFEDefOf.VFEC_LeaveMap);
        }
    }

    public class Hireable : IGrouping<string, HireableFactionDef>, ICommunicable, ILoadReferenceable
    {
        private static readonly AccessTools.FieldRef<CrossRefHandler, LoadedObjectDirectory> loadedObjectInfo =
            AccessTools.FieldRefAccess<CrossRefHandler, LoadedObjectDirectory>("loadedObjectDirectory");

        private readonly List<HireableFactionDef> factions;

        public Hireable(string label, List<HireableFactionDef> list)
        {
            Key      = label;
            factions = list;

            loadedObjectInfo(Scribe.loader.crossRefs).RegisterLoaded(this);
        }

        public string GetCallLabel() => "VEF.Hire".Translate(Key.CapitalizeFirst());

        public string GetInfoText() => "VEF.HireDesc".Translate(Key.CapitalizeFirst());

        public void TryOpenComms(Pawn negotiator)
        {
            Find.WindowStack.Add(new Dialog_Hire(negotiator, this));
        }

        public Faction GetFaction() => null;

        public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator) => FloatMenuUtility.DecoratePrioritizedTask(
         new FloatMenuOption(GetCallLabel(), () => console.GiveUseCommsJob(negotiator, this), MenuOptionPriority.InitiateSocial), negotiator, console);

        public IEnumerator<HireableFactionDef> GetEnumerator() => factions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public string Key               { get; }
        public string GetUniqueLoadID() => $"{nameof(Hireable)}_{Key}";
    }
}