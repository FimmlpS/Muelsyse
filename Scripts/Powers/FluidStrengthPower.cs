using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Muelsyse.Scripts.Cards.Attack;

namespace Muelsyse.Scripts.Powers;

/// <summary>流体力学: temporary strength, removed at the end of the player's turn.</summary>
public class FluidStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<FluidPower>();
}
