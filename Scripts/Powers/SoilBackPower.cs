using MegaCrit.Sts2.Core.Entities.Powers;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 土壤改造：回肥: the main cultivation material is not dried out this turn.
/// </summary>
public class SoilBackPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
