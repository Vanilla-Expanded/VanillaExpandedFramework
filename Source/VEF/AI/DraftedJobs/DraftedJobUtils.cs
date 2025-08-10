using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using Verse.AI.Group;
using HarmonyLib;
using Verse.Noise;
using UnityEngine;

namespace VEF.AI
{
    public class Command_ToggleWithRClick : Command_Toggle
    {
        public Action rightClickAction;

        public override void ProcessInput(Event ev)
        {
            if (ev.button == 1)
            {
                rightClickAction?.Invoke();
            }
            else
            {
                base.ProcessInput(ev);
            }
        }
    }

    [HarmonyPatch]
    [StaticConstructorOnStartup]
    public static class DraftGizmos
    {
        public static readonly Texture2D AutoCastTex = ContentFinder<Texture2D>.Get("UI/CheckAuto");
        public static readonly Texture2D VEFHuntIcon = ContentFinder<Texture2D>.Get("UI/Commands/VEF_Hunt");

        public static bool IsPlayerDraftedInsectoid(Pawn pawn)
        {
            return pawn?.Faction == Faction.OfPlayerSilentFail && pawn.Drafted && pawn?.RaceProps?.Animal == true && pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid;
        }

        [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> GetGizmosPostfix(IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
        {
            static IEnumerable<Gizmo> UpdateEnumerable(IEnumerable<Gizmo> gizmos, Command_ToggleWithRClick draftHunt)
            {
                foreach (var gizmo in gizmos)
                {
                    if (gizmo is Command_Toggle draftCommand && draftCommand.icon == TexCommand.Draft)
                    {
                        yield return draftCommand;
                        yield return draftHunt;
                    }
                    else
                    {
                        yield return gizmo;
                    }
                }
            }

            var pawn = __instance.pawn;
            if (IsPlayerDraftedInsectoid(pawn))
            {
                // Add Hunt Toggle Gizmo
                var huntCommand = new Command_ToggleWithRClick
                {
                    defaultLabel = "VEF.DraftHuntLabel".Translate(),
                    defaultDesc = "VEF.HuntDescription".Translate(),
                    icon = VEFHuntIcon,
                    isActive = () => DraftedActionHolder.GetData(pawn).hunt,
                    toggleAction = () => DraftedActionHolder.GetData(pawn).ToggleHuntMode(),
                    rightClickAction = () =>
                    {
                        DraftedActionData data = DraftedActionHolder.GetData(pawn);
                        data.ToggleAutoForAll();
                    },
                    activateSound = SoundDefOf.Click,
                    groupKey = 6173612,
                    hotKey = KeyBindingDefOf.Misc1
                };
                return UpdateEnumerable(__result, huntCommand);
            }
            return __result;
        }

        [HarmonyPatch(typeof(Command), "GizmoOnGUIInt")]
        [HarmonyPostfix]
        public static void GizmoOnGUIPostfix(Command __instance, GizmoResult __result, Rect butRect, GizmoRenderParms parms)
        {
            // If instance is EXACTLY Command_Ability, and not a subclass of it. We don't want to mess with people's modded abilities that might have special logic.
            if (__instance.GetType() == typeof(Command_Ability))
            {
                var cmd = __instance as Command_Ability;
                var pawn = cmd.Pawn;
                if (IsPlayerDraftedInsectoid(pawn))
                {
                    var data = DraftedActionHolder.GetData(pawn);
                    if (data.AutoCastFor(cmd.Ability.def))
                    {
                        var size = parms.shrunk ? 12f : 24f;
                        Rect position = new(butRect.x + butRect.width - size, butRect.y, size, size);
                        GUI.DrawTexture(position, AutoCastTex);
                    }
                    if (__result.State == GizmoState.OpenedFloatMenu)
                    {
                        data.ToggleAutoCastFor(cmd.Ability.def);
                    }
                }
            }
        }
    }

    public class DraftedActionHolder : GameComponent
    {
        public static Dictionary<string, DraftedActionData> pawnDraftActionData = new();

        public static DraftedActionData GetData(Pawn pawn)
        {
            if (pawnDraftActionData.TryGetValue(pawn.ThingID, out DraftedActionData data))
            {
                return data;
            }
            pawnDraftActionData[pawn.ThingID] = new DraftedActionData(pawn);
            return pawnDraftActionData[pawn.ThingID];
        }

        public DraftedActionHolder(Game game) : base() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pawnDraftActionData, "draftedActions", LookMode.Value, LookMode.Deep);
        }
    }

    public class DraftedActionData : IExposable
    {
        private Pawn pawn = null;
        public string pawnID;
        public bool hunt = false;
        public List<AbilityDef> autocastAbilities = new();

        public Pawn Pawn  // Always use this to get the pawn, not the field since it might be null.
        {
            get
            {
                if (pawn == null)
                {
                    // Get pawn using the pawnID
                    var allPawns = Find.Maps?.SelectMany(x => x.mapPawns.AllPawns);
                    // Get all pawns from the game and add them to the cache
                    foreach (var pawn in allPawns.Where(x => x.ThingID == pawnID))
                    {
                        this.pawn = pawn;
                        break;
                    }
                    if (pawn == null)
                    {
                        Log.Error($"DraftedActionData could not find pawn with ID {pawnID}.");
                    }
                }
                return pawn;
            }
        }

        public DraftedActionData(Pawn pawn)
        {
            this.pawn = pawn;
            this.pawnID = pawn.ThingID;
        }

        public DraftedActionData() { }   // For scribe

        private void RefreshDraft()
        {
            Pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        public bool ToggleHuntMode()
        {
            hunt = !hunt;
            RefreshDraft();
            return hunt;
        }

        public bool AutoCastFor(AbilityDef def)
        {
            return autocastAbilities.Contains(def);
        }

        public void ToggleAutoForAll()
        {
            if (autocastAbilities.Empty() && Pawn?.abilities?.abilities != null)
            {
                foreach (var ability in Pawn.abilities.abilities)
                {
                    if (ability.def.aiCanUse)
                    {
                        autocastAbilities.Add(ability.def);
                    }
                }
            }
            else autocastAbilities.Clear();
            RefreshDraft();
        }

        public void ToggleAutoCastFor(AbilityDef def)
        {
            if (autocastAbilities.Contains(def))
            {
                autocastAbilities.Remove(def);
            }
            else
            {
                autocastAbilities.Add(def);
            }
            RefreshDraft();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref pawnID, "pawnID");
            Scribe_Values.Look(ref hunt, "huntMode", false);
            Scribe_Collections.Look(ref autocastAbilities, "autocastAbilities", LookMode.Def);
        }
    }

}
