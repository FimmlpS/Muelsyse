using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Muelsyse.Scripts.Mechanics.Stroll;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 不定形态: at the start of every turn, 漫步. Unchosen cards are exhausted and replaced by a draw.
/// </summary>
public class UncertainFormPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner.Player)
        {
            return;
        }

        await StrollCmd.Stroll(choiceContext, player, Amount, this, remain: StrollRemain.ExhaustAndDraw);
    }
}
