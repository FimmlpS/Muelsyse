using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 净水: the basic manifold steals Strength when it attacks and gives it back at the end of the turn.
/// </summary>
public class CleanWaterPower : AbstractMuelsysePower
{
    private readonly Dictionary<Creature, int> _stolen = new();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task Steal(PlayerChoiceContext choiceContext, Creature target)
    {
        // The steal runs after the manifold's damage lands, so a dead target here is one the attack
        // just killed: there is nothing left to take from.
        if (Owner.IsDead || target.IsDead || Amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, target, -Amount, Owner, null);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        _stolen[target] = _stolen.GetValueOrDefault(target) + Amount;
        Flash();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy)
        {
            return;
        }

        if (_stolen.Count == 0)
        {
            return;
        }

        // Nothing goes back to a target that died in the meantime, so its share must not be taken
        // out of the manifold either. The manifold also only ever gives back Strength it still has:
        // if something drained it below what it stole, the return stops at zero instead of running
        // the manifold negative. The amount handed over is therefore decided first, and only that
        // amount leaves the manifold.
        foreach ((Creature target, int amount) in _stolen)
        {
            if (target.IsDead)
            {
                continue;
            }

            int strength = target.GetPower<StrengthPower>()?.Amount ?? 0;
            int give = Math.Min(amount, -strength);
            if (give <= 0)
            {
                continue;
            }

            int available = Owner.IsDead ? 0 : Math.Max(0, Owner.GetPower<StrengthPower>()?.Amount ?? 0);
            int restore = Math.Min(give, available);
            if (restore <= 0)
            {
                continue;
            }

            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -restore, Owner, null);
            await PowerCmd.Apply<StrengthPower>(choiceContext, target, restore, Owner, null);
        }

        _stolen.Clear();
        Flash();
    }
}
