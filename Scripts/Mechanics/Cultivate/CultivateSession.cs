using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>
/// The live state of a cultivation while the main material selection is open.
/// The selection overlay polls this to draw the before/after preview.
/// </summary>
public sealed class CultivateSession
{
    public static CultivateSession? Active { get; set; }

    public CultivateSession(Player player, CardModel target, int amount, bool doubleMain, AbstractModel source)
    {
        Player = player;
        Target = target;
        Amount = amount;
        DoubleMain = doubleMain;
        Source = source;
        Bonus = new CultivateBonus(target);
    }

    public Player Player { get; }
    public CardModel Target { get; }
    public int Amount { get; }
    public bool DoubleMain { get; }
    public AbstractModel Source { get; }

    public CardModel? Main { get; private set; }
    public IReadOnlyList<CardModel> SubMaterials { get; private set; } = Array.Empty<CardModel>();
    public CultivateBonus Bonus { get; private set; }

    public void SetMain(CardModel? main)
    {
        if (main == Main)
        {
            return;
        }

        Main = main;
        if (main == null)
        {
            SubMaterials = Array.Empty<CardModel>();
            Bonus = new CultivateBonus(Target);
            return;
        }

        SubMaterials = CultivateCmd.AutoFill(Player, Target, main, Amount);
        Bonus = CultivateCmd.BuildBonus(Target, main, SubMaterials, DoubleMain);
    }
}
