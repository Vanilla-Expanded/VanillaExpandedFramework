using Verse;

namespace VFECore
{
    public class ComProperties_AffectSkyWithToggle : CompProperties_AffectsSky
    {
        public ComProperties_AffectSkyWithToggle()
        {
            this.compClass = typeof(CompAffectSkyWithToggle);
        }
    }

    public class CompAffectSkyWithToggle : CompAffectsSky
    {
        public new ComProperties_AffectSkyWithToggle Props
        {
            get
            {
                return (ComProperties_AffectSkyWithToggle)this.props;
            }
        }

        public bool shouldAffectSky;
    }
}