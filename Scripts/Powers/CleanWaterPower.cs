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

        int total = _stolen.Values.Sum();
        if (total > 0 && !Owner.IsDead)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -total, Owner, null);
        }

        foreach ((Creature target, int amount) in _stolen)
        {
            if (target.IsDead)
            {
                continue;
            }

            int strength = target.GetPower<StrengthPower>()?.Amount ?? 0;
            int restore = Math.Min(amount, Math.Max(0, -strength));
            if (restore > 0)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, target, restore, Owner, null);
            }
        }

        _stolen.Clear();
        Flash();
    }
}
