using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>
/// 花粉: the four pollen cultivations (散孢 / 寄生 / 疗愈 / 水培) as a single random unit.
/// Pollen is planted directly on the combat copy of a card; it must never leak into the deck copy,
/// because it only lasts for this combat.
/// </summary>
public static class PollenCmd
{
    private static readonly CultivateAim[] Flowers =
    [
        CultivateAim.Spread,
        CultivateAim.Parasitism,
        CultivateAim.Cure,
        CultivateAim.WaterGrow
    ];

    public static CultivateAim RandomFlower(Player player)
    {
        return Flowers[player.RunState.Rng.CombatCardSelection.NextInt(Flowers.Length)];
    }

    public static void PlantRandom(CardModel target, int amount)
    {
        if (target.Owner is not { } player)
        {
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            CultivateCmd.AddBonus(target, RandomFlower(player), 1);
        }
    }
}
