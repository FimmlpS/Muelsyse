using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>新枝: three units of water and a blue hyacinth, already cultivated.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Shoot : AbstractMuelsyseCard, ICultivateCard
{
    private const int WaterCount = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cultivate", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        .. HoverTipFactory.FromCardWithCardHoverTips<BlueHyacinth>(IsUpgraded)
    ];

    public Shoot() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        for (int i = 0; i < WaterCount; i++)
        {
            await WaterCmd.AddToHand(choiceContext, Owner, IsUpgraded);
        }

        BlueHyacinth hyacinth = combatState.CreateCard<BlueHyacinth>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(hyacinth);
        }

        await CultivateCmd.Cultivate(choiceContext, Owner, hyacinth,
            DynamicVars["Cultivate"].IntValue, this);
    }

    protected override void OnUpgrade() => DynamicVars["Cultivate"].UpgradeValueBy(1);
}
