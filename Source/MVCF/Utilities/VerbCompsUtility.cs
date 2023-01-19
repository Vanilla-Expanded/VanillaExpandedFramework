using System.Collections.Generic;
using System.Linq;
using MVCF.VerbComps;

namespace MVCF.Utilities;

public static class VerbCompsUtility
{
    public static IEnumerable<VerbComp> GetComps(this ManagedVerb verb) => verb is VerbWithComps vwc ? vwc.GetComps() : Enumerable.Empty<VerbComp>();

    public static T TryGetComp<T>(this ManagedVerb verb) where T : VerbComp =>
        verb is VerbWithComps vwc ? vwc.GetComps().OfType<T>().FirstOrDefault() : default;
}
