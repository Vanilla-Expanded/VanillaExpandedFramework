using System;
using Verse;

namespace VEF.Buildings
{

    [Obsolete($"Use {nameof(GhostGraphicExtension)} instead")]
    public class ShowBlueprintExtension : DefModExtension
    {

        //A simple mod extension accesed by a Harmont patch to show a building's
        //blueprint when it is in ghost (placement) mode

        private static readonly ShowBlueprintExtension DefaultValues = new ShowBlueprintExtension();
        public static ShowBlueprintExtension Get(Def def) => def.GetModExtension<ShowBlueprintExtension>() ?? DefaultValues;

        
        public bool showBlueprintInGhostMode = true;

        public override void ResolveReferences(Def parentDef)
        {
            base.ResolveReferences(parentDef);

            Log.Warning($"{parentDef} ({parentDef.modContentPack?.Name}) is using {nameof(ShowBlueprintExtension)}, which is now obsolete. Please replace it with {nameof(GhostGraphicExtension)}. This DefModExtension will still work for the time being, but may be removed in the future.");

            // Create a new extension that will work the same as this one
            var newExtension = new GhostGraphicExtension { ghostMode = GhostGraphicExtension.CustomGhostMode.Blueprint };
            // Find the index of this extension
            var index = parentDef.modExtensions.IndexOf(this);
            // Replace this extension with GhostGraphicExtension at the same index (don't move extensions around while they are iterated over)
            parentDef.modExtensions[index] = newExtension;
            // Call ResolveReferences on our new DefModExtension (probably unneeded), since it's
            // currently at the same place this extension was at and parent def won't call it.
            newExtension.ResolveReferences(parentDef);
        }
    }

}