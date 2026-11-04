using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>夜愿: waiting in the draw pile, it grants a wish of extra draws each time it arrives there.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class NightWish : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Draw", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public NightWish() : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (card != this || Pile?.Type != PileType.Draw || oldPileType == PileType.Draw
            || CombatState == null || Owner.PlayerCombatState == null)
        {
            return;
        }

        await DrawWish();
    }

    /// <summary>
    /// The draw pile is also filled at combat start, but the engine does that by moving the freshly
    /// cloned deck cards in directly (<c>Player.PopulateCombatState</c>), which never dispatches
    /// <see cref="AfterCardChangedPiles"/>. This hook runs right after that population, while the
    /// card still sits in the draw pile, so the wish is granted for the opening hand too.
    /// </summary>
    public override async Task BeforeCombatStart()
    {
        if (Pile?.Type != PileType.Draw || CombatState == null || Owner.PlayerCombatState == null)
        {
            return;
        }

        await DrawWish();
    }

    private async Task DrawWish()
    {
        await CardPileCmd.Draw(new BlockingPlayerChoiceContext(),
            DynamicVars["Draw"].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Unplayable);
    }
}
