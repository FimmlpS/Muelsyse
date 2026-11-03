using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Skill;

[Pool(typeof(MuelsyseCardPool))]
public class GraduallyWater : AbstractMuelsyseCard
{
    private static readonly SavedSpireField<CardModel, int> Growth =
        new(_ => 0, "GraduallyWaterGrowth");

    private int Water => 1 + Growth.Get(this);

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public GraduallyWater() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        description.Add("Water", Water);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int water = Water;
        Growth.Set(this, Water);
        await LifeCircle.AddWater(choiceContext, Owner, water, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
