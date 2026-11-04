using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// <see cref="TogetherDamageVar" /> for cards whose damage is calculated from combat state.
/// </summary>
public class TogetherCalculatedDamageVar : CalculatedDamageVar
{
    public TogetherCalculatedDamageVar(ValueProp props) : base(props)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target,
        bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
        if (!runGlobalHooks && card.Keywords.Contains(MuelsyseKeywords.Together))
        {
            PreviewValue -= Math.Floor(PreviewValue / 2m);
        }
    }
}
