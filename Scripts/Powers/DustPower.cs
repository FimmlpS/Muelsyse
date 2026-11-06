using MegaCrit.Sts2.Core.Entities.Powers;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 遗尘: every 漫步 also shows this many free random cards. Unchosen ones are illusions and vanish.
/// </summary>
public class DustPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
