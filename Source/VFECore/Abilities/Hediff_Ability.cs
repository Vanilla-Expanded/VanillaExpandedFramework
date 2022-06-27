namespace VFECore.Abilities
{
    using Verse;

    public class Hediff_Ability : HediffWithComps
    {
        public Ability ability;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ability, "ability");
        }
    }
}
