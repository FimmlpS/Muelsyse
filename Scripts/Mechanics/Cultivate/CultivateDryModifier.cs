using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>失肥: the card has already been used as a cultivation material and cannot be used again.</summary>
public class CultivateDryModifier : CardModifier
{
    public static bool IsDry(CardModel card) => card.GetModifier<CultivateDryModifier>() != null;

    /// <summary>Restores a card that was dried out by an earlier cultivation.</summary>
    public static bool Remove(CardModel card)
    {
        CultivateDryModifier? modifier = card.GetModifier<CultivateDryModifier>();
        return modifier != null && CardModifier.RemoveModifier(card, modifier);
    }
}
