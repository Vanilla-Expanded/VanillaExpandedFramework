using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ModSettingsFramework
{
    public abstract class PatchOperationModSettings : PatchOperation
    {
        public int order;
        public string category;
        public List<string> mods;
        public string id;
        public string label;
        public string tooltip;
        public string modPackageSettingsID;
        public ModSettingsContainer SettingsContainer
        {
            get
            {
                if (modPackageSettingsID.NullOrEmpty() is false)
                {
                    var modHandle = LoadedModManager.RunningMods.FirstOrDefault(x => x.PackageIdPlayerFacing.ToLower() 
                    == modPackageSettingsID.ToLower());
                    if (modHandle != null)
                    {
                        return ModSettingsFrameworkSettings.GetModSettingsContainer(modHandle);
                    }
                }
                foreach (var runningMod in LoadedModManager.RunningMods)
                {
                    if (runningMod.Patches.Contains(this))
                    {
                        var container = ModSettingsFrameworkSettings.GetModSettingsContainer(runningMod);
                        return container;
                    }
                }
                return null;
            }
        }
        public abstract void DoSettings(ModSettingsContainer container, Listing_Standard list);
        public abstract int SettingsHeight();
        public bool CanRun()
        {
            if (mods != null)
            {
                for (int i = 0; i < mods.Count; i++)
                {
                    if (ModLister.HasActiveModWithName(mods[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }
    }
}
