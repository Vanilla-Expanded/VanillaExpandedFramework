
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace VEF.AnimalBehaviours
{
    class CompGasProducer : ThingComp
    {
        private int gasTickMax = 65;
        public bool productionOn = true;

        public CompProperties_GasProducer Props
        {
            get
            {
                return (CompProperties_GasProducer)this.props;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            Pawn pawn = parent as Pawn;
            if (ModsConfig.OdysseyActive && pawn.training.HasLearned(InternalDefOf.VEF_FumeRegulation))
            {
                if (productionOn)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            productionOn = false;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_DisableGasDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_FumeRegulation", true),
                        defaultLabel = "VEF_DisableGas".Translate()
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            productionOn = true;
                        },
                        hotKey = KeyBindingDefOf.Misc2,
                        defaultDesc = "VEF_EnableGasDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Abilities/VEF_FumeRegulation", true),
                        defaultLabel = "VEF_EnableGas".Translate()
                    };
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.productionOn, "productionOn", true, false);
        }

        public override void CompTickInterval(int delta)
        {
            //Since it's a laggy class, allow options to toggle it
            if (AnimalBehaviours_Settings.flagAnimalParticles)
            {

                if (parent.IsHashIntervalTick(gasTickMax, delta))
                {
                    Pawn pawn = this.parent as Pawn;

                    if (productionOn)
                    {

                        if (pawn.Map != null)
                        {
                            if (!Props.generateIfDowned || (Props.generateIfDowned && !pawn.Downed && !pawn.Dead))
                            {
                                CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                                rect = rect.ExpandedBy(Props.radius);

                                foreach (IntVec3 current in rect.Cells)
                                {
                                    if (current.InBounds(pawn.Map) && Rand.Chance(Props.rate))
                                    {
                                        Thing thing = ThingMaker.MakeThing(ThingDef.Named(Props.gasType), null);
                                        thing.Rotation = Rot4.North;
                                        thing.Position = current;
                                        //Directly using SpawnSetup instead of GenSpawn.Spawn to further reduce lag
                                        thing.SpawnSetup(pawn.Map, false);
                                    }
                                }

                            }
                        }
                    }
                    else if (!ModsConfig.OdysseyActive || !pawn.training.HasLearned(InternalDefOf.VEF_FumeRegulation))
                    {
                        productionOn = true;
                    }

                }
            }
        }
    }
}
