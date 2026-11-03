using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

[Pool(typeof(MuelsyseCardPool))]
public class Wave : AbstractMuelsyseCard
{
    public int WaterAmount => DynamicVars["Water"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Ethereal,
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public Wave() : base(-1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(1);
}
