namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// Per-combat bookkeeping for 水. Several cards read how many Water cards have been exhausted during
/// the fight (在水散时相别), so the count lives here instead of on each card instance.
/// </summary>
public static class WaterLedger
{
    /// <summary>Water cards that have entered the exhaust pile this combat.</summary>
    public static int Exhausted { get; private set; }

    public static void RegisterExhausted()
    {
        Exhausted++;
    }

    public static void Reset()
    {
        Exhausted = 0;
    }
}
