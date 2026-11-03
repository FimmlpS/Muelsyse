using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Skill;

[Pool(typeof(MuelsyseCardPool))]
public class WaterDefend : AbstractMuelsyseCard
{
    private int _firstWord;

    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Together];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? [
            HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
            HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
            HoverTipFactory.FromKeyword(MuelsyseKeywords.FirstWord)
        ]
        : [
            HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
            HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
        ];

    public WaterDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    public override Task BeforeCombatStart()
    {
        _firstWord = IsUpgraded ? 1 : 0;
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        if (_firstWord > 0)
        {
            _firstWord--;
            await LifeCircle.AddWater(choiceContext, Owner, 1, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        _firstWord = 1;
    }
}
