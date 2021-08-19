using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Reloading
{
    public class Command_ReloadableVerbTarget : Command_VerbTarget
    {
        public IReloadable Reloadable;

        public Command_ReloadableVerbTarget(IReloadable reloadable)
        {
            Reloadable = reloadable;
        }

        public override string TopRightLabel => Reloadable.ShotsRemaining + " / " + Reloadable.MaxShots;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (var option in base.RightClickFloatMenuOptions) yield return option;

                if (Reloadable is CompChangeableAmmo ccwa)
                    foreach (var option in ccwa.AmmoOptions.Select(pair =>
                        new FloatMenuOption(pair.First.LabelCap, pair.Second)))
                        yield return option;
            }
        }
    }
}