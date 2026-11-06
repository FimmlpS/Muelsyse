using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>
/// 蒲公英: converts its printed block value into 润化. The number is deliberately not affected by
/// dexterity or other block powers — it is a conversion, not a block gain.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class Dandelion : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(1, ValueProp.Unpowered)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public Dandelion() : base(1, CardType.Skill, CardRarity.Token, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await LifeCircle.AddWater(choiceContext, Owner,
            (int)DynamicVars.Block.BaseValue, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(1);
}
