using System.Text;
using RimWorld;
using Verse;

namespace VEF.Buildings;

public class CompStatsWhenPowered : ThingComp
{
    protected CompPowerTrader powerTrader;

    public CompProperties_StatsWhenPowered Props => (CompProperties_StatsWhenPowered)props;

    public virtual bool IsPowered => powerTrader is { PowerOn: true };

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        powerTrader = parent.GetComp<CompPowerTrader>();
    }

    public override void ReceiveCompSignal(string signal)
    {
        if (signal is CompPowerTrader.PowerTurnedOnSignal or CompPowerTrader.PowerTurnedOffSignal)
        {
            // Forcibly clear cache for specific stats when a power is turned on/off.
            // Generally avoid using, unless it's absolutely necessary, the stat is
            // extremely rarely re-cached or never re-cached, or we need to immediately
            // re-cache the stat due to us clearing the room cache.
            var list = Props.clearStatCacheOnPowerChange;
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    list[i].Worker.ClearCacheForThing(parent);
            }

            // The room stats don't get re-cached unless something in the room changes.
            // If one of the stats we're changing (like cleanliness) is tied to a room
            // stat as well, we'll need to force the room stats to be re-cached.
            // There isn't a way to force a single, specific room stat to be re-cached,
            // and there may be a reason for it, so I'm not making a custom system for it.
            if (Props.clearRoomCacheOnPowerChange)
                parent.GetRoom()?.Notify_BedTypeChanged();
        }
    }

    public override float GetStatOffset(StatDef stat)
    {
        if (IsPowered)
            return Props.poweredStatOffsets.GetStatOffsetFromList(stat);

        return Props.unpoweredStatOffsets.GetStatOffsetFromList(stat);
    }

    public override float GetStatFactor(StatDef stat)
    {
        if (IsPowered)
            return Props.poweredStatFactors.GetStatFactorFromList(stat);

        return Props.unpoweredStatFactors.GetStatFactorFromList(stat);
    }

    public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
    {
        var (text, offsets, factors) = IsPowered switch
        {
            true => ("VEF.StatsReport_Powered", Props.poweredStatOffsets, Props.poweredStatFactors),
            false => ("VEF.StatsReport_Unpowered", Props.unpoweredStatOffsets, Props.unpoweredStatFactors),
        };

        var offset = offsets.GetStatOffsetFromList(stat);
        if (offset != 0f)
            sb.AppendLine($"{text.Translate()}: {offset.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset)}");

        var factor = factors.GetStatFactorFromList(stat);
        if (factor != 1f)
            sb.AppendLine($"{text.Translate()}: {factor.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Factor)}");
    }
}