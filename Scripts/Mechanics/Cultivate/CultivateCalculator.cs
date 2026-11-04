namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>
/// A single cultivation step. Calculators are applied in order, exactly like StS1:
/// additive ones add to the running value, multiplicative ones scale it.
/// </summary>
public readonly record struct CultivateCalculator(CultivateAim Aim, bool IsMultiplicative, decimal Amount)
{
    public static CultivateCalculator Add(CultivateAim aim, decimal amount) => new(aim, false, amount);

    public static CultivateCalculator Multiply(CultivateAim aim, decimal factor) => new(aim, true, factor);
}
