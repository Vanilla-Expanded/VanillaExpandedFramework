using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using VEF;

namespace VEF.AnimalBehaviours
{
    public class CompDigWhenHungry : ThingComp
    {
        public int stopdiggingcounter = 0;
        private Effecter effecter;
        public bool diggingOn = true;

        public CompProperties_DigWhenHungry Props
        {
            get
            {
                return (CompProperties_DigWhenHungry)this.props;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            Pawn pawn = parent as Pawn;
            if (pawn?.training?.HasLearned(InternalDefOf.VEF_DiggingDiscipline) == true)
            {
                if (diggingOn)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            diggingOn = false;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_DisableDiggingDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_DiggingDiscipline", true),
                        defaultLabel = "VEF_DisableDigging".Translate()
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            diggingOn = true;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_EnableDiggingDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_DiggingDiscipline", true),
                        defaultLabel = "VEF_EnableDigging".Translate()
                    };
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.diggingOn, "diggingOn", true, false);
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = this.parent as Pawn;
            if (AnimalBehaviours_Settings.flagDigWhenHungry && (pawn.Map != null) && (pawn.Awake()) && (!Props.digOnlyOnGrowingSeason ||
                (Props.digOnlyOnGrowingSeason && (pawn.Map.mapTemperature.OutdoorTemp > Props.minTemperature && pawn.Map.mapTemperature.OutdoorTemp < Props.maxTemperature))) &&
                ((pawn.needs?.food?.CurLevelPercentage < pawn.needs?.food?.PercentageThreshHungry) ||
                (Props.digAnywayEveryXTicks && this.parent.IsHashIntervalTick(Props.timeToDigForced))))
            {
                if (diggingOn)
                {
                    if (pawn.Position.GetTerrain(pawn.Map).affordances.Contains(VEFDefOf.Diggable))
                    {
                        if (stopdiggingcounter <= 0)
                        {
                            if (Props.acceptedTerrains != null)
                            {
                                if (Props.acceptedTerrains.Contains(pawn.Position.GetTerrain(pawn.Map).defName))
                                {
                                    Thing newcorpse;
                                    if (Props.isFrostmite)
                                    {
                                        PawnKindDef wildman = PawnKindDef.Named("WildMan");
                                        Faction faction = FactionUtility.DefaultFactionFrom(wildman.defaultFactionDef);
                                        Pawn newPawn = PawnGenerator.GeneratePawn(wildman, faction);
                                        newcorpse = GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map, WipeMode.Vanish);
                                        newcorpse.Kill(null, null);

                                    }
                                    else
                                    {
                                        if (Props.customThingsToDig != null)
                                        {
                                            string thingToDig = this.Props.customThingsToDig.RandomElement();
                                            int index = Props.customThingsToDig.IndexOf(thingToDig);
                                            int amount;
                                            if (Props.customAmountsToDig != null)
                                            {
                                                amount = Props.customAmountsToDig[index];

                                            }
                                            else
                                            {
                                                amount = Props.customAmountToDig;
                                            }
                                            ThingDef newThing = ThingDef.Named(thingToDig);
                                            newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                            newcorpse.stackCount = amount;
                                        }
                                        else
                                        {
                                            ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                                            newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                            newcorpse.stackCount = this.Props.customAmountToDig;
                                        }
                                    }
                                    if (Props.spawnForbidden)
                                    {
                                        newcorpse.SetForbidden(true);
                                    }

                                    if (this.effecter == null)
                                    {
                                        this.effecter = EffecterDefOf.Mine.Spawn();
                                    }
                                    this.effecter.Trigger(pawn, newcorpse);

                                }
                            }
                            else
                            {
                                Thing newcorpse;
                                if (Props.isFrostmite)
                                {
                                    PawnKindDef wildman = PawnKindDef.Named("WildMan");
                                    Faction faction = FactionUtility.DefaultFactionFrom(wildman.defaultFactionDef);
                                    Pawn newPawn = PawnGenerator.GeneratePawn(wildman, faction);
                                    newcorpse = GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map, WipeMode.Vanish);
                                    newcorpse.Kill(null, null);

                                }
                                else
                                {
                                    if (Props.customThingsToDig != null)
                                    {
                                        string thingToDig = this.Props.customThingsToDig.RandomElement();
                                        int index = Props.customThingsToDig.IndexOf(thingToDig);
                                        int amount;
                                        if (Props.customAmountsToDig != null)
                                        {
                                            amount = Props.customAmountsToDig[index];

                                        }
                                        else
                                        {
                                            amount = Props.customAmountToDig;
                                        }
                                        ThingDef newThing = ThingDef.Named(thingToDig);
                                        newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                        newcorpse.stackCount = amount;
                                    }
                                    else
                                    {
                                        ThingDef newThing = ThingDef.Named(this.Props.customThingToDig);
                                        newcorpse = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                                        newcorpse.stackCount = this.Props.customAmountToDig;
                                    }
                                }
                                if (Props.spawnForbidden)
                                {
                                    newcorpse.SetForbidden(true);
                                }
                                if (this.effecter == null)
                                {
                                    this.effecter = EffecterDefOf.Mine.Spawn();
                                }
                                this.effecter.Trigger(pawn, newcorpse);
                            }





                            stopdiggingcounter = Props.timeToDig;
                        }
                        stopdiggingcounter--;
                    }

                }
                else if (pawn?.training?.HasLearned(InternalDefOf.VEF_DiggingDiscipline) != true)
                {
                    diggingOn = true;
                }
            }
        }
    }
}

