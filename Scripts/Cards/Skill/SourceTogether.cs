using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>溯途同归: spends all energy for a flood of water.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class SourceTogether : AbstractMuelsyseCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 4)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public SourceTogether() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int water = ResolveEnergyXValue() * DynamicVars["Water"].IntValue;
        if (water > 0)
        {
            await LifeCircle.AddWater(choiceContext, Owner, water, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(1);
}
