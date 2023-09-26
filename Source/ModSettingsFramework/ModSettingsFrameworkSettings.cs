using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ModSettingsFramework
{
    public class ModSettingsFrameworkSettings : ModSettings
    {
        public static Dictionary<string, ModSettingsContainer> modSettingsPerModId = new Dictionary<string, ModSettingsContainer>();
        public override void ExposeData()
        {
            base.ExposeData();
            Log_Error_Patch.suppressErrorMessages = true;
            Scribe_Collections.Look(ref modSettingsPerModId, "modSettingsPerModId", LookMode.Value, LookMode.Deep); 
            Log_Error_Patch.suppressErrorMessages = false;
        }

        public static ModSettingsContainer GetModSettingsContainer(string packageID)
        {
            var mod = LoadedModManager.RunningMods.FirstOrDefault(x => x.PackageIdPlayerFacing.ToLower() == packageID.ToLower());
            if (mod != null)
            {
                return GetModSettingsContainer(mod);
            }
            return null;
        }

        public static ModSettingsContainer GetModSettingsContainer(ModContentPack modHandle)
        {
            if (!modSettingsPerModId.TryGetValue(modHandle.PackageIdPlayerFacing.ToLower(), out var container))
            {
                modSettingsPerModId[modHandle.PackageIdPlayerFacing.ToLower()] = container = new ModSettingsContainer
                { 
                    packageID = modHandle.PackageIdPlayerFacing.ToLower(),
                }; 
            }

            if (container.modHandle is null)
            {
                ModSettingsFrameworkMod.fakeInit = true;
                container.modHandle = new ModSettingsFrameworkMod(modHandle)
                {
                    modPackOverride = modHandle,
                    fakeMod = true,
                };
                ModSettingsFrameworkMod.fakeInit = false;
            }
            return container;
        }
	}
}
