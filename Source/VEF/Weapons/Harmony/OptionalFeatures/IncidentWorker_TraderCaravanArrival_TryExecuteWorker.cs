using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_IncidentWorker_TraderCaravanArrival_TryExecuteWorker_Patch
    {
        private static readonly HashSet<HistoryEventDef> factionImpacts = [];

        public static void DetectEmpireContraband(bool __result, IncidentParms parms)
        {
            if (__result)
            {
                Map map = (Map)parms.target;
                Faction faction = parms.faction;
                if (map != null && faction != null)
                {
                    List<Pawn> colonists = map.PlayerPawnsForStoryteller.ToList();
                    factionImpacts.Clear();
                    foreach (Pawn pawn in colonists)
                    {
                        CompUniqueWeapon comp = pawn.equipment?.Primary?.GetComp<CompUniqueWeapon>();
                        if (comp != null)
                        {
                            foreach (WeaponTraitDef item in comp.TraitsListForReading)
                            {
                                WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                                if (extension != null && !extension.factionRelationImpacts.NullOrEmpty())
                                {
                                    foreach (var impact in extension.factionRelationImpacts)
                                    {
                                        if (impact.factionDef == faction.def && impact.eventDef != null && impact.impact != 0 && factionImpacts.Add(impact.eventDef))
                                        {
                                            Faction.OfPlayer.TryAffectGoodwillWith(faction, impact.impact, canSendMessage: true, canSendHostilityLetter: true, impact.eventDef);
                                        }
                                    }
                                }
                            }
                        }
                        


                    }
                    factionImpacts.Clear();
                }


            }
        }
    }


}
