// Decompiled with JetBrains decompiler
// Type: RRO.RandomOutcomeComp
// Assembly: RRO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DB012EEB-8B8C-475A-AEE6-3087EC66C203
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\VanillaExpandedFramework\1.4\Assemblies\RRO.dll

using RimWorld;
using System;
using System.Linq;
using Verse;

namespace RRO
{
  internal class RandomOutcomeComp : ThingComp
  {
    public CompProperties_RandomOutcomeComp Props => (CompProperties_RandomOutcomeComp) this.props;

    public Thing RandomWeapons()
    {
      this.Props.canProvideTags.Count<string>();
      ThingStuffPair thingStuffPair = ThingStuffPair.AllWith((Predicate<ThingDef>) (td => td.equipmentType == EquipmentType.Primary && td.weaponTags != null && td.weaponTags.Contains(this.Props.canProvideTags[0]))).RandomElement<ThingStuffPair>();
      return ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
    }

    public override void CompTick()
    {
      base.CompTick();
      if (this.parent.Map == null)
        return;
      Thing thing = this.RandomWeapons();
      QualityCategory qc;
      if (thing.TryGetComp<CompQuality>() != null && this.parent.TryGetQuality(out qc))
        thing.TryGetComp<CompQuality>().SetQuality(qc, ArtGenerationContext.Colony);
      GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Direct);
      this.parent.Destroy(DestroyMode.Vanish);
    }
  }
}
