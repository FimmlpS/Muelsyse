using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>笼中云杉: draws, cultivates a card and keeps it, then ends the turn.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class SpruceInCage : AbstractMuelsyseCard
{
    private const int CultivateAmount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Draw", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate)
    ];

    public SpruceInCage() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].IntValue, Owner);

        CardModel? target = await CultivateCmd.SelectTarget(choiceContext, Owner, this);
        if (target != null)
        {
            await CultivateCmd.Cultivate(choiceContext, Owner, target,
                CultivateAmount, this, retain: true);
        }

        PlayerCmd.EndTurn(Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1);
}
