using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 寄生: whenever this creature is hit by an attack, the attacker gains block equal to the pollen stacks.
/// </summary>
public class FlowerParasitismPower : AbstractFlowerPower
{
    public override PowerType Type => PowerType.Debuff;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (Owner == null || target != Owner || dealer == null || !IsAttack(props) || Amount <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(dealer, Amount, ValueProp.Unpowered, null, fast: true);
    }
}
