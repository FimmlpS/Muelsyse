using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 散孢: whenever this creature is hit by an attack, every enemy takes damage equal to the pollen stacks.
/// </summary>
public class FlowerSpreadPower : AbstractFlowerPower
{
    public override PowerType Type => PowerType.Debuff;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (Owner == null || target != Owner || !IsAttack(props) || Amount <= 0)
        {
            return;
        }

        Flash();
        if (Owner.CombatState is { } combatState)
        {
            await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
        }
    }
}
