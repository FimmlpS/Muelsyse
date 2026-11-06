using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Attack;

/// <summary>
/// 几何: throws away every hand card with a non-numeric cost (X-cost or unplayable) and hits once
/// for each of them.
/// </summary>
[Pool(typeof(MuelsyseCardPool))]
public class Geometry : AbstractMuelsyseCard
{
    private const string CountKey = "Count";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar(CountKey).WithMultiplier(MatchingCards)
    ];

    public Geometry() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    private static decimal MatchingCards(CardModel card, Creature? target)
    {
        if (card.Owner?.PlayerCombatState is not { } state)
        {
            return 0;
        }

        return state.Hand.Cards.Count(IsNonNumericCost);
    }

    private static bool IsNonNumericCost(CardModel card)
    {
        return card.EnergyCost.CostsX || card.EnergyCost.GetWithModifiers(CostModifiers.None) < 0;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } state)
        {
            return;
        }

        List<CardModel> cards = state.Hand.Cards.Where(IsNonNumericCost).ToList();
        foreach (CardModel card in cards)
        {
            await CardCmd.Discard(choiceContext, card);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
