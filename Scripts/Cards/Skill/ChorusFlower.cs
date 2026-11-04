using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Muelsyse.Scripts.Mechanics.Cultivate;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>紫颂: replays copies of the draw pile, one verse for each point of cultivation.</summary>
[Pool(typeof(TokenCardPool))]
public class ChorusFlower : AbstractMuelsyseCard
{
    private const string RepeatKey = "CalculatedRepeat";

    /// <summary>
    /// One repeat, plus one for every point of 攻击 cultivation. The var is calculated (rather than a
    /// fixed number) so the number printed on the card face follows the cultivation modifier in real
    /// time; <see cref="CalculatedVar.UpdateCardPreview"/> refreshes it on every card refresh, and
    /// <see cref="OnPlay"/> reads the exact same calculation.
    /// <para>
    /// The plain "Damage" var is not shown any more, but it has to stay: cultivation uses it to tell
    /// that this card has an attack value (<c>CultivateBonus.TryGetAttackBase</c>), and it is what
    /// makes 攻击 cultivation grow this card's attack at all.
    /// </para>
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Damage", 1),
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar(RepeatKey).WithMultiplier(CultivateBonus)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    public ChorusFlower() : base(2, CardType.Skill, CardRarity.Token, TargetType.None)
    {
    }

    /// <summary>
    /// The extra repeat count. Must be static: <see cref="CalculatedVar.WithMultiplier"/> rejects a
    /// calculator that is bound to a model instance.
    /// </summary>
    private static decimal CultivateBonus(CardModel card, Creature? target)
    {
        return card.GetModifier<CultivateModifier>()?.Attack ?? 0;
    }

    private int Repeats => (int)((CalculatedVar)DynamicVars[RepeatKey]).Calculate(null);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState == null)
        {
            return;
        }

        int repeats = Repeats;

        for (int i = 0; i < repeats; i++)
        {
            List<CardModel> candidates = PileType.Draw.GetPile(Owner).Cards
                .Where(card => card is not ChorusFlower)
                .ToList();

            CardModel? source = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
            if (source == null)
            {
                return;
            }

            CardModel copy = source.CreateClone();
            await CardCmd.AutoPlay(choiceContext, copy, null);

            // The copy is a performance, not a card: destroy it instead of letting it settle in the
            // discard (or exhaust) pile, where it would otherwise linger for the rest of the fight.
            await Purge(copy);
        }
    }

    /// <summary>
    /// Removes a played copy from combat. Power cards (and copies that remove themselves) are no
    /// longer in any pile by the time the auto-play returns, so only combat-pile cards are purged.
    /// </summary>
    private static async Task Purge(CardModel card)
    {
        if (card.Pile is { IsCombatPile: true })
        {
            await CardPileCmd.RemoveFromCombat(card, skipVisuals: true);
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
