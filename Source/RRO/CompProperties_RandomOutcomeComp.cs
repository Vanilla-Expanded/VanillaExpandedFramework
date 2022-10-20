// Decompiled with JetBrains decompiler
// Type: RRO.CompProperties_RandomOutcomeComp
// Assembly: RRO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DB012EEB-8B8C-475A-AEE6-3087EC66C203
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\VanillaExpandedFramework\1.4\Assemblies\RRO.dll

using System.Collections.Generic;
using Verse;

namespace RRO
{
  internal class CompProperties_RandomOutcomeComp : CompProperties
  {
    public List<string> canProvideTags = new List<string>();

    public CompProperties_RandomOutcomeComp() => this.compClass = typeof (RandomOutcomeComp);
  }
}
