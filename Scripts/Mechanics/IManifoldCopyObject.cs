using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// Implemented by cards that modify the basic manifold itself. Applied whenever the basic manifold
/// is created or revived, so the effect is baked into the manifold rather than the player.
/// </summary>
public interface IManifoldCopyObject
{
    Task ApplyToBasicManifold(PlayerChoiceContext choiceContext, Creature basic);
}
