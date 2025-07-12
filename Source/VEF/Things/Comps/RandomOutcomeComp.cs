using RimWorld;
using System;
using System.Linq;
using Verse;

namespace VEF.Things
{
    public class RandomOutcomeComp : ThingComp
    {
        public CompProperties_RandomOutcomeComp Props => (CompProperties_RandomOutcomeComp)this.props;

        public Thing RandomWeapons()
        {
            ThingStuffPair thingStuffPair = ThingStuffPair.AllWith((Predicate<ThingDef>)(td => td.equipmentType == EquipmentType.Primary && td.weaponTags != null && td.weaponTags.Contains(this.Props.canProvideTags[0]))).RandomElement<ThingStuffPair>();
            return ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.Map == null)
                return;
            Thing thing = this.RandomWeapons();
            if ((thing as ThingWithComps)?.compQuality is { } comp && this.parent.TryGetQuality(out var qc))
                comp.SetQuality(qc, ArtGenerationContext.Colony);
            GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Direct);
            this.parent.Destroy(DestroyMode.Vanish);
        }
    }
}
