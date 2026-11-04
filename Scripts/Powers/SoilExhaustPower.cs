using MegaCrit.Sts2.Core.Entities.Powers;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 土壤改造：净化: statuses and curses become unlimited 枯竭 materials, and each exhausted material draws cards.
/// </summary>
public class SoilExhaustPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
