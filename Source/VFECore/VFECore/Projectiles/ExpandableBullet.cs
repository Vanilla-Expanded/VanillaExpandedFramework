using Verse;

namespace VFECore
{
    public class ExpandableBullet : ExpandableProjectile
	{
        public override void DoDamage(IntVec3 pos)
        {
            base.DoDamage(pos);
            try
            {
                if (pos != this.launcher.Position && this.launcher.Map != null && GenGrid.InBounds(pos, this.launcher.Map))
                {
                    var list = this.launcher.Map.thingGrid.ThingsListAt(pos);
                    for (int num = list.Count - 1; num >= 0; num--)
                    {
                        if (IsDamagable(list[num]))
                        {
                            this.customImpact = true;
                            base.Impact(list[num]);
                            this.customImpact = false;
                        }
                    }
                }
            }
            catch { };
        }
    }
}
