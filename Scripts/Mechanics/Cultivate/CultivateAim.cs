namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>
/// What a cultivation calculator strengthens. The numeric values match StS1's
/// <c>CultivatePatch.Calculator.aimType</c> so ported card data reads identically.
/// </summary>
public enum CultivateAim
{
    Attack = 1,
    Block = 2,
    Cost = 3,
    Draw = 4,
    Energy = 5,
    Water = 6,
    Spread = 7,
    Parasitism = 8,
    Cure = 9,
    WaterGrow = 10,
}
