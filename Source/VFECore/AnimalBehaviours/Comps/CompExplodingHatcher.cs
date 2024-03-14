using System;
using RimWorld.Planet;
using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompExplodingHatcher : ThingComp
    {
        public CompProperties_ExplodingHatcher Props
        {
            get
            {
                return (CompProperties_ExplodingHatcher)this.props;
            }
        }

        private CompTemperatureRuinable FreezerComp
        {
            get
            {
                return this.parent.GetComp<CompTemperatureRuinable>();
            }
        }

        public bool TemperatureDamaged
        {
            get
            {
                return this.FreezerComp != null && this.FreezerComp.Ruined;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.gestateProgress, "gestateProgress", 0f, false);
            Scribe_References.Look<Pawn>(ref this.hatcheeParent, "hatcheeParent", false);
            Scribe_References.Look<Pawn>(ref this.otherParent, "otherParent", false);
            Scribe_References.Look<Faction>(ref this.hatcheeFaction, "hatcheeFaction", false);
        }

        public override void CompTick()
        {
            if (!this.TemperatureDamaged)
            {
                float num = 1f / (this.Props.hatcherDaystoHatch * 60000f);
                this.gestateProgress += num;
                if (this.gestateProgress > 1f)
                {
                    this.Hatch();
                }
            }
        }

        public void Hatch()
        {
            try
            {

                PawnGenerationRequest request = new PawnGenerationRequest(this.Props.hatcherPawn, this.hatcheeFaction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, true, 1f, false, false, true, true, true, false, false, false, false, 0f, 0f,null, 1f, null, null, null, null, null, null, null, null, null, null, null, null);
                for (int i = 0; i < this.parent.stackCount; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, this.parent))
                    {
                        if (pawn != null)
                        {
                          
                            if (this.hatcheeParent != null)
                            {
                                if (pawn.playerSettings != null && this.hatcheeParent.playerSettings != null && this.hatcheeParent.Faction == this.hatcheeFaction)
                                {
                                    pawn.playerSettings.AreaRestrictionInPawnCurrentMap = this.hatcheeParent.playerSettings.AreaRestrictionInPawnCurrentMap;
                                }
                                if (pawn.RaceProps.IsFlesh)
                                {
                                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, this.hatcheeParent);
                                }
                            }
                            if (this.otherParent != null && (this.hatcheeParent == null || this.hatcheeParent.gender != this.otherParent.gender) && pawn.RaceProps.IsFlesh)
                            {
                                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, this.otherParent);
                            }
                            if (parent.Map != null)
                            {
                                List<Thing> ignoredThings = new List<Thing>();
                                IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;

                                for (int k = 0; k < allPawnsSpawned.Count; k++)
                                {
                                    if (allPawnsSpawned[k] != null && allPawnsSpawned[k].def.defName == pawn.def.defName)
                                    {
                                        ignoredThings.Add(allPawnsSpawned[k]);
                                    }
                                }
                                Thing pawnThing = pawn as Thing;
                                ignoredThings.Add(pawnThing);
                             
                                if (AnimalBehaviours_Settings.flagExplodingAnimalEggs) {
                                    GenExplosion.DoExplosion(parent.Position, parent.Map, Props.range, DefDatabase<DamageDef>.GetNamed(Props.damageDef), parent, Props.damage, -1, SoundDef.Named(Props.soundDef), null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0, false, null, ignoredThings);
                                }

                            }
                        }
                        if (this.parent.Spawned)
                        {
                            FilthMaker.TryMakeFilth(this.parent.Position, this.parent.Map, ThingDefOf.Filth_AmnioticFluid, 1, FilthSourceFlags.None);
                        }
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    }
                }
            }
            finally
            {

                this.parent.Destroy(DestroyMode.Vanish);
            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(this.parent.stackCount + count);
            float b = ((ThingWithComps)otherStack).GetComp<CompExplodingHatcher>().gestateProgress;
            this.gestateProgress = Mathf.Lerp(this.gestateProgress, b, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            CompExplodingHatcher comp = ((ThingWithComps)piece).GetComp<CompExplodingHatcher>();
            comp.gestateProgress = this.gestateProgress;
            comp.hatcheeParent = this.hatcheeParent;
            comp.otherParent = this.otherParent;
            comp.hatcheeFaction = this.hatcheeFaction;
        }

        public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            base.PrePreTraded(action, playerNegotiator, trader);
            if (action == TradeAction.PlayerBuys)
            {
                this.hatcheeFaction = Faction.OfPlayer;
                return;
            }
            if (action == TradeAction.PlayerSells)
            {
                this.hatcheeFaction = trader.Faction;
            }
        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
        {
            base.PostPostGeneratedForTrader(trader, forTile, forFaction);
            this.hatcheeFaction = forFaction;
        }

        public override string CompInspectStringExtra()
        {
            if (!this.TemperatureDamaged)
            {
                if (AnimalBehaviours_Settings.flagExplodingAnimalEggs)
                {
                    return "EggProgress".Translate() + ": " + this.gestateProgress.ToStringPercent() + "\n" + "VEF_WarningEggExplodes".Translate();
                }
                else
                {
                    return "EggProgress".Translate() + ": " + this.gestateProgress.ToStringPercent();
                }
            }
            return null;
        }

        private float gestateProgress;

        public Pawn hatcheeParent;

        public Pawn otherParent;

        public Faction hatcheeFaction;
    }
}
