using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>
/// The total change one cultivation applies to a target card.
/// Attack/Block/Cost start from the target's current printed values because later
/// multiplicative calculators (精调, 化梦泡影) scale those values, not the delta.
/// </summary>
public sealed class CultivateBonus
{
    private readonly decimal _baseAttack;
    private readonly decimal _baseBlock;
    private readonly decimal _baseCost;
    private readonly bool _hasAttack;
    private readonly bool _hasBlock;
    private readonly bool _hasCost;

    private decimal _attack;
    private decimal _block;
    private decimal _cost;

    public CultivateBonus(CardModel card)
    {
        _hasAttack = TryGetAttackBase(card, out _baseAttack);
        _hasBlock = card.DynamicVars.TryGetValue("Block", out DynamicVar? block) && block.IntValue >= 0;
        _hasCost = !card.EnergyCost.CostsX && card.EnergyCost.GetWithModifiers(CostModifiers.None) >= 0;

        _attack = _baseAttack;
        _baseBlock = _block = _hasBlock ? card.DynamicVars["Block"].BaseValue : 0m;
        _baseCost = _cost = _hasCost ? card.EnergyCost.GetWithModifiers(CostModifiers.None) : 0m;
    }

    public decimal Draw { get; private set; }
    public decimal Energy { get; private set; }
    public decimal Water { get; private set; }
    public decimal Spread { get; private set; }
    public decimal Parasitism { get; private set; }
    public decimal Cure { get; private set; }
    public decimal WaterGrow { get; private set; }

    /// <summary>
    /// Every material also strengthens the target by one attack and one block. Cards without a
    /// printed damage/block value still record the stacks: the icon row shows them, and they can
    /// be carried onto another card when this one is consumed as a material.
    /// </summary>
    public void AddMaterialBase(int repetitions = 1)
    {
        _attack += repetitions;
        _block += repetitions;
    }

    public void Apply(CultivateCalculator calculator)
    {
        switch (calculator.Aim)
        {
            case CultivateAim.Attack:
                _attack = Next(_attack, calculator, _hasAttack);
                break;
            case CultivateAim.Block:
                _block = Next(_block, calculator, _hasBlock);
                break;
            case CultivateAim.Cost:
                if (_hasCost)
                {
                    _cost = Next(_cost, calculator);
                }
                break;
            case CultivateAim.Draw:
                Draw = Next(Draw, calculator);
                break;
            case CultivateAim.Energy:
                Energy = Next(Energy, calculator);
                break;
            case CultivateAim.Water:
                Water = Next(Water, calculator);
                break;
            case CultivateAim.Spread:
                Spread = Next(Spread, calculator);
                break;
            case CultivateAim.Parasitism:
                Parasitism = Next(Parasitism, calculator);
                break;
            case CultivateAim.Cure:
                Cure = Next(Cure, calculator);
                break;
            case CultivateAim.WaterGrow:
                WaterGrow = Next(WaterGrow, calculator);
                break;
        }
    }

    /// <summary>Copies the carried effects of a 枯竭 material onto this bonus.</summary>
    public void Carry(CultivateModifier source)
    {
        if (source.Attack != 0)
        {
            Apply(CultivateCalculator.Add(CultivateAim.Attack, source.Attack));
        }

        if (source.Block != 0)
        {
            Apply(CultivateCalculator.Add(CultivateAim.Block, source.Block));
        }

        Draw += source.Draw;
        Energy += source.Energy;
        Water += source.Water;
        Spread += source.Spread;
        Parasitism += source.Parasitism;
        Cure += source.Cure;
        WaterGrow += source.WaterGrow;
    }

    public int AttackDelta => (int)(_attack - _baseAttack);
    public int BlockDelta => (int)(_block - _baseBlock);
    public int CostDelta => (int)(_cost - _baseCost);

    public int DrawAmount => Clamp(Draw);
    public int EnergyAmount => Clamp(Energy);
    public int WaterAmount => Clamp(Water);
    public int SpreadAmount => Clamp(Spread);
    public int ParasitismAmount => Clamp(Parasitism);
    public int CureAmount => Clamp(Cure);
    public int WaterGrowAmount => Clamp(WaterGrow);

    public bool IsEmpty =>
        AttackDelta == 0 && BlockDelta == 0 && CostDelta == 0
        && DrawAmount == 0 && EnergyAmount == 0 && WaterAmount == 0
        && SpreadAmount == 0 && ParasitismAmount == 0 && CureAmount == 0 && WaterGrowAmount == 0;

    /// <summary>
    /// The printed value cultivation scales. Dynamic damage cards (始源) keep their base in
    /// <see cref="DynamicVarSet.CalculationBase" /> instead of a plain Damage var.
    /// </summary>
    private static bool TryGetAttackBase(CardModel card, out decimal value)
    {
        if (card.DynamicVars.TryGetValue("Damage", out DynamicVar? damage) && damage.IntValue >= 0)
        {
            value = damage.BaseValue;
            return true;
        }

        if (card.DynamicVars.TryGetValue("CalculatedDamage", out _)
            && card.DynamicVars.TryGetValue("CalculationBase", out DynamicVar? calculation)
            && calculation.IntValue >= 0)
        {
            value = calculation.BaseValue;
            return true;
        }

        value = 0m;
        return false;
    }

    /// <summary>
    /// A card without a printed attack/block value has nothing to scale, so a multiplicative
    /// calculator degrades to a flat addition there — otherwise ×2 on a 0 shows no icon at all.
    /// </summary>
    private static decimal Next(decimal current, CultivateCalculator calculator, bool hasBase)
    {
        if (!calculator.IsMultiplicative)
        {
            return current + calculator.Amount;
        }

        return hasBase ? current * calculator.Amount : current + calculator.Amount;
    }

    private static decimal Next(decimal current, CultivateCalculator calculator)
    {
        return calculator.IsMultiplicative ? current * calculator.Amount : current + calculator.Amount;
    }

    private static int Clamp(decimal value) => Math.Max(0, (int)value);
}
