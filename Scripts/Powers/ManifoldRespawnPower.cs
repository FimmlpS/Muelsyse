using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Muelsyse.Scripts.Mechanics;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// Marks the basic manifold. The basic manifold survives death and revives after two player turns.
/// </summary>
public class ManifoldRespawnPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldPowerBeRemovedAfterOwnerDeath()
    {
        return false;
    }

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        return creature != Owner;
    }

    public override bool ShouldAllowHitting(Creature creature)
    {
        return creature != Owner || Owner.IsAlive;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        Player? player = Owner.PetOwner;
        if (side != CombatSide.Enemy || player == null)
        {
            return;
        }

        if (Owner.IsAlive)
        {
            SetAmount(ManifoldCmd.BasicReviveTurns);
            return;
        }

        SetAmount(Amount - 1);
        if (Amount > 0)
        {
            return;
        }

        await ManifoldCmd.Revive(choiceContext, player, Owner);
    }
}
