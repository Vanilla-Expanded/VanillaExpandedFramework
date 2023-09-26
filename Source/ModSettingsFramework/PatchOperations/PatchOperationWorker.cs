using System.Xml;
using Verse;

namespace ModSettingsFramework
{
    public abstract class PatchOperationWorker : PatchOperationModSettings, IExposable
    {
        public virtual void ApplySettings()
        {
            Init();
        }

        public void Init()
        {
            var container = SettingsContainer;
            if (container.patchWorkers.ContainsKey(this.GetType().FullName) is false)
            {
                container.patchWorkers[this.GetType().FullName] = this;
            }
        }

        public abstract void ExposeData();
        public abstract void CopyFrom(PatchOperationWorker savedWorker);

        public abstract void Reset();

        public void CopyValues()
        {
            if (SettingsContainer.patchWorkers.TryGetValue(this.GetType().FullName, out var workerInstance) && workerInstance != this)
            {
                workerInstance.CopyFrom(this);
            }
        }
        public override bool ApplyWorker(XmlDocument xml)
        {
            var container = SettingsContainer;
            return true;
        }
    }
}
