namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// Marks a 协同 card whose buffs/debuffs are shared with every manifold.
/// Not every 协同 card hands out powers (小憩 also draws next turn), so the listener
/// must never infer this from the keyword alone.
/// </summary>
public interface ITogetherPowerCard
{
}
