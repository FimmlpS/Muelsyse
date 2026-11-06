using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Cards.Attack;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// 漫步于黄金之梦's shuffle rule, mirroring S1's <c>CardGroup.shuffle</c> patch: after a player's draw
/// pile is shuffled, every copy of the card that ended up in that draw pile slides into the discard
/// pile and gains damage for the rest of the combat.
///
/// <para>The rule lives on a combat singleton instead of on the card so that it is driven by the
/// shuffle itself (the pile is scanned after the shuffle) rather than by the card's own copy of the
/// hook: the card may be in hand, in the discard pile, in play, or have just been created this
/// combat, and the shuffle must still catch it.</para>
/// </summary>
public class GoldenWanderListener : CustomSingletonModel
{
    public GoldenWanderListener() : base(HookType.Combat)
    {
    }

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        IReadOnlyList<CardModel> drawPile = PileType.Draw.GetPile(shuffler).Cards;

        // Snapshot first: moving a card out of the pile mutates the live collection.
        foreach (GoldenWander card in drawPile.OfType<GoldenWander>().ToList())
        {
            if (card.Owner != shuffler || card.Pile?.Type != PileType.Draw)
            {
                continue;
            }

            // Grow before the move: the pile change awaits visuals, and the damage must not be
            // lost if that wait is cut short by the combat ending.
            card.GrowOnShuffle();
            await CardPileCmd.Add(card, PileType.Discard, CardPilePosition.Random);
        }
    }
}
