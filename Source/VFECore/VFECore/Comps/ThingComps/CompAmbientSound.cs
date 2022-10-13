using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore
{
    public class CompProperties_AmbientSound : CompProperties
    {
        public SoundDef ambientSound;
        public CompProperties_AmbientSound()
        {
            compClass = typeof(CompAmbientSound);
        }
    }

    public class CompAmbientSound : ThingComp
    {
        private Sustainer sustainerAmbient;
        public CompProperties_AmbientSound Props => base.props as CompProperties_AmbientSound;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent is Pawn)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    StartSustainer();
                });
            }
            else
            {
                CompPowerTrader compPower = parent.TryGetComp<CompPowerTrader>();
                if ((compPower == null || compPower.PowerOn) && FlickUtility.WantsToBeOn(parent))
                {
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        StartSustainer();
                    });
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            EndSustainer();
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            switch (signal)
            {
                case CompAutoPowered.AutoPoweredWantsOffSignal:
                case CompPowerTrader.PowerTurnedOffSignal:
                case CompSchedule.ScheduledOffSignal:
                case CompFlickable.FlickedOffSignal:
                    EndSustainer();
                    break;
                case CompPowerTrader.PowerTurnedOnSignal:
                case CompSchedule.ScheduledOnSignal:
                case CompFlickable.FlickedOnSignal:
                    StartSustainer();
                    break;
                default:
                    break;
            }
        }

        protected void StartSustainer()
        {
            if (CanStartSustainer() is false)
            {
                return;
            }

            if (sustainerAmbient != null)
            {
                return;
            }

            SoundInfo info = SoundInfo.InMap(parent);
            if (parent is Pawn pawn)
            {
                pawn.pather ??= new Pawn_PathFollower(pawn);
                pawn.stances ??= new Pawn_StanceTracker(pawn);
            }
            sustainerAmbient = Props.ambientSound.TrySpawnSustainer(info);

        }

        protected void EndSustainer()
        {
            if (sustainerAmbient != null)
            {
                sustainerAmbient.End();
                sustainerAmbient = null;
            }
        }

        protected virtual bool CanStartSustainer()
        {
            return true;
        }
    }
}

