using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Muelsyse.Scripts.Cards;

public abstract class AbstractMuelsyseCard : CustomCardModel
{
    public override string PortraitPath => $"res://Muelsyse/images/cards/{replaceID(Id.Entry.ToLowerInvariant())}.png";

    public string replaceID(string path) => path.Replace("muelsyse-", "");

    protected AbstractMuelsyseCard(int baseCost, CardType type, CardRarity rarity, TargetType target)
        : base(baseCost, type, rarity, target, true, true)
    {
    }

    protected AbstractMuelsyseCard(int baseCost, CardType type, CardRarity rarity, TargetType target,
        bool showInCardLibrary, bool autoAdd)
        : base(baseCost, type, rarity, target, showInCardLibrary, autoAdd)
    {
    }
}
