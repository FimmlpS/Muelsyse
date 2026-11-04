using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>花语: a free, reusable material that turns into water.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class WordOfFlower : AbstractMuelsyseCard, ICultivateExhaust, ICultivateUnlimited, IOnCultivated
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.ExhaustMaterial),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public WordOfFlower() : base(-1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
    }

    public async Task OnCultivated(PlayerChoiceContext choiceContext, Player player, bool isMain,
        CardModel? target)
    {
        await LifeCircle.AddWater(choiceContext, player, DynamicVars["Water"].IntValue, this);
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(2);
}
