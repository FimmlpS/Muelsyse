using BaseLib.Patches.Hooks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// The hand limit, asked of BaseLib instead of being written as 10: <see cref="MaxHandSizePatch.GetMaxHandSize"/>
/// folds in every <c>IMaxHandSizeModifier</c> (this mod has none today, but another mod may add one),
/// and it is the same number the engine itself checks before letting a card into the hand.
/// </summary>
public static class HandLimit
{
    /// <summary>The largest number of cards this player's hand may hold.</summary>
    public static int Of(Player player) =>
        MaxHandSizePatch.GetMaxHandSize(player, CardPile.MaxCardsInHand);

    /// <summary>True when the player's hand already holds as many cards as it can.</summary>
    public static bool IsFull(Player player)
    {
        if (player.PlayerCombatState is not { } state)
        {
            return true;
        }

        return state.Hand.Cards.Count >= Of(player);
    }

    /// <summary>How many more cards fit into the player's hand (never negative).</summary>
    public static int FreeSpace(Player player)
    {
        if (player.PlayerCombatState is not { } state)
        {
            return 0;
        }

        return Math.Max(0, Of(player) - state.Hand.Cards.Count);
    }
}
