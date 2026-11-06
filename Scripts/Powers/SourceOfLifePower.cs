using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Muelsyse.Scripts.Mechanics;

namespace Muelsyse.Scripts.Powers;

/// <summary>生命之源: at the start of every turn, add upgraded 水 to the hand.</summary>
public class SourceOfLifePower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        for (int i = 0; i < Amount; i++)
        {
            await WaterCmd.AddToHand(choiceContext, player, upgraded: true);
        }
    }
}
