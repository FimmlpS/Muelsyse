using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>深层反映: teaches the costliest card in hand to refund energy, then grows costlier.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class DeepReflect : AbstractMuelsyseCard
{
    public DeepReflect() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is { } combatState)
        {
            CardModel? best = null;
            int bestCost = int.MinValue;

            foreach (CardModel card in combatState.Hand.Cards)
            {
                int cost = card.EnergyCost.GetWithModifiers(CostModifiers.None);
                if (best == null || cost > bestCost)
                {
                    best = card;
                    bestCost = cost;
                }
            }

            if (best != null)
            {
                CultivateCmd.AddBonus(best, CultivateAim.Energy, 1);
            }
        }

        IncreaseOwnCost(1);
        return Task.CompletedTask;
    }

    private void IncreaseOwnCost(int amount)
    {
        IncreaseCost(this, amount);

        if (DeckVersion is { } deckVersion && deckVersion != this)
        {
            IncreaseCost(deckVersion, amount);
        }
    }

    private static void IncreaseCost(CardModel card, int amount)
    {
        card.EnergyCost.SetCustomBaseCost(
            card.EnergyCost.GetWithModifiers(CostModifiers.None) + amount);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
