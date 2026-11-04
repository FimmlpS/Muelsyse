using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// Flower pollen planted on a creature. All pollen halves (rounded down) at the start of its owner's turn.
/// </summary>
public abstract class AbstractFlowerPower : AbstractMuelsysePower
{
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || side != Owner.Side)
        {
            return;
        }

        int half = (int)Math.Floor(Amount / 2m);
        if (half > 0)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -half, Owner, null, silent: true);
        }
    }

    protected static bool IsAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}
