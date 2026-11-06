using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

/// <summary>苏生: gain block whenever a 漫步 sends a card into the draw pile.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Revive : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Block", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll)
    ];

    public Revive() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RevivePower>(choiceContext, Owner.Creature,
            DynamicVars["Block"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
