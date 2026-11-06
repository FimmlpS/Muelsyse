using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Models;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// Marks the temporary random cards that 遗尘 adds to a 漫步. The card UI draws a badge on them so the
/// player can tell which entries of the selection are illusory; the mark is dropped as soon as the
/// card is chosen (it then becomes a normal free card for the rest of the combat).
/// </summary>
public static class RemainDustMarker
{
    private static readonly ConditionalWeakTable<CardModel, object> Marks = new();
    private static readonly object Value = new();

    public static void Mark(CardModel card) => Marks.AddOrUpdate(card, Value);

    public static bool IsMarked(CardModel? card) => card != null && Marks.TryGetValue(card, out _);

    public static void Clear(CardModel card) => Marks.Remove(card);
}
