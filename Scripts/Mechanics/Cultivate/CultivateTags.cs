using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>养料: auto-filled first and contributes its calculators to the cultivated target.</summary>
public interface ICultivateNutrient
{
    IEnumerable<CultivateCalculator> NutrientCalculators { get; }
}

/// <summary>枯竭: does not count towards the material cap and exhausts after being used.</summary>
public interface ICultivateExhaust
{
}

/// <summary>不占用栽培上限。</summary>
public interface ICultivateUnlimited
{
}

/// <summary>永不失肥。</summary>
public interface IWontDry
{
}

/// <summary>Called when this card has just been consumed as a cultivation material.</summary>
public interface IOnCultivated
{
    Task OnCultivated(PlayerChoiceContext choiceContext, Player player, bool isMain, CardModel? target);
}
