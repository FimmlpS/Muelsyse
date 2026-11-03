using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Mechanics;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 阈值: the amount of attack damage a single manifold can absorb at once.
/// </summary>
public class ManifoldPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner || !props.IsPoweredAttack())
        {
            return decimal.MaxValue;
        }

        int threshold = ManifoldCmd.Threshold(Owner);
        return threshold > 0 ? threshold : decimal.MaxValue;
    }
}
