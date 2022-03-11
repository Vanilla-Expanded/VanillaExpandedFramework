using System.Collections.Generic;
using System.Linq;
using MVCF.Reloading.Comps;
using Verse;

namespace MVCF.Commands
{
    public class Command_ReloadableVerbTarget : Command_VerbTargetExtended
    {
        public VerbComp_Reloadable Reloadable;

        public Command_ReloadableVerbTarget(VerbComp_Reloadable reloadable, ManagedVerb mv, Thing ownerThing) : base(mv, ownerThing)
        {
            Reloadable = reloadable;
            if (reloadable.ShotsRemaining < verb.verbProps.burstShotCount)
                Disable("CommandReload_NoAmmo".Translate("ammo".Named("CHARGENOUN"),
                    reloadable.Props.AmmoFilter.AnyAllowedDef.Named("AMMO"),
                    ((reloadable.Props.MaxShots - reloadable.ShotsRemaining) * reloadable.Props.ItemsPerShot).Named("COUNT")));
        }

        public override string TopRightLabel => Reloadable.ShotsRemaining + " / " + Reloadable.Props.MaxShots;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (var option in base.RightClickFloatMenuOptions) yield return option;

                if (Reloadable is VerbComp_Reloadable_ChangeableAmmo ccwa)
                    foreach (var option in ccwa.AmmoOptions.Select(pair =>
                        new FloatMenuOption(pair.First.LabelCap, pair.Second)))
                        yield return option;
            }
        }
    }
}