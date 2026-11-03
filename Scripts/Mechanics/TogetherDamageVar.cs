using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// Displays the halved value of a 协同 card outside of combat, where global hooks are not run.
/// In combat the halving is handled by <see cref="TogetherListener" /> instead.
/// </summary>
public class TogetherDamageVar : DamageVar
{
    public TogetherDamageVar(decimal damage, ValueProp props) : base(damage, props)
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
