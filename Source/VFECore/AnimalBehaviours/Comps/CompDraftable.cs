
using Verse;

namespace AnimalBehaviours
{
    class CompDraftable : ThingComp
    {
        /*
        Reference for the abilities array (only for Genetic Rim abilities):
        GetRage -> 0 
        GetExplodable -> 1 
        GetChickenRimPox -> 2 
        GetCanCarryMore -> 3
        GetAdrenalineBurst -> 4
        GetCanDoInsectClouds -> 5
        GetCanStampede -> 6
        GetCanDoPoisonousCloud -> 7
        GetCanBurrow -> 8
        HasDinoStamina -> 9
        GetHorror -> 10
        GetMechablast -> 11
        GetKeenSenses -> 12
        GetCatReflexes -> 13
        GetOrbitalStrike -> 14
        */

        public CompProperties_Draftable Props
        {
            get
            {
                return (CompProperties_Draftable)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddDraftableAnimalToList(this.parent, new bool[] { GetRage, GetExplodable, GetChickenRimPox, GetCanCarryMore,
                GetAdrenalineBurst, GetCanDoInsectClouds,GetCanStampede,GetCanDoPoisonousCloud,GetCanBurrow,HasDinoStamina,
            GetHorror,GetMechablast,GetKeenSenses,GetCatReflexes,GetOrbitalStrike});

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveDraftableAnimalFromList(this.parent);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveDraftableAnimalFromList(this.parent);
        }

        public bool GetExplodable
        {
            get
            {
                return this.Props.explodable;
            }
        }

        public bool GetRage
        {
            get
            {
                return this.Props.rage;
            }
        }

        public bool GetChickenRimPox
        {
            get
            {
                return this.Props.chickenRimPox;
            }
        }

        public bool GetCanCarryMore
        {
            get
            {
                return this.Props.carrymore;
            }
        }

        public bool GetAdrenalineBurst
        {
            get
            {
                return this.Props.adrenalineburst;
            }
        }

        public bool GetCanDoInsectClouds
        {
            get
            {
                return this.Props.insectclouds;
            }
        }
        public bool GetCanStampede
        {
            get
            {
                return this.Props.stampede;
            }
        }

        public bool GetCanDoPoisonousCloud
        {
            get
            {
                return this.Props.poisonouscloud;
            }
        }
        public bool GetCanBurrow
        {
            get
            {
                return this.Props.burrowing;
            }
        }
        public bool HasDinoStamina
        {
            get
            {
                return this.Props.dinostamina;
            }
        }
        public bool GetHorror
        {
            get
            {
                return this.Props.horror;
            }
        }

        public bool GetMechablast
        {
            get
            {
                return this.Props.mechablast;
            }
        }
        public bool GetKeenSenses
        {
            get
            {
                return this.Props.keensenses;
            }
        }
        public bool GetCatReflexes
        {
            get
            {
                return this.Props.catreflexes;
            }
        }

        public bool GetOrbitalStrike
        {
            get
            {
                return this.Props.orbitalstrike;
            }
        }
    }
}
