using MegaCrit.Sts2.Core.Entities.Powers;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// 塑形：生态耦合: future extra manifolds linger this many turns longer.
/// Purely bookkeeping, so it never appears in the power bar.
/// </summary>
public class EcologicalCouplingPower : AbstractMuelsysePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override string? CustomPackedIconPath =>
        "res://Muelsyse/images/powers/soil_back_power.png";

    public override string? CustomBigIconPath =>
        "res://Muelsyse/images/powers/soil_back_power.png";
}
