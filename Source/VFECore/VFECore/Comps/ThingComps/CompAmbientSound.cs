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
        protected Sustainer sustainerAmbient;
        protected bool isPawn;
        // Fields only used if the parent is not a pawn
        protected CompPowerTrader powerTrader;
        protected CompSchedule schedule;
        protected CompFlickable flickable;
        protected CompRefuelable refuelable;

        public CompProperties_AmbientSound Props => base.props as CompProperties_AmbientSound;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            isPawn = parent is Pawn;
            if (!isPawn)
            {
                powerTrader = parent.GetComp<CompPowerTrader>();
                schedule = parent.GetComp<CompSchedule>();
                flickable = parent.GetComp<CompFlickable>();
                refuelable = parent.GetComp<CompRefuelable>();
            }

            LongEventHandler.ExecuteWhenFinished(StartSustainer);
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
                case CompPowerTrader.PowerTurnedOffSignal:
                case CompSchedule.ScheduledOffSignal:
                case CompFlickable.FlickedOffSignal:
                case CompRefuelable.RanOutOfFuelSignal:
                    EndSustainer();
                    break;
                case CompPowerTrader.PowerTurnedOnSignal:
                case CompSchedule.ScheduledOnSignal:
                case CompFlickable.FlickedOnSignal:
                case CompRefuelable.RefueledSignal:
                    StartSustainer();
                    break;
            }
        }

        protected void StartSustainer()
        {
            if (CanStartSustainer() is false)
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
            // The sustainer is not null and hasn't ended, no point in starting the sustainer again.
            if (sustainerAmbient is { Ended: false })
                return false;

            // If parent is a pawn, always restart sustainer.
            if (isPawn)
            {
                StartSustainer();
                return true;
            }

            // If not a pawn, check if the sustainer can start.
            // Return early if null or not allowed to be on from one of the comps.
            if (powerTrader is { PowerOn: false })
                return false;
            if (schedule is { Allowed: false })
                return false;
            if (flickable is { SwitchIsOn: false })
                return false;
            if (refuelable is { HasFuel: false })
                return false;

            return true;
        }
    }
}

