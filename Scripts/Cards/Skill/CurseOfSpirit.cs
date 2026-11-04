using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>精灵的诅咒: a small heal now, and a lasting max-HP gift if it ever leaves the deck.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class CurseOfSpirit : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHp", 6)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public CurseOfSpirit() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, 2m);
    }

    public override async Task BeforeCardRemoved(CardModel card)
    {
        if (card != this || Owner == null)
        {
            return;
        }

        await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars["MaxHp"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["MaxHp"].UpgradeValueBy(4);
}
