using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Cards.Attack;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

[Pool(typeof(MuelsyseCardPool))]
public class SongOfRose : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cultivate", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        .. HoverTipFactory.FromCardWithCardHoverTips<WhiteRose>()
    ];

    public SongOfRose() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        WhiteRose rose = combatState.CreateCard<WhiteRose>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(rose);
        }

        await CultivateCmd.Cultivate(choiceContext, Owner, rose,
            DynamicVars["Cultivate"].IntValue, this);
    }

    protected override void OnUpgrade() => DynamicVars["Cultivate"].UpgradeValueBy(1);
}
