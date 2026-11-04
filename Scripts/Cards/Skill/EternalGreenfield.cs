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

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>永恒的绿地: a nutrient that grows every time it is used and never dries out.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class EternalGreenfield : AbstractMuelsyseCard, ICultivateNutrient, IWontDry, IOnCultivated
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Growth", 0)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Nutrient)
    ];

    public IEnumerable<CultivateCalculator> NutrientCalculators => [
        CultivateCalculator.Add(CultivateAim.Attack, DynamicVars["Growth"].IntValue),
        CultivateCalculator.Add(CultivateAim.Block, DynamicVars["Growth"].IntValue)
    ];

    public EternalGreenfield() : base(-1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public Task OnCultivated(PlayerChoiceContext choiceContext, Player player, bool isMain,
        CardModel? target)
    {
        DynamicVars["Growth"].BaseValue += 1;
        CultivateCmd.RefreshVisuals(this);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["Growth"].UpgradeValueBy(1);
}
