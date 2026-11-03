using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// Marks an extra manifold and counts down the turns until it disappears.
/// </summary>
public class ManifoldDisappearPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy)
        {
            return;
        }

        if (!Owner.IsAlive)
        {
            return;
        }

        SetAmount(Amount - 1);
        if (Amount <= 0)
        {
            await CreatureCmd.Kill(Owner);
        }
    }
}
