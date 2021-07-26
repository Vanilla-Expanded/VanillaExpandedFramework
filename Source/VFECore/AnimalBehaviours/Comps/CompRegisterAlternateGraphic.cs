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
        public PawnRenderer pawn_renderer;
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
                    if (faction == Faction.OfPlayer)
                    {
                        Pawn pawn = this.parent as Pawn;
                        Pawn_DrawTracker drawtracker = ((Pawn_DrawTracker)typeof(Pawn).GetField("drawer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pawn));
                        if (drawtracker != null)
                        {
                            this.pawn_renderer = drawtracker.renderer;
                        }

                        string graphicPath = pawn_renderer.graphics.nakedGraphic.path;

                        AnimalCollectionClass.AddGraphicPathToList(graphicPath);
                    }
                    else {
                        string graphicPath = pawn_renderer.graphics.nakedGraphic.path;
                        AnimalCollectionClass.RemoveGraphicPathFromList(graphicPath);
                    }
                    oldFaction = faction;

                }

                Log.Message(AnimalCollectionClass.salamander_graphics.ToStringSafeEnumerable());


            }
        }


        public override void PostDeSpawn(Map map)
        {

            Log.Message("Despawning");
            string graphicPath = pawn_renderer.graphics.nakedGraphic.path;
            AnimalCollectionClass.RemoveGraphicPathFromList(graphicPath);
        }

       /* public override void PostDestroy(DestroyMode mode, Map previousMap)
        {

            Log.Message("Destroying");
            string graphicPath = pawn_renderer.graphics.nakedGraphic.path;
            AnimalCollectionClass.RemoveGraphicPathFromList(graphicPath);
        }*/
    }
}

