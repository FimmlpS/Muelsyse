using MegaCrit.Sts2.Core.Entities.Powers;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 土壤改造：散孢: every cultivation also plants this many stacks of 散孢 pollen on the target.
/// </summary>
public class SoilSpreadPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
