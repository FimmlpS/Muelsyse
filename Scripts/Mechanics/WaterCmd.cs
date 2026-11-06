using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Cards.Status;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>Shared helpers for creating and finding 水 cards.</summary>
public static class WaterCmd
{
    public static Water Create(Player player, bool upgraded = false)
    {
        if (player.Creature.CombatState is not { } combatState)
        {
            throw new InvalidOperationException("Water can only be created in combat.");
        }

        Water water = combatState.CreateCard<Water>(player);
        if (upgraded)
        {
            CardCmd.Upgrade(water);
        }

        return water;
    }

    public static async Task AddToDiscard(PlayerChoiceContext choiceContext, Player player, bool upgraded = false)
    {
        if (player.Creature.CombatState == null)
        {
            return;
        }

        await CardPileCmd.AddGeneratedCardToCombat(
            Create(player, upgraded), PileType.Discard, player, CardPilePosition.Random);
    }

    /// <summary>
    /// Puts a 水 card into the hand, unless the hand is already at its limit: a full hand must never
    /// end up holding more cards than the game allows, so the water goes to the discard pile instead
    /// (where it still splashes and can be drawn again later). Checking here rather than letting the
    /// engine silently reroute the card keeps every caller — 生命之源 included — honest about it.
    /// </summary>
    public static async Task AddToHand(PlayerChoiceContext choiceContext, Player player, bool upgraded = false,
        CardPilePosition position = CardPilePosition.Random)
    {
        if (player.Creature.CombatState == null)
        {
            return;
        }

        if (HandLimit.IsFull(player))
        {
            // Same feedback the engine shows when it reroutes a card out of a full hand.
            if (LocalContext.IsMe(player))
            {
                ThinkCmd.Play(new LocString("combat_messages", "HAND_FULL"), player.Creature, 2.0);
            }

            await AddToDiscard(choiceContext, player, upgraded);
            return;
        }

        await CardPileCmd.AddGeneratedCardToCombat(
            Create(player, upgraded), PileType.Hand, player, position);
    }

    /// <summary>
    /// Up to <paramref name="max"/> 水 cards in the order they are consumed: draw pile first, then
    /// hand, then discard pile. 水连击/水障壁 cash in a bounded handful of them instead of every
    /// copy the player has accumulated over the fight.
    /// </summary>
    public static List<CardModel> InConsumptionOrder(Player player, int max)
    {
        List<CardModel> result = [];
        if (max <= 0)
        {
            return result;
        }

        foreach (PileType type in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            foreach (CardModel card in type.GetPile(player).Cards)
            {
                if (card is not Water)
                {
                    continue;
                }

                result.Add(card);
                if (result.Count >= max)
                {
                    return result;
                }
            }
        }

        return result;
    }
}
