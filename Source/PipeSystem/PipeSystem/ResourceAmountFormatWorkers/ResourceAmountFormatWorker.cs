namespace PipeSystem;

public class ResourceAmountFormatWorker
{
    public virtual string FormatResourceAmount(float amount, PipeNetDef def, CompResource comp = null) => amount.ToString(def.resourceAmountFormat);

    public virtual string FormatResourceAmount(int amount, PipeNetDef def, CompResource comp = null) => amount.ToString(def.resourceAmountFormat);
}