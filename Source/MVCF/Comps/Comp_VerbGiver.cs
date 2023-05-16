using System.Collections.Generic;
using System.Linq;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Comps;

public class Comp_VerbGiver : ThingComp, IVerbOwner
{
    private VerbTracker verbTracker;

    public Comp_VerbGiver() => verbTracker = new VerbTracker(this);

    public CompProperties_VerbGiver Props => (CompProperties_VerbGiver)props;

    public VerbTracker VerbTracker => verbTracker;

    public List<VerbProperties> VerbProperties => parent.def.Verbs;

    public List<Tool> Tools => parent.def.tools;

    Thing IVerbOwner.ConstantCaster => null;

    ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Weapon;

    string IVerbOwner.UniqueVerbOwnerID() => parent.GetUniqueLoadID() + "_" + parent.AllComps.IndexOf(this);

    bool IVerbOwner.VerbsStillUsableBy(Pawn p) => p.apparel.Contains(parent);

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
        verbTracker ??= new VerbTracker(this);
        if (parent?.holdingOwner?.Owner is not Pawn_ApparelTracker tracker) return;
        foreach (var verb in verbTracker.AllVerbs) verb.caster = tracker.pawn;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (verbTracker.AllVerbs?[0]?.caster != null)
            verbTracker.VerbsTick();
    }

    public void Notify_Worn(Pawn pawn)
    {
        foreach (var verb in verbTracker.AllVerbs)
        {
            verb.caster = pawn;
            verb.Notify_PickedUp();
        }
    }

    public void Notify_Unworn()
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
