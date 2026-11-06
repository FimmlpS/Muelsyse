using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 苏生: whenever a 漫步 sends a card into the draw pile, gain block.
/// The trigger lives in <c>StrollCmd</c> — the user's rework limits the power to strolled cards only,
/// so it deliberately does not hook every card that enters the draw pile.
/// </summary>
public class RevivePower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public static async Task OnCardEnteredDraw(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature.GetPower<RevivePower>() is not { Amount: > 0 } power)
        {
            return;
        }

        power.Flash();
        await CreatureCmd.GainBlock(player.Creature, power.Amount, ValueProp.Unpowered, null, fast: true);
    }
}
