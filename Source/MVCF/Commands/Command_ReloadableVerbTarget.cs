using System.Collections.Generic;
using System.Linq;
using Reloading;
using Verse;

namespace MVCF.Commands
{
    public class Command_ReloadableVerbTarget : Command_VerbTargetExtended
    {
        public IReloadable Reloadable;

        public Command_ReloadableVerbTarget(IReloadable reloadable, ManagedVerb mv) : base(mv)
        {
            Reloadable = reloadable;
            if (reloadable.ShotsRemaining < verb.verbProps.burstShotCount)
                Disable("CommandReload_NoAmmo".Translate("ammo".Named("CHARGENOUN"),
                    reloadable.AmmoExample.Named("AMMO"),
                    ((reloadable.MaxShots - reloadable.ShotsRemaining) * reloadable.ItemsPerShot).Named("COUNT")));
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