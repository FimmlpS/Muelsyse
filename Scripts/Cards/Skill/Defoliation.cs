using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
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

/// <summary>落叶: a free, reusable material that turns into energy.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Defoliation : AbstractMuelsyseCard, ICultivateExhaust, ICultivateUnlimited, IOnCultivated
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.ExhaustMaterial)
    ];

    public Defoliation() : base(-1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public async Task OnCultivated(PlayerChoiceContext choiceContext, Player player, bool isMain,
        CardModel? target)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, player);
    }

    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1);
}
