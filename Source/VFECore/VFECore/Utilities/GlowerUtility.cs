using Verse;

namespace VFECore;

public static class GlowerUtility
{
    public static bool IsDarklight(Thing thing)
    {
        var comp = thing.TryGetComp<CompGlower>();
        return comp != null && DarklightUtility.IsDarklight(comp.GlowColor.ProjectToColor32);
    }
}