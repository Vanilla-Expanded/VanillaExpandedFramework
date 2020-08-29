using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class PawnKindDefExtension : DefModExtension
    {

        private static readonly PawnKindDefExtension DefaultValues = new PawnKindDefExtension();
        public static PawnKindDefExtension Get(Def def) => def.GetModExtension<PawnKindDefExtension>() ?? DefaultValues;

        public bool countAsSlave;
        public List<BodyPartGroupDef> factionColourApparelPartList;
        public List<ApparelLayerDef> factionColourApparelLayerList;
        public List<string> shieldTags;
        public FloatRange shieldMoney;

        public List<Pair<BodyPartGroupDef, ApparelLayerDef>> FactionColourApparelWithPartAndLayersList
        {
            get
            {
                if (_factionColourApparelWithPartAndLayersList == null)
                {
                    _factionColourApparelWithPartAndLayersList = new List<Pair<BodyPartGroupDef, ApparelLayerDef>>();
                    if (factionColourApparelPartList != null && factionColourApparelLayerList != null)
                        for (int i = 0; i < factionColourApparelPartList.Count; i++)
                            _factionColourApparelWithPartAndLayersList.Add(new Pair<BodyPartGroupDef, ApparelLayerDef>(factionColourApparelPartList[i], factionColourApparelLayerList[i]));
                }
                return _factionColourApparelWithPartAndLayersList;
            }
        }

        [Unsaved]
        private List<Pair<BodyPartGroupDef, ApparelLayerDef>> _factionColourApparelWithPartAndLayersList;

        public override IEnumerable<string> ConfigErrors()
        {
            if (factionColourApparelPartList != null && factionColourApparelLayerList != null && factionColourApparelPartList.Count != factionColourApparelLayerList.Count)
                yield return "factionColourApparelPartList and factionColourApparelLayerList must be of the same length.";
        }

    }

}
