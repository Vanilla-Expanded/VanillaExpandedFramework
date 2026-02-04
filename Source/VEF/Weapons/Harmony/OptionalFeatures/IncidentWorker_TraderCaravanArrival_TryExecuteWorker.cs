using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VEF.Weapons
{

    // This Harmony patch will only be patched if WeaponTraitDefMechanics is added via XML to a mod using OptionalFeatures

    public static class VanillaExpandedFramework_IncidentWorker_TraderCaravanArrival_TryExecuteWorker_Patch
    {

        public static void DetectEmpireContraband(bool __result, IncidentParms parms)
        {
            if (__result)
            {
                Map map = (Map)parms.target;
                Faction faction = parms.faction;
                if (map != null && faction == Faction.OfEmpire)
                {
                    bool foundAColonist = false;
                    List<Pawn> colonists = map.PlayerPawnsForStoryteller.ToList();
                    foreach (Pawn pawn in colonists)
                    {
                        if (!foundAColonist)
                        {
                            CompUniqueWeapon comp = pawn.equipment?.Primary?.GetComp<CompUniqueWeapon>();
                            if (comp != null)
                            {
                                foreach (WeaponTraitDef item in comp.TraitsListForReading)
                                {
                                    WeaponTraitDefExtension extension = item.GetModExtension<WeaponTraitDefExtension>();
                                    if (extension != null)
                                    {
                                        if (extension.relationsImpactWithEmpire != 0)
                                        {
                                            Faction.OfPlayer.TryAffectGoodwillWith(Faction.OfEmpire, extension.relationsImpactWithEmpire, canSendMessage: true, canSendHostilityLetter: true, InternalDefOf.VWE_DeserterMarkings);
                                            foundAColonist = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        


                    }
                }


            }
        }
    }


}
