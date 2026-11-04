using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Mechanics;

namespace Muelsyse.Scripts.Powers;

/// <summary>生命涟漪: each living manifold washes block over itself and the player.</summary>
public class RipplePower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner.Player is not { } player)
        {
            return;
        }

        foreach (Creature manifold in ManifoldCmd.Alive(player))
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
            await CreatureCmd.GainBlock(manifold, Amount, ValueProp.Move, null);
        }
    }
}
