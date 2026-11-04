using MegaCrit.Sts2.Core.Entities.Powers;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 水长: pure pollen payload. Other cards (水, 叶) read these stacks as bonus values.
/// </summary>
public class FlowerWaterPower : AbstractFlowerPower
{
    public override PowerType Type => PowerType.Buff;
}
