using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Muelsyse.Scripts.Mechanics.Stroll;

/// <summary>
/// Called on a card that the player has just chosen during a 漫步. <c>first</c> is true for the
/// first card of the selection, which is the one 播种 / 时光的河 / 绿野幻梦 look at.
/// </summary>
public interface IOnStrolled
{
    Task OnStrolled(PlayerChoiceContext choiceContext, Player player, bool first);
}
