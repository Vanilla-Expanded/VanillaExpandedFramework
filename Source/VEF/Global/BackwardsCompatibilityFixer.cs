using System.Linq;
using Verse;

namespace VEF;

internal static class BackwardsCompatibilityFixer
{
    internal static void FixSettingsNameOrNamespace(Mod mod, ModSettings settings, string oldNamespace = null, string oldName = null)
    {
        // Due to change in namespace for VEF setting classes, trying to load them will cause
        // a harmless error. It can be fixed by deleting old settings or saving over them.
        // We check if the specific error message related to this issue pops up, and
        // force save the settings right after to prevent it from appearing in the future.

        var error = Log.Messages.LastOrDefault();
        if (error is { type: LogMessageType.Error, text: not null })
        {
            var type = settings.GetType();
            // Log.Error($"Could not find class {oldNamespace ?? "VFECore"}.{oldName ?? type.Name} while resolving node ModSettings. Trying to use {type.Namespace}.{type.Name} instead. Full node: ");
            if (error.text.StartsWith($"Could not find class {oldNamespace ?? "VFECore"}.{oldName ?? type.Name} while resolving node ModSettings. Trying to use {type.Namespace}.{type.Name} instead. Full node: "))
            {
                Log.Error("Settings related error detected, fixing. Feel free to ignore this and previous error, they should be gone the next time you start the game.");
                LongEventHandler.ExecuteWhenFinished(mod.WriteSettings);
            }
        }
    }
}