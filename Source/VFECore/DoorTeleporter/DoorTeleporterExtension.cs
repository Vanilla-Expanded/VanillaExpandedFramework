using Verse;

namespace VFECore
{
    public class DoorTeleporterExtension : DefModExtension
    {
        public string doorTeleporterTexPath;
        public string doorTeleporterMaskPath;
        public float maskThreshold;
        public string doorTeleporterBackgroundPath;
        public string mainMatPath;
        public string distortionMatPath;
        public string distortionMaskPath;
        public string destroyIconPath;
        public string renameIconPath;

        public string destroyLabelKey;
        public string destroyDescKey;
        public string renameLabelKey;
        public string renameDescKey;

        public SoundDef sustainer;
    }
}
