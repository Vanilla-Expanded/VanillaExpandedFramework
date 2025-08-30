using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Factions;

[HarmonyPatch(typeof(Transferable), "CanAdjustBy")]
public static class Transferable_CanAdjustBy_Patch
{
    public static Transferable curTransferable;

    public static void Postfix(Transferable __instance)
    {
        if (curTransferable != __instance && Find.WindowStack.IsOpen<Dialog_Trade>() &&
            __instance.CountToTransferToDestination > 0 && TradeSession.trader != null)
        {
            foreach (ContrabandDef contrabandDef in DefDatabase<ContrabandDef>.AllDefs.Where(iid =>
                         !iid.factions.Contains(TradeSession.trader.Faction.def)))
            {
                if (contrabandDef.IsThingDefContraband(__instance.ThingDef, out var _, out var _, out var _))
                {
                    curTransferable = __instance;
                    if (TradeSession.giftMode)
                    {
                        foreach (TaggedString materialMessage in __instance.ThingDef.GetContrabandWarningMessages(true))
                        {
                            Messages.Message(materialMessage, MessageTypeDefOf.CautionInput);
                        }
                    }
                    else
                    {
                        foreach (TaggedString materialMessage in __instance.ThingDef.GetContrabandWarningMessages(false))
                        {
                            Messages.Message(materialMessage, MessageTypeDefOf.CautionInput);
                        }
                    }
                }
            }
        }
    }
}