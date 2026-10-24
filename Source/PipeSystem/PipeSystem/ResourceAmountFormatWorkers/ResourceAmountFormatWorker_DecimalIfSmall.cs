using Verse;

namespace PipeSystem;

public class ResourceAmountFormatWorker_DecimalIfSmall : ResourceAmountFormatWorker
{
    public override string FormatResourceAmount(float amount, PipeNetDef def, CompResource comp = null) => amount.ToStringDecimalIfSmall();
}