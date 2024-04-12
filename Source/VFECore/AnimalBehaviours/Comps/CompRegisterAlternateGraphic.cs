using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace AnimalBehaviours
{
    public class CompRegisterAlternateGraphic : ThingComp
    {
        /* TODO: Update to 1.5

      public Faction faction;
      public Faction oldFaction;


      public CompProperties_RegisterAlternateGraphic Props
      {
          get
          {
              return (CompProperties_RegisterAlternateGraphic)this.props;
          }
      }

      public override void PostExposeData()
      {
          base.PostExposeData();
          Scribe_References.Look<Faction>(ref this.faction, "faction", false);
          Scribe_References.Look<Faction>(ref this.oldFaction, "oldFaction", false);

      }

      public override void PostSpawnSetup(bool respawningAfterLoad)
      {
          base.PostSpawnSetup(respawningAfterLoad);
          Faction oldFaction = this.parent.Faction;


      }

      public override void CompTick()
      {
          base.CompTick();
          if (this.parent.IsHashIntervalTick(1000)){
              Faction faction = this.parent.Faction;
              if (faction != oldFaction)
              {
                  Pawn pawn = this.parent as Pawn;
                  string graphicPath = pawn.Drawer.renderer.graphics.nakedGraphic.path;
                  if (faction == Faction.OfPlayer)
                  {                   
                      AnimalCollectionClass.AddGraphicPathToList(pawn,graphicPath);
                  }
                  else {

                      AnimalCollectionClass.RemoveGraphicPathFromList(pawn,graphicPath);
                  }
                  oldFaction = faction;

              }



          }
      }


      public override void PostDeSpawn(Map map)
      {


          Pawn pawn = this.parent as Pawn;
          string graphicPath = pawn?.Drawer?.renderer?.graphics?.nakedGraphic?.path;
          AnimalCollectionClass.RemoveGraphicPathFromList(pawn,graphicPath);
      }

       public override void PostDestroy(DestroyMode mode, Map previousMap)
       {


           Pawn pawn = this.parent as Pawn;
          string graphicPath = pawn?.Drawer?.renderer?.graphics?.nakedGraphic?.path;
          AnimalCollectionClass.RemoveGraphicPathFromList(pawn,graphicPath);
       }
  */
    }
}

