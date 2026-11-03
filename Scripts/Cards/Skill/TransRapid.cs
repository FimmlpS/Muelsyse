using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Skill;

[Pool(typeof(MuelsyseCardPool))]
public class TransRapid : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Strength", 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MuelsyseKeywords.Molding,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Molding)
    ];

    public TransRapid() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int strength = DynamicVars["Strength"].IntValue;
        ManifoldLinkPower link = await ManifoldCmd.EnsureLink(choiceContext, Owner);
        link.StrengthBonus += strength;

        foreach (Creature manifold in ManifoldCmd.Alive(Owner))
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, manifold, strength, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Strength"].UpgradeValueBy(1);
}
