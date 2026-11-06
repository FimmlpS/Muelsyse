namespace Muelsyse.Scripts.Mechanics.Stroll;

/// <summary>What happens to the reviewed cards that the player did not choose.</summary>
public enum StrollRemain
{
    /// <summary>They stay where they were (the discard pile).</summary>
    None,

    /// <summary>大地巡旅: they go to the hand.</summary>
    Hand,

    /// <summary>不定形态: each one is exhausted and draws a card.</summary>
    ExhaustAndDraw
}
