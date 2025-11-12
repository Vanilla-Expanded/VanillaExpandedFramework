using System.Collections.Generic;

namespace PipeSystem;

using Verse;

public class AutoRefuelableLinkingExtension : DefModExtension
{
    /// <summary>
    /// A list of pipe nets which this refuelable won't automatically link with.
    /// </summary>
    public List<PipeNetDef> disabledAutoLinkingNetDefs;
}