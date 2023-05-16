using System.Collections.Generic;
using System.Linq;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Comps;

public class CompVerbsFromInventory : ThingComp, IVerbOwner
{
    private VerbTracker verbTracker;
    public CompVerbsFromInventory() => verbTracker = new VerbTracker(this);
    public CompProperties_VerbsFromInventory Props => props as CompProperties_VerbsFromInventory;
    string IVerbOwner.UniqueVerbOwnerID() => parent.GetUniqueLoadID() + "_" + parent.AllComps.IndexOf(this);

    public bool VerbsStillUsableBy(Pawn p) => p.inventory.Contains(parent);
    public VerbTracker VerbTracker => verbTracker;

    public List<VerbProperties> VerbProperties => parent.def.Verbs;
    public List<Tool> Tools => parent.def.tools;
    public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Weapon;
    public Thing ConstantCaster => null;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref verbTracker, "inventoryVerbTracker", this);
        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
        verbTracker ??= new VerbTracker(this);
        if (parent?.holdingOwner?.Owner is not Pawn_InventoryTracker tracker) return;
        foreach (var verb in verbTracker.AllVerbs)
            verb.caster = tracker.pawn;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (verbTracker.AllVerbs?[0]?.caster != null)
            verbTracker.VerbsTick();
    }

    public void Notify_PickedUp(Pawn pawn)
    {
        foreach (var verb in verbTracker.AllVerbs)
        {
            verb.caster = pawn;
            verb.Notify_PickedUp();
        }
    }

    public void Notify_Dropped()
    {
        foreach (var verb in verbTracker.AllVerbs)
        {
            verb.Notify_EquipmentLost();
            verb.caster = null;
            verb.state = VerbState.Idle;
        }
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        foreach (var gizmo in base.CompGetWornGizmosExtra())
            yield return gizmo;
        foreach (var gizmo in from verb in verbTracker.AllVerbs
                 from gizmo in verb.GetGizmosForVerb(verb.Managed())
                 select gizmo) yield return gizmo;
    }

    public AdditionalVerbProps PropsFor(Verb verb) => Props.PropsFor(verb);
}

public class CompProperties_VerbsFromInventory : CompProperties_VerbProps
{
    public CompProperties_VerbsFromInventory() => compClass = typeof(CompVerbsFromInventory);

    public override void PostLoadSpecial(ThingDef parent)
    {
        base.PostLoadSpecial(parent);
        MVCF.EnabledFeatures.Add("InventoryVerbs");
    }
}
